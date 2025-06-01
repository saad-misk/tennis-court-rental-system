using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TennisCourtRentalSystem.Models;
using TennisCourtRentalSystem.Repositories;

namespace TennisCourtRentalSystem.Controllers;

public class CourtController : Controller
{
    private readonly CourtRepository _courtRepo;

    public CourtController(CourtRepository courtRepo)
    {
        _courtRepo = courtRepo;
    }

    public IActionResult AvailableCourts(List<Court> courts)
    {
        return View(courts);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult ManageCourts()
    {
        var courts = _courtRepo.GetAllCourts();
        return View(courts);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult CreateCourt()
    {
        return View(new Court());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateCourt(Court model)
    {
        if (ModelState.IsValid)
        {
            _courtRepo.AddCourt(model);
            return RedirectToAction("ManageCourts");
        }
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult EditCourt(int id)
    {
        var court = _courtRepo.GetCourtByNumber(id);
        return View(court);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult EditCourt(Court model)
    {
        if (ModelState.IsValid)
        {
            _courtRepo.UpdateCourt(model);
            return RedirectToAction("ManageCourts");
        }
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult DeleteCourt(int id)
    {
        _courtRepo.DeleteCourt(id);
        return RedirectToAction("ManageCourts");
    }

    public IActionResult Availability(DateTime date, DateTime startTime, DateTime endTime)
    {
        var availability = _courtRepo.GetAvailableCourts(date, startTime, endTime);

        return View(availability);
    }
    
}