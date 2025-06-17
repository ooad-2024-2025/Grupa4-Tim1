using Jahacki_klub_Zeljeznicar.Data;
using Jahacki_klub_Zeljeznicar.Models;
using Jahacki_klub_Zeljeznicar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

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

            bool hasActiveMembership = model.CurrentUserClanarina != null &&
                                      model.CurrentUserClanarina.IstekClanarine >= DateTime.Now;

            var registeredTrainingIds = await _context.TreningUsers
                .Where(tu => tu.UserId == userId)
                .Select(tu => tu.TreningId)
                .ToListAsync();

            if (hasActiveMembership)
            {
                var availableTrainings = await _context.Treninzi
                    .Include(t => t.Trener)
                    .Include(t => t.TreningKonji)
                        .ThenInclude(tk => tk.Konj)
                    .Include(t => t.TreningUsers) 
                    .Where(t => t.Datum > DateTime.Now && 
                               !registeredTrainingIds.Contains(t.Id) &&
                               t.TreningUsers.Count < t.MaxBrClanova)
                    .ToListAsync();

                model.RecommendedTrainingIds = new List<int>();

                if (userDetails != null && availableTrainings.Any())
                {
                    var userTrainingHistory = await _context.TreningUsers
                        .Include(tu => tu.Trening)
                            .ThenInclude(t => t.Trener)
                        .Include(tu => tu.Trening)
                            .ThenInclude(t => t.TreningKonji)
                                .ThenInclude(tk => tk.Konj)
                        .Where(tu => tu.UserId == userId)
                        .OrderByDescending(tu => tu.Trening.Datum)
                        .ToListAsync();

                    var recommendedTrainings = ApplyRecommendationAlgorithm(userDetails, userTrainingHistory, availableTrainings)
                        .Take(3)
                        .ToList();

                    model.RecommendedTrainingIds = recommendedTrainings.Select(t => t.Id).ToList();

                    var sortedTrainings = new List<Trening>();
                    sortedTrainings.AddRange(recommendedTrainings);
                    var nonRecommendedTrainings = availableTrainings
                        .Where(t => !model.RecommendedTrainingIds.Contains(t.Id))
                        .OrderBy(t => t.Datum)
                        .ToList();
                    sortedTrainings.AddRange(nonRecommendedTrainings);
                    model.AvailableTrainings = sortedTrainings;
                }
                else
                {
                    model.AvailableTrainings = availableTrainings.OrderBy(t => t.Datum).ToList();
                }

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
            else
            {
                model.AvailableTrainings = new List<Trening>();
                model.RegisteredTrainings = new List<Trening>();
                model.RecommendedTrainingIds = new List<int>();
            }

            model.HasActiveMembership = hasActiveMembership;
        }

        private async Task LoadTrenerDashboardData(DashboardViewModel model, string userId)
        {
            // Get all trainings (trainer can see all and create new ones)
            model.AllTrainings = await _context.Treninzi
                .Include(t => t.Trener)
                .Include(t => t.TreningUsers)
                    .ThenInclude(tu => tu.User)
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
        [Authorize(Policy = "ClanOnly")]
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
        [Authorize(Policy = "ClanOnly")]
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
        [Authorize(Policy = "ClanOnly")]
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

        [Authorize(Policy = "ClanOnly")]
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

                // Get user's training history for analysis
                var userTrainingHistory = await _context.TreningUsers
                    .Include(tu => tu.Trening)
                        .ThenInclude(t => t.Trener)
                    .Include(tu => tu.Trening)
                        .ThenInclude(t => t.TreningKonji)
                            .ThenInclude(tk => tk.Konj)
                    .Where(tu => tu.UserId == currentUser.Id)
                    .OrderByDescending(tu => tu.Trening.Datum)
                    .ToListAsync();

                // Get all available trainings
                var availableTrainings = await _context.Treninzi
                    .Include(t => t.Trener)
                    .Include(t => t.TreningKonji)
                        .ThenInclude(tk => tk.Konj)
                    .Where(t => t.Datum >= DateTime.Today && !registeredTrainingIds.Contains(t.Id))
                    .ToListAsync();

                // DEBUG: Log what we have
                System.Diagnostics.Debug.WriteLine($"Available trainings count: {availableTrainings.Count}");
                System.Diagnostics.Debug.WriteLine($"User training history count: {userTrainingHistory.Count}");

                // Apply sophisticated recommendation algorithm
                var recommendedTrainings = ApplyRecommendationAlgorithm(currentUser, userTrainingHistory, availableTrainings).Take(3).ToList();

                // DEBUG: Log recommended trainings
                System.Diagnostics.Debug.WriteLine($"Recommended trainings count: {recommendedTrainings.Count}");
                foreach (var rt in recommendedTrainings)
                {
                    System.Diagnostics.Debug.WriteLine($"Recommended training ID: {rt.Id}, Name: {rt.Naziv}");
                }

                // Pass recommended training IDs separately to view
                var recommendedIds = recommendedTrainings?.Select(t => t.Id).ToList() ?? new List<int>();
                ViewBag.RecommendedTrainingIds = recommendedIds;

                // DEBUG: Log ViewBag content
                System.Diagnostics.Debug.WriteLine($"ViewBag.RecommendedTrainingIds: [{string.Join(", ", recommendedIds)}]");

                return View(availableTrainings);
            }
            else
            {
                ViewBag.CurrentUserLevel = Nivo.Pocetnik;
                ViewBag.CurrentUserName = "Nepoznat korisnik";

                var treninzi = await _context.Treninzi
                    .Include(t => t.Trener)
                    .Include(t => t.TreningKonji)
                        .ThenInclude(tk => tk.Konj)
                    .Where(t => t.Datum >= DateTime.Today && t.Nivo == Nivo.Pocetnik)
                    .OrderBy(t => t.Datum)
                    .ToListAsync();

                ViewBag.RecommendedTrainingIds = new List<int>();
                return View(treninzi);
            }
        }




        private List<Trening> ApplyRecommendationAlgorithm(User user, List<Trening_User> trainingHistory, List<Trening> availableTrainings)
        {
            var scoredTrainings = new List<(Trening training, double score)>();

            foreach (var training in availableTrainings)
            {
                double score = 0;

                // 1. Level compatibility (40% weight)
                score += CalculateLevelCompatibilityScore(user.Nivo, training.Nivo) * 0.4;

                // 2. Trainer preference based on history (25% weight)
                score += CalculateTrainerPreferenceScore(trainingHistory, training.Trener) * 0.25;

                // 3. Horse familiarity and progression (20% weight)
                score += CalculateHorseFamiliarityScore(trainingHistory, training.TreningKonji.Select(tk => tk.Konj).ToList()) * 0.2;

                // 4. Scheduling pattern preference (10% weight)
                score += CalculateSchedulingPreferenceScore(trainingHistory, training.Datum) * 0.1;

                // 5. Training diversity bonus (5% weight)
                score += CalculateDiversityScore(trainingHistory, training) * 0.05;

                scoredTrainings.Add((training, score));
            }

            // Sort by score (highest first) and return
            return scoredTrainings
                .OrderByDescending(st => st.score)
                .ThenBy(st => st.training.Datum)
                .Select(st => st.training)
                .ToList();
        }

        private double CalculateLevelCompatibilityScore(Nivo? userLevel, Nivo trainingLevel)
        {
            // Perfect match gets highest score
            if (userLevel == trainingLevel) return 1.0;

            // Calculate level difference penalty
            int levelDifference = Math.Abs((int)userLevel - (int)trainingLevel);

            // Users can handle one level above their current level (challenge)
            if ((int)trainingLevel == (int)userLevel + 1) return 0.8;

            // One level below is acceptable (review/confidence building)
            if ((int)trainingLevel == (int)userLevel - 1) return 0.6;

            // Significant mismatch gets low score
            return Math.Max(0, 1.0 - (levelDifference * 0.3));
        }

        private double CalculateTrainerPreferenceScore(List<Trening_User> history, User trainer)
        {
            if (!history.Any()) return 0.5; // Neutral score for new users

            var trainerFrequency = history
                .GroupBy(h => h.Trening.TrenerId)
                .ToDictionary(g => g.Key, g => g.Count());

            var totalTrainings = history.Count;
            var trainingsWithThisTrainer = trainerFrequency.GetValueOrDefault(trainer.Id, 0);

            // Calculate preference based on frequency
            double preferenceRatio = (double)trainingsWithThisTrainer / totalTrainings;

            // Boost score for preferred trainers, but also encourage some variety
            if (preferenceRatio > 0.4) return 1.0; // Strong preference
            if (preferenceRatio > 0.2) return 0.8; // Moderate preference
            if (preferenceRatio > 0) return 0.6;   // Some experience

            return 0.4; // New trainer - slight penalty but still viable
        }

        private double CalculateHorseFamiliarityScore(List<Trening_User> history, List<Konj> trainingHorses)
        {
            if (!history.Any() || !trainingHorses.Any()) return 0.5;

            var riddenHorses = history
                .SelectMany(h => h.Trening.TreningKonji.Select(tk => tk.KonjId))
                .Distinct()
                .ToHashSet();

            var familiarHorses = trainingHorses.Count(horse => riddenHorses.Contains(horse.Id));
            var newHorses = trainingHorses.Count - familiarHorses;

            // Balance between familiar horses (comfort) and new horses (progression)
            double familiarityRatio = (double)familiarHorses / trainingHorses.Count;

            // Optimal mix: some familiar, some new
            if (familiarityRatio >= 0.3 && familiarityRatio <= 0.7) return 1.0;
            if (familiarityRatio > 0) return 0.7; // Some familiarity

            return 0.5; // All new horses - neutral
        }

        private double CalculateSchedulingPreferenceScore(List<Trening_User> history, DateTime trainingDate)
        {
            if (!history.Any()) return 0.5;

            var recentHistory = history
                .Where(h => h.Trening.Datum >= DateTime.Today.AddMonths(-3))
                .ToList();

            if (!recentHistory.Any()) return 0.5;

            // Analyze preferred days of week
            var dayPreferences = recentHistory
                .GroupBy(h => h.Trening.Datum.DayOfWeek)
                .ToDictionary(g => g.Key, g => g.Count());

            var preferredDay = dayPreferences.GetValueOrDefault(trainingDate.DayOfWeek, 0);
            var maxPreference = dayPreferences.Values.DefaultIfEmpty(1).Max();

            // Analyze preferred times (assuming time is stored in Datum)
            var timePreferences = recentHistory
                .GroupBy(h => h.Trening.Datum.Hour)
                .ToDictionary(g => g.Key, g => g.Count());

            var preferredTime = timePreferences.GetValueOrDefault(trainingDate.Hour, 0);
            var maxTimePreference = timePreferences.Values.DefaultIfEmpty(1).Max();

            // Combine day and time preferences
            double dayScore = (double)preferredDay / maxPreference;
            double timeScore = (double)preferredTime / maxTimePreference;

            return (dayScore + timeScore) / 2;
        }

        private double CalculateDiversityScore(List<Trening_User> history, Trening training)
        {
            if (!history.Any()) return 1.0; // New users get full diversity bonus

            var recentHistory = history
                .Where(h => h.Trening.Datum >= DateTime.Today.AddMonths(-1))
                .ToList();

            if (!recentHistory.Any()) return 1.0;

            // Check if this type of training was done recently
            var recentTrainingTypes = recentHistory
                .Select(h => new { h.Trening.Nivo, h.Trening.TrenerId })
                .ToHashSet();

            bool isDiverse = !recentTrainingTypes.Contains(new { training.Nivo, training.TrenerId });

            return isDiverse ? 1.0 : 0.3; // Encourage diversity
        }

    }
}