using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using TennisCourtRentalSystem.Models;
using TennisCourtRentalSystem.Repositories;

namespace TennisCourtRentalSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly CustomerRepository _customerRepo;
        private readonly CourtManagerRepository _managerRepo;

        public AccountController(CustomerRepository customerRepo, CourtManagerRepository managerRepo)
        {
            _customerRepo = customerRepo;
            _managerRepo = managerRepo;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var hashed = HashPassword(password);
            var customer = _customerRepo.GetCustomerByUsername(username);

            if (customer != null && customer.PasswordHash == hashed)
            {
                HttpContext.Session.SetString("UserType", "Customer");
                HttpContext.Session.SetInt32("UserId", customer.CustomerId);
                return RedirectToAction("Dashboard", "Customer");
            }

            var manager = _managerRepo.GetManagerByUsername(username);
            if (manager != null && manager.PasswordHash == hashed)
            {
                HttpContext.Session.SetString("UserType", "Manager");
                HttpContext.Session.SetInt32("UserId", manager.EmployeeId);
                return RedirectToAction("Dashboard", "CourtManager");
            }

            ViewBag.Error = "Invalid credentials.";
            return View();
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(Customer customer, string password)
        {
            customer.PasswordHash = HashPassword(password);
            customer.DateCreated = DateTime.Now;
            _customerRepo.CreateCustomer(customer);

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
