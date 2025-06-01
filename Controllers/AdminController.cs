using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TennisCourtRentalSystem.Repositories;

namespace TennisCourtRentalSystem.Controllers;

public class AdminController : Controller
{

    private readonly RentalRepository _rentalRepo;
    private readonly CustomerRepository _customerRepo;
    private readonly CourtRepository _courtRepo;
    private readonly CourtManagerRepository _courtManagerRepo;

    public AdminController(
        RentalRepository rentalRepository,
        CustomerRepository customerRepository,
        CourtRepository courtRepository,
        CourtManagerRepository courtManagerRepository
    )
    {
        _rentalRepo = rentalRepository;
        _customerRepo = customerRepository;
        _courtRepo = courtRepository;
        _courtManagerRepo = courtManagerRepository;
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Dashboard()
    {
        ViewBag.TotalCourts = _courtRepo.GetCourtCount() ?? 0;
        ViewBag.ActiveRentals = _rentalRepo.GetActiveRentalCount() ?? 0;
        
        ViewBag.DailyRevenue = _rentalRepo.GetDailyRevenue();  
        ViewBag.CurrentUsers = _rentalRepo.GetActiveUserCount();
        ViewBag.RecentActivities = _courtManagerRepo.GetRecentActivities();
        ViewBag.Courts = _courtRepo.GetAllCourts();

        return View();
    }

    [Authorize(Roles = "Admin")]
    public IActionResult ActiveUsers()
    {
        var currentTime = DateTime.Now;
        var activeRentals = _rentalRepo.GetActiveRentals(currentTime);
        return View(activeRentals);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult CourtAvailability(DateTime date, DateTime startTime, DateTime endTime)
    {
        var availability = _courtRepo.GetAvailableCourts(
            date,
            startTime,
            endTime
        );
        return View(availability);
    }

}