using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TennisCourtRentalSystem.Models;
using TennisCourtRentalSystem.Repositories;

namespace TennisCourtRentalSystem.Controllers;

public class RentalController : Controller
{
    private readonly RentalRepository _rentalRepo;
    private readonly CustomerRepository _customerRepo;
    private readonly CourtRepository _courtRepo;
    private readonly CourtManagerRepository _courtManagerRepo;

    public RentalController(
        RentalRepository rentalRepo,
        CustomerRepository customerRepo,
        CourtRepository courtRepo,
        CourtManagerRepository courtManagerRepo
        )
    {
        _rentalRepo = rentalRepo;
        _customerRepo = customerRepo;
        _courtRepo = courtRepo;
        _courtManagerRepo = courtManagerRepo;
    }

    [Authorize(Roles = "Customer")]
    public IActionResult Create(int courtNumber, DateOnly rentalDate, TimeOnly startTime, TimeSpan duration)
    {
        var customer = _customerRepo.GetCustomerByUsername(User.Identity.Name);
        var court = _courtRepo.GetCourtByNumber(courtNumber);

        var model = new Rental
        {
            CourtNumber = courtNumber,
            CustomerId = customer.CustomerId,
            RentalDate = DateTime.Parse(rentalDate.ToString("yyyy-MM-dd")),
            StartTime =  DateTime.Parse(startTime.ToString("HH:mm")),
            EndTime = DateTime.Parse(duration.Add(startTime.ToTimeSpan()).ToString("HH:mm")),
            RentalFee = CalculateFee(customer.CustomerType, duration),
            PaymentStatus = "Pending",
            DateBooked = DateTime.UtcNow
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public IActionResult Create(Rental rental)
    {
        if (ModelState.IsValid)
        {
            // Double-check court availability
            var isAvailable = _courtRepo.GetAvailableCourts(
                rental.RentalDate,
                rental.StartTime,
                rental.EndTime
            ).Any(c => c.CourtNumber.ToString() == rental.CourtNumber.ToString());

            if (isAvailable)
            {
                _rentalRepo.CreateRental(rental);
                return RedirectToAction("MyRentals", "Customer");
            }
            ModelState.AddModelError("", "Court is no longer available.");
        }
        return View(rental);
    }
    
    [Authorize(Roles = "Customer")]
    public IActionResult Edit(int id)
    {
        var rental = _rentalRepo.GetRentalById(id);
        if (rental == null || rental.CustomerId != GetCurrentCustomerId())
        {
            return Forbid();
        }
        return View(rental);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public IActionResult Edit(Rental model)
    {
        if (ModelState.IsValid && model.CustomerId == GetCurrentCustomerId())
        {
            var isAvailable = _courtRepo.GetAvailableCourts(
                model.RentalDate,
                model.StartTime,
                model.EndTime
            ).Any(c => c.CourtNumber.ToString() == model.CourtNumber.ToString());

            if (isAvailable)
            {
                _rentalRepo.UpdateRental(model);
                return RedirectToAction("MyRentals");
            }
            ModelState.AddModelError("", "Court is no longer available.");
        }
        return View(model);
    }

    [Authorize(Roles = "Customer")]
    public IActionResult Delete(int id)
    {
        var rental = _rentalRepo.GetRentalById(id);
        if (rental == null || rental.CustomerId != GetCurrentCustomerId())
        {
            return Forbid();
        }
        return View(rental);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Customer")]
    public IActionResult DeleteConfirmed(int id)
    {
        _rentalRepo.DeleteRental(id);
        return RedirectToAction("MyRentals");
    }

    private string GetCurrentCustomerId()
    {
        var username = User.Identity.Name;
        return _customerRepo.GetCustomerByUsername(username).CustomerId;
    }

    public IActionResult Bill(string id)
    {
        var rental = _rentalRepo.GetRentalWithDetails(id);
        if (rental == null)
        {
            return Forbid();
        }
        return View(rental);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult UpdatePayment(int id, string status)
    {
        _rentalRepo.UpdatePaymentStatus(id, status);
        return RedirectToAction("Details", new { id });
    }

    private decimal CalculateFee(int CustomerType, TimeSpan duration)
    {
        if (CustomerType == 1)
        {
            return (int)duration.TotalHours * 10;
        }
        else
        {
            return (int)duration.TotalHours * 20;
        }
    }
}