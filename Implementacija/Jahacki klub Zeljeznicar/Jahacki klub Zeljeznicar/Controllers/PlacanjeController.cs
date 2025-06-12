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
                return RedirectToAction("Index", "Clanarina");
            }

            ViewBag.Mjeseci = mjeseci;
            return View("~/Views/Clanarina/Placanje.cshtml");
        }

        [HttpPost]
        [Authorize(Policy = "ClanOnly")]
        public async Task<IActionResult> PotvrdiPlacanje(string BrojKartice, int Opcija)
        {
            if (string.IsNullOrWhiteSpace(BrojKartice) || (Opcija != 1 && Opcija != 6 && Opcija != 12))
            {
                TempData["Error"] = "Unos nije ispravan.";
                return RedirectToAction("Index", new { mjeseci = Opcija });
            }

            return RedirectToAction("Potvrdi", "Clanarina", new { mjeseci = Opcija });
        }
    }
}
