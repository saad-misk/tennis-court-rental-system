using Microsoft.AspNetCore.Mvc;
using TennisCourtRentalSystem.Repositories;

namespace TennisCourtRentalSystem.Controllers
{
    public class CourtController : Controller
    {
        private readonly CourtRepository _courtRepository;
        private readonly RentalRepository _rentalRepository;

        public CourtController(CourtRepository courtRepository, RentalRepository rentalRepository)
        {
            _courtRepository = courtRepository;
            _rentalRepository = rentalRepository;
        }

        // GET: Court
        public IActionResult Index()
        {
            var courts = _courtRepository.GetAllCourts();
            return View("~/Views/Court/courts.cshtml", courts);
        }

        // GET: Court/Available/5
        public IActionResult Available(int courtNumber)
        {
            var court = _courtRepository.GetCourtByNumber(courtNumber);
            if (court == null)
            {
                return NotFound();
            }
            return View(court);
        }

        public IActionResult Courts() => View();  // Views/Court/courts.cshtml  

        // POST: Court/CheckAvailability
        [HttpPost]
        public IActionResult CheckAvailability(int courtNumber, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            // Convert to DateOnly/TimeOnly (requires .NET 6+)
            var dateOnly = DateOnly.FromDateTime(date);
            var startTimeOnly = TimeOnly.FromTimeSpan(startTime);
            var endTimeOnly = TimeOnly.FromTimeSpan(endTime);

            // Check for conflicts - You'll need to implement this in RentalRepository
            var isAvailable = !_rentalRepository.HasConflict(courtNumber, dateOnly, startTimeOnly, endTimeOnly);

            ViewBag.IsAvailable = isAvailable;
            ViewBag.SelectedDate = date;
            ViewBag.StartTime = startTime;
            ViewBag.EndTime = endTime;
            
            var court = _courtRepository.GetCourtByNumber(courtNumber);
            return View("Available", court);
        }

        // POST: Court/UpdateStatus
        [HttpPost]
        public IActionResult UpdateStatus(int courtNumber, string newStatus)
        {
            _courtRepository.UpdateCourtStatus(courtNumber, newStatus);
            return RedirectToAction(nameof(Index));
        }
    }
}