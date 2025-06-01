using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TennisCourtRentalSystem.Models;
using TennisCourtRentalSystem.Models.ViewModels;
using TennisCourtRentalSystem.Repositories;
using Microsoft.AspNetCore.Identity;

namespace TennisCourtRentalSystem.Controllers;

public class AccountController : Controller
{
    private readonly CustomerRepository _customerRepo;
    private readonly AddressRepository _addressRepo;
    private readonly CourtManagerRepository _courtManager;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AccountController(
        CustomerRepository customerRepo,
        AddressRepository addressRepo,
        CourtManagerRepository courtManager,
        IPasswordHasher<User> passwordHasher)
    {
        _customerRepo = customerRepo;
        _addressRepo = addressRepo;
        _courtManager = courtManager;
        _passwordHasher = passwordHasher;
    }

    // REGISTER
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (_customerRepo.GetCustomerByUsername(model.UserName) != null)
            {
                ModelState.AddModelError("UserName", "Username is taken.");
                return View(model);
            }

            var address = new Address
            {
                State = model.State,
                City = model.City,
                Street = model.Street
            };
            string addressId = _addressRepo.CreateAddress(address);

            var customer = new Customer
            {
                UserName = model.UserName,
                // PasswordHash = _passwordHasher.HashPassword(new User(), model.Password),
                PasswordHash = model.Password,
                Email = model.Email,
                DateCreated = DateTime.UtcNow,
                TelNo = model.TelNo,
                CustomerType = model.CustomerType,
                AddressId = addressId,
                Gender = model.Gender,
                OrganizationName = model.OrganizationName
            };

            _customerRepo.CreateCustomer(customer);

            // Automatically log in the new user
            await SignInUser(customer, "Customer");
            return RedirectToAction("Dashboard", "Customer");
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var admin = _courtManager.GetManagerByUsername(model.UserName);
            if (admin != null)
            {
                // if (_passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, model.Password) == 
                //     PasswordVerificationResult.Success)
                // {
                //     await SignInUser(admin, "Admin");
                //     return RedirectToAction("Dashboard", "Admin");
                // }
                if (admin.PasswordHash == model.Password)
                {
                    await SignInUser(admin, "Admin"); // Admin
                    return RedirectToAction("Dashboard", "Admin"); // Admin
                }
            }

            var customer = _customerRepo.GetCustomerByUsername(model.UserName);
            if (customer != null)
            {
                // if (_passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, model.Password) == 
                //     PasswordVerificationResult.Success)
                // {
                //     await SignInUser(customer, "Customer");
                //     return RedirectToAction("Dashboard", "Customer");
                // }
                if (customer.PasswordHash == model.Password)
                {
                    await SignInUser(customer, "Customer"); // Customer
                    return RedirectToAction("Dashboard", "Customer"); // Customer
                }
            }

            ModelState.AddModelError("", "Invalid credentials.");
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Account");
    }

    private async Task SignInUser(User user, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}