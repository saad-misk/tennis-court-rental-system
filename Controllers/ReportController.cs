using Microsoft.AspNetCore.Mvc;
using TennisCourtRentalSystem.Models;
using TennisCourtRentalSystem.Repositories;

namespace TennisCourtRentalSystem.Controllers
{
    public class ReportController : Controller
    {
        private readonly RentalRepository _rentalRepository;
        private readonly CustomerRepository _customerRepository;
        private readonly CourtRepository _courtRepository;

        public ReportController(
            RentalRepository rentalRepository,
            CustomerRepository customerRepository,
            CourtRepository courtRepository)
        {
            _rentalRepository = rentalRepository;
            _customerRepository = customerRepository;
            _courtRepository = courtRepository;
        }

        // Active Rentals
        public IActionResult ActiveRentals(string status = "Pending")
        {
            ViewBag.StatusList = new List<string> { "Pending", "Paid", "Cancelled" };
            var rentals = _rentalRepository.GetByStatus(status);
            return View(rentals);
        }

        // Court Availability
        public IActionResult CourtAvailability(DateTime? date)
        {
            date ??= DateTime.Today;
            ViewBag.SelectedDate = date;
            
            var courts = _courtRepository.GetAllCourts();
            foreach (var court in courts)
            {
                court.CourtStatus = _rentalRepository.IsCourtAvailable(court.CourtNumber, date.Value) 
                    ? "Available" 
                    : "Booked";
            }
            return View(courts);
        }

        // Customer List
        public IActionResult CustomerList()
        {
            var customers = _customerRepository.GetAllWithRentals();
            return View(customers);
        }

        // Generate Bill
        public IActionResult GenerateBill(int id)
        {
            var rental = _rentalRepository.GetRentalsByCustomer(id);
            return View(rental);
        }

        // Revenue Report
        public IActionResult Revenue(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null)
            {
                ViewBag.ReportGenerated = false;
                return View(0m);
            }

            var totalRevenue = _rentalRepository.GetRevenueByDateRange(startDate.Value, endDate.Value);
            ViewBag.ReportGenerated = true;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            return View(totalRevenue);
        }
    }
}