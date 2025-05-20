// Controllers/CustomersController.cs
using Microsoft.AspNetCore.Mvc;
using TennisCourtRentalSystem.Models;
using TennisCourtRentalSystem.Repositories;

namespace TennisCourtRentalSystem.Controllers
{
    public class CustomersController : Controller
    {
        private readonly CustomerRepository _customerRepository;

        public CustomersController(CustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        // GET: Customers/Create
        // public IActionResult Create()
        // {
        //     return View(); // Looks for Views/Customers/Create.cshtml
        // }

        // POST: Customers/Create
        // [HttpPost]
        // public IActionResult Create(Customer customer)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         // Add to database
        //         return RedirectToAction("Customers");
        //     }
        //     return View(customer);
        // }

        public IActionResult Create() => View();
        public IActionResult Edit(int id) => View();
        public IActionResult Details(int id) => View();
    }
}