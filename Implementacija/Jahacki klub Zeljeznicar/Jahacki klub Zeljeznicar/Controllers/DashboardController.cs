using Jahacki_klub_Zeljeznicar.Data;
using Jahacki_klub_Zeljeznicar.Models;
using Jahacki_klub_Zeljeznicar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Jahacki_klub_Zeljeznicar.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Fix: Ensure the `User` type is used instead of `IdentityUser` when accessing properties like `Kategorija`.

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            Debug.WriteLine("TU SAM");
            Debug.WriteLine(currentUser);

            if (currentUser == null)
            {
                Debug.WriteLine("TU SAM");
                return RedirectToAction("Login", "Account");
            }

            // Explicitly cast or retrieve the user as the `User` type instead of `IdentityUser`
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == currentUser.UserName);

            Debug.WriteLine("TU SAM 2");
            Debug.WriteLine(user);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Create dashboard view model based on user category
            var dashboardData = new DashboardViewModel
            {
                CurrentUser = user, // Use the `User` type here
                UserCategory = user.Kategorija // Access `Kategorija` from the `User` type
            };

            switch (user.Kategorija)
            {
                case Kategorija.Guest:
                    await LoadGuestDashboardData(dashboardData, user.Id);
                    break;
                case Kategorija.Clan:
                    await LoadClanDashboardData(dashboardData, user.Id);
                    break;
                case Kategorija.Trener:
                    await LoadTrenerDashboardData(dashboardData, user.Id);
                    break;
                case Kategorija.Admin:
                    await LoadAdminDashboardData(dashboardData);
                    break;
            }

            return View(dashboardData);
        }

        private async Task LoadGuestDashboardData(DashboardViewModel model, string userId)
        {
            // Guest can only see trails they signed up for
            model.UserTrails = await _context.TrailKonji
                .Include(tk => tk.Trail)
                .Where(tk => tk.Trail.RezervatorId == userId)
                .Select(tk => tk.Trail)
                .Distinct()
                .ToListAsync();
        }

        private async Task LoadClanDashboardData(DashboardViewModel model, string userId)
        {
            // Get user's current level
            var userDetails = await _context.Users.FindAsync(userId);
            model.CurrentUserLevel = userDetails?.Nivo ?? Nivo.Pocetnik;

            // Get current user's clanarina (latest one)
            model.CurrentUserClanarina = await _context.Clanarine
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IstekClanarine)
                .FirstOrDefaultAsync();

            // Get IDs of trainings user is already registered for
            var registeredTrainingIds = await _context.TreningUsers
                .Where(tu => tu.UserId == userId)
                .Select(tu => tu.TreningId)
                .ToListAsync();

            // Get all available trainings for registration (excluding ones user is already registered for and full trainings)
            model.AvailableTrainings = await _context.Treninzi
                .Include(t => t.Trener)
                .Include(t => t.TreningKonji)
                    .ThenInclude(tk => tk.Konj)
                .Include(t => t.TreningUsers) // Include to check capacity
                .Where(t => t.Datum > DateTime.Now && // Only future trainings
                           !registeredTrainingIds.Contains(t.Id) && // Exclude trainings user is already registered for
                           t.TreningUsers.Count < t.MaxBrClanova) // Exclude full trainings
                .OrderBy(t => t.Nivo)
                .ToListAsync();

            // Get trainings user is already registered for
            model.RegisteredTrainings = await _context.TreningUsers
                .Include(tu => tu.Trening)
                    .ThenInclude(t => t.Trener)
                .Include(tu => tu.Trening)
                    .ThenInclude(t => t.TreningKonji)
                        .ThenInclude(tk => tk.Konj)
                .Where(tu => tu.UserId == userId)
                .Select(tu => tu.Trening)
                .OrderBy(t => t.Datum)
                .ToListAsync();
        }

        private async Task LoadTrenerDashboardData(DashboardViewModel model, string userId)
        {
            // Get all trainings (trainer can see all and create new ones)
            model.AllTrainings = await _context.Treninzi
                .Include(t => t.Trener)
                .OrderByDescending(t => t.Datum)
                .ToListAsync();

            // Get all clan members whose level can be changed
            model.ClanMembers = await _context.Users
                .Where(u => u.Kategorija == Kategorija.Clan)
                .ToListAsync();
        }


        private async Task LoadAdminDashboardData(DashboardViewModel model)
        {
            // Get all trainings
            model.AllTrainings = await _context.Treninzi
                .Include(t => t.Trener)
                .OrderByDescending(t => t.Datum)
                .ToListAsync();

            // Get all trails
            model.AllTrails = await _context.Trails
                .Include(t => t.Rezervator)
                .OrderByDescending(t => t.Datum)
                .ToListAsync();

            // Get all trainers
            model.AllTrainers = await _context.Users
                .Where(u => u.Kategorija == Kategorija.Trener)
                .ToListAsync();

            // Get all users for management
            model.AllUsers = await _context.Users
                .OrderBy(u => u.Ime)
                .ToListAsync();

            // Get all horses for management
            model.AllHorses = await _context.Konji
                .OrderBy(k => k.Ime)
                .ToListAsync();
        }

        // Action for changing user level (Trener only)
        // Action for changing user level (Trener only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserLevel(string userId, int newLevel)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Greška: Korisnik nije pronađen.";
                    return RedirectToAction("Index");
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == currentUser.UserName);

                if (user?.Kategorija != Kategorija.Trener && user?.Kategorija != Kategorija.Admin)
                {
                    TempData["ErrorMessage"] = "Nemate dozvolu za menjanje nivoa korisnika.";
                    return RedirectToAction("Index");
                }

                var targetUser = await _context.Users.FindAsync(userId);
                if (targetUser == null)
                {
                    TempData["ErrorMessage"] = "Korisnik nije pronađen.";
                    return RedirectToAction("Index");
                }

                if (targetUser.Kategorija != Kategorija.Clan)
                {
                    TempData["ErrorMessage"] = "Možete menjati nivo samo članovima kluba.";
                    return RedirectToAction("Index");
                }

                // Validate the new level value
                if (!Enum.IsDefined(typeof(Nivo), newLevel))
                {
                    TempData["ErrorMessage"] = "Neispravna vrednost za nivo.";
                    return RedirectToAction("Index");
                }

                var oldLevel = targetUser.Nivo;
                targetUser.Nivo = (Nivo)newLevel;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Nivo korisnika {targetUser.Ime} {targetUser.Prezime} je uspešno promenjen sa {oldLevel} na {(Nivo)newLevel}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Greška pri menjanju nivoa: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // Dodaj metodu za prijavljivanje na trening
        [HttpGet]
        public async Task<IActionResult> PrijaviSe(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user has active membership before allowing training registration
            if (currentUser.Kategorija == Kategorija.Clan)
            {
                var aktivnaClanarina = await _context.Clanarine
                    .Where(c => c.UserId == currentUser.Id)
                    .OrderByDescending(c => c.IstekClanarine)
                    .FirstOrDefaultAsync();

                if (aktivnaClanarina == null || aktivnaClanarina.IstekClanarine < DateTime.Now)
                {
                    TempData["Error"] = "Ne možete se prijaviti na trening jer nemate aktivnu članarinu. Molimo produžite članarinu.";
                    return RedirectToAction("Index");
                }
            }

            var trening = await _context.Treninzi
                .Include(t => t.TreningUsers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trening == null)
            {
                return NotFound();
            }

            // Proveri da li je korisnik već prijavljen
            var vec_prijavljen = trening.TreningUsers.Any(tu => tu.UserId == currentUser.Id);
            if (vec_prijavljen)
            {
                TempData["Error"] = "Već ste prijavljeni na ovaj trening!";
                return RedirectToAction("Index");
            }

            // Proveri da li je trening popunjen
            if (trening.TreningUsers.Count >= trening.MaxBrClanova)
            {
                TempData["Error"] = "Trening je popunjen!";
                return RedirectToAction("Index");
            }

            // Dodaj korisnika na trening
            var treningUser = new Trening_User
            {
                TreningId = id,
                UserId = currentUser.Id
            };

            _context.TreningUsers.Add(treningUser);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Uspešno ste se prijavili na trening!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> OdjaviSe(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var treningUser = await _context.TreningUsers
                .FirstOrDefaultAsync(tu => tu.TreningId == id && tu.UserId == currentUser.Id);

            if (treningUser == null)
            {
                TempData["Error"] = "Niste prijavljeni na ovaj trening!";
                return RedirectToAction("Index");
            }

            // Check if training is in the past (optional - you might want to prevent unenrolling from past trainings)
            var trening = await _context.Treninzi.FindAsync(id);
            if (trening != null && trening.Datum <= DateTime.Now)
            {
                TempData["Error"] = "Ne možete se odjaviti sa treninga koji je već završen!";
                return RedirectToAction("Index");
            }

            _context.TreningUsers.Remove(treningUser);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Uspešno ste se odjavili sa treninga!";
            return RedirectToAction("Index");
        }


        // Metoda za prikaz prijavljenih treninga korisnika
        public async Task<IActionResult> MojiTreninzi()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.CurrentUserLevel = currentUser.Nivo;
            ViewBag.CurrentUserName = $"{currentUser.Ime} {currentUser.Prezime}";

            var mojiTreninzi = await _context.TreningUsers
                .Where(tu => tu.UserId == currentUser.Id)
                .Include(tu => tu.Trening)
                    .ThenInclude(t => t.Trener)
                .Include(tu => tu.Trening)
                    .ThenInclude(t => t.TreningKonji)
                        .ThenInclude(tk => tk.Konj)
                .Select(tu => tu.Trening)
                .OrderBy(t => t.Datum)
                .ToListAsync();

            return View(mojiTreninzi);
        }

        public async Task<IActionResult> AvailableTrainings()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                ViewBag.CurrentUserLevel = currentUser.Nivo;
                ViewBag.CurrentUserName = $"{currentUser.Ime} {currentUser.Prezime}";

                // Get IDs of trainings user is already registered for
                var registeredTrainingIds = await _context.TreningUsers
                    .Where(tu => tu.UserId == currentUser.Id)
                    .Select(tu => tu.TreningId)
                    .ToListAsync();

                // Filter out trainings user is already registered for
                var treninzi = await _context.Treninzi
                    .Include(t => t.Trener)
                    .Include(t => t.TreningKonji)
                        .ThenInclude(tk => tk.Konj)
                    .Where(t => t.Datum >= DateTime.Today && !registeredTrainingIds.Contains(t.Id))
                    .OrderBy(t => t.Datum)
                    .ToListAsync();

                return View(treninzi);
            }
            else
            {
                ViewBag.CurrentUserLevel = Nivo.Pocetnik;
                ViewBag.CurrentUserName = "Nepoznat korisnik";

                var treninzi = await _context.Treninzi
                    .Include(t => t.Trener)
                    .Include(t => t.TreningKonji)
                        .ThenInclude(tk => tk.Konj)
                    .Where(t => t.Datum >= DateTime.Today)
                    .OrderBy(t => t.Datum)
                    .ToListAsync();

                return View(treninzi);
            }
        }

    }
}