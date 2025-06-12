using Jahacki_klub_Zeljeznicar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Jahacki_klub_Zeljeznicar.Controllers
{
    [Authorize]
    public class PlacanjeController : Controller
    {
        private readonly UserManager<User> _userManager;

        public PlacanjeController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Policy = "ClanOnly")]
        public IActionResult Index(int mjeseci)
        {
            if (mjeseci != 1 && mjeseci != 6 && mjeseci != 12)
            {
                ModelState.AddModelError(nameof(mjeseci), "Nevažeći izbor trajanja članarine.");
                return RedirectToAction("Index", "Clanarina");
            }

            ViewBag.Mjeseci = mjeseci;
            return View("~/Views/Clanarina/Placanje.cshtml");
        }

        [HttpPost]
        [Authorize(Policy = "ClanOnly")]
        public async Task<IActionResult> PotvrdiPlacanje(string brojKartice, int opcija)
        {
            // Validate credit card number (basic validation)
            if (string.IsNullOrWhiteSpace(brojKartice))
            {
                ModelState.AddModelError(nameof(brojKartice), "Broj kartice je obavezan.");
            }
            else if (brojKartice.Length < 13 || brojKartice.Length > 19)
            {
                ModelState.AddModelError(nameof(brojKartice), "Broj kartice mora imati između 13 i 19 znamenki.");
            }
            else if (!long.TryParse(brojKartice, out _))
            {
                ModelState.AddModelError(nameof(brojKartice), "Broj kartice može sadržavati samo znamenke.");
            }

            // Validate membership option
            if (opcija != 1 && opcija != 6 && opcija != 12)
            {
                ModelState.AddModelError(nameof(opcija), "Nevažeći izbor trajanja članarine.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Popravite greške u obrascu.";
                return RedirectToAction("Index", new { mjeseci = opcija });
            }

            return RedirectToAction("Potvrdi", "Clanarina", new { mjeseci = opcija });
        }
    }
}