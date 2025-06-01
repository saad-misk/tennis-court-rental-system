using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TennisCourtRentalSystem.Repositories;

namespace TennisCourtRentalSystem.Controllers;

public class ReportController : Controller
{
    private readonly RentalRepository _rentalRepo;
    private readonly CustomerRepository _customerRepo;
    private readonly CourtManagerRepository _courtmngrRepo;

    private readonly CourtRepository _courtRepo;

    public ReportController(
        RentalRepository rentalRepository,
        CustomerRepository customerRepository,
        CourtRepository courtRepository,
        CourtManagerRepository courtManagerRepository)
    {
        _rentalRepo = rentalRepository;
        _customerRepo = customerRepository;
        _courtRepo = courtRepository;
        _courtmngrRepo = courtManagerRepository;
    }

    [Authorize(Roles = "Admin")]
    public IActionResult ActiveRentals()
    {
        var activeRentals = _rentalRepo.GetActiveRentals(DateTime.Now);
        return View(activeRentals);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AvailabilityStatus(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        var courts = _courtRepo.GetCourtRentalStatus(date, startTime, endTime);
        return View(courts);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult CourtRevenue(int courtNumber)
    {

        var rentals = _courtmngrRepo.GetRentalsForCourt(courtNumber);

        return View(rentals);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult CurrentUsage()
    {
        var currentUsage = _rentalRepo.GetCurrentCourtUsage();
        return View(currentUsage);
    }

}