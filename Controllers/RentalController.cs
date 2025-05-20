using Microsoft.AspNetCore.Mvc;

public class RentalController : Controller
{
    public IActionResult Create() => View(); // Views/Rental/Create.cshtml
    public IActionResult Edit(int id) => View();   // Views/Rental/Edit.cshtml
    public IActionResult Delete(int id) => View(); // Views/Rental/Delete.cshtml
    public IActionResult MyRentals() => View();    // Views/Rental/MyRentals.cshtml
}