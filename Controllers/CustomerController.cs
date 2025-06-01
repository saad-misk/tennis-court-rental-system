using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TennisCourtRentalSystem.Models;
using TennisCourtRentalSystem.Models.ViewModels;
using TennisCourtRentalSystem.Repositories;

namespace TennisCourtRentalSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CustomerRepository _customerRepo;
        private readonly RentalRepository _rentalRepo;
        private readonly CourtRepository _courtRepo;

        public CustomerController(CustomerRepository customerRepository, RentalRepository rentalRepository, CourtRepository courtRepository)
        {
            _customerRepo = customerRepository;
            _rentalRepo = rentalRepository;
            _courtRepo = courtRepository;
        }

        public IActionResult Dashboard()
        {
            var username = User.Identity.Name;
            var customer = _customerRepo.GetCustomerByUsername(username);

            if (customer == null)
            {
                return RedirectToAction("Logout", "Account");
            }

            var allRentals = _rentalRepo.GetRentalsByCustomerId(customer.CustomerId);
            var now = DateTime.Now;

            var upcomingRentals = allRentals
                .Where(r => r.RentalDate > now)
                .OrderBy(r => r.RentalDate)
                .ToList();

            var pastRentals = allRentals
                .Where(r => r.RentalDate <= now)
                .OrderByDescending(r => r.RentalDate)
                .ToList();

            var viewModel = new CustomerDashboardViewModel
            {
                Customer = customer,
                ActiveRentalsCount = upcomingRentals.Count(r => r.PaymentStatus == "Paid"),
                UpcomingRentalsCount = upcomingRentals.Count,
                TotalSpent = pastRentals.Sum(r => r.RentalFee),
                UpcomingRentals = upcomingRentals,
                PastRentals = pastRentals
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult SortedCustomers()
        {
            var customers = _customerRepo.GetAllCustomers()
                            .OrderBy(c => c.UserName)
                            .ToList();
            return View(customers);
        }

        [HttpGet]
        public IActionResult CourtRentalRequest()
        {
            var courts = _courtRepo.GetAvailableCourts();

            var model = new CourtRentalRequestViewModel
            {
                RentalDate = DateTime.Today,
                StartTime = DateTime.Today.AddHours(8),
                Duration = "1:00",
                AvailableCourts = courts.Select(c => new CourtRentalRequestViewModel.CourtSelectionViewModel
                {
                    CourtNumber = c.CourtNumber,
                    Description = $"{c.Location} - Status: {c.CourtStatus}",
                    IsSelected = false
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult GetAvailableCourts(DateTime rentalDate, DateTime startTime, string duration)
        {
            if (string.IsNullOrWhiteSpace(duration))
                return Json(new { success = false, message = "Please select a duration." });

            try
            {
                var (start, end) = ParseDuration(startTime, duration);

                var fullStartTime = rentalDate.Date.Add(start.TimeOfDay);
                var fullEndTime = rentalDate.Date.Add(end.TimeOfDay);
                if (fullEndTime <= fullStartTime)
                    fullEndTime = fullEndTime.AddDays(1);

                var availableCourts = _courtRepo.GetAvailableCourts(rentalDate, fullStartTime, fullEndTime);

                var courtsViewModel = availableCourts.Select(c => new CourtRentalRequestViewModel.CourtSelectionViewModel
                {
                    CourtNumber = c.CourtNumber,
                    Description = c.Location
                }).ToList();

                return Json(new { success = true, courts = courtsViewModel });
            }
            catch (Exception ex)
            {
                // You may optionally log ex here
                return Json(new { success = false, message = "An error occurred while checking availability. Please try again." });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> SubmitRentalRequest(CourtRentalRequestViewModel model)
        {
            ModelState.Remove(nameof(model.AvailableCourts));

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please ensure all fields are filled correctly and a court is selected.";
                return View("CourtRentalRequest", model);
            }

            var customer = _customerRepo.GetCustomerByUsername(User.Identity.Name);
            if (customer == null)
            {
                ModelState.AddModelError("", "Unable to identify customer.");
                return View("CourtRentalRequest", model);
            }

            var (startTime, endTime) = ParseDuration(model.StartTime, model.Duration);

            var fullStartTime = model.RentalDate.Date.Add(startTime.TimeOfDay);
            var fullEndTime = model.RentalDate.Date.Add(endTime.TimeOfDay);
            if (fullEndTime <= fullStartTime)
                fullEndTime = fullEndTime.AddDays(1);

            var courtNumber = model.SelectedCourtNumber.Value;

            if (!_courtRepo.IsCourtAvailable(courtNumber, model.RentalDate, fullStartTime, fullEndTime))
            {
                TempData["ErrorMessage"] = $"Sorry, Court {courtNumber} is no longer available. Please search again.";
                return View("CourtRentalRequest", new CourtRentalRequestViewModel());
            }

            var totalCost = CalculateTotalCost(customer.CustomerType, model.Duration, 1);

            var rental = new Rental
            {
                CustomerId = customer.CustomerId,
                RentalDate = model.RentalDate.Date,
                StartTime = fullStartTime,
                EndTime = fullEndTime,
                RentalFee = totalCost,
                CourtNumber = courtNumber,
                PaymentStatus = "Pending"
            };

            _rentalRepo.CreateRental(rental);

            return RedirectToAction("Confirmation", new { id = rental.RentalId });
        }

        public IActionResult Confirmation(int id)
        {
            ViewBag.Message = $"Your booking (ID: {id}) is confirmed (Pending Payment)!";
            return View();
        }


        private (DateTime start, DateTime end) ParseDuration(DateTime startTime, string duration)
        {
            var timeSpan = TimeSpan.Parse(duration);
            return (startTime, startTime.Add(timeSpan));
        }

        private decimal CalculateTotalCost(int customerType, string duration, int courtCount)
        {
            var timeSpan = TimeSpan.Parse(duration);
            var hours = timeSpan.TotalHours;
            var rate = customerType == 1 ? 10 : 20; // Resident/Non-resident rates
            return (decimal)(hours * rate * courtCount);
        }

        [Authorize(Roles = "Customer")]
        public IActionResult MyRentals()
        {
            var customer = _customerRepo.GetCustomerByUsername(User.Identity.Name);
            var rentals = _rentalRepo.GetRentalsByCustomerId(customer.CustomerId);
            return View(rentals);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Customers()
        {
            var customers = _customerRepo.GetAllCustomers();
            return View(customers);
        }
    }
}