using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Jahacki_klub_Zeljeznicar.Data;
using Jahacki_klub_Zeljeznicar.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Jahacki_klub_Zeljeznicar.Controllers
{
    public class TreningController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public TreningController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Trening
        public async Task<IActionResult> Index()
        {
            // Učitaj sve treninge sa povezanim podacima
            var treninzi = await _context.Treninzi
                .Include(t => t.Trener)
                .Include(t => t.TreningKonji)
                    .ThenInclude(tk => tk.Konj)
                .OrderBy(t => t.Datum)
                .ToListAsync();

            return View(treninzi);
        }

        // GET: Trening/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trening = await _context.Treninzi
                .Include(t => t.Trener)
                .Include(t => t.TreningKonji)
                    .ThenInclude(tk => tk.Konj)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trening == null)
            {
                return NotFound();
            }

            return View(trening);
        }

        // GET: Trening/Create
        public IActionResult Create()
        {
            ViewBag.Konji = _context.Konji.ToList();
            return View();
        }

        // POST: Trening/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trening trening, int[] SelectedHorseIds)
        {
            ModelState.Remove("TrenerId");
            ModelState.Remove("Trener");

            if (ModelState.IsValid)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(currentUserId))
                {
                    // fallback TrenerId (uzmi neki postojećeg trenera iz baze)
                    var nekiTrenerId = _context.Users
                        .Select(u => u.Id)
                        .FirstOrDefault();

                    trening.TrenerId = nekiTrenerId;
                }
                else
                {
                    trening.TrenerId = currentUserId;
                }

                _context.Treninzi.Add(trening);
                await _context.SaveChangesAsync();

                if (SelectedHorseIds != null && SelectedHorseIds.Length > 0)
                {
                    foreach (var konjId in SelectedHorseIds)
                    {
                        var treningKonj = new Trening_Konj
                        {
                            TreningId = trening.Id,
                            KonjId = konjId
                        };
                        _context.TreningKonji.Add(treningKonj);
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Konji = _context.Konji.ToList();
            return View(trening);
        }

        // GET: Trening/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trening = await _context.Treninzi
                .Include(t => t.TreningKonji)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trening == null)
            {
                return NotFound();
            }

            ViewData["TrenerId"] = new SelectList(_context.Set<User>(), "Id", "Id", trening.TrenerId);
            ViewBag.Konji = _context.Konji.ToList();
            ViewBag.SelectedHorseIds = trening.TreningKonji.Select(tk => tk.KonjId).ToArray();

            return View(trening);
        }

        // POST: Trening/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Naziv,Nivo,Datum,MaxBrClanova,TrenerId")] Trening trening, int[] SelectedHorseIds)
        {
            if (id != trening.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trening);

                    // Ukloni postojeće veze sa konjima
                    var postojeciTreningKonji = _context.TreningKonji.Where(tk => tk.TreningId == id);
                    _context.TreningKonji.RemoveRange(postojeciTreningKonji);

                    // Dodaj nove veze sa konjima
                    if (SelectedHorseIds != null && SelectedHorseIds.Length > 0)
                    {
                        foreach (var konjId in SelectedHorseIds)
                        {
                            var treningKonj = new Trening_Konj
                            {
                                TreningId = id,
                                KonjId = konjId
                            };
                            _context.TreningKonji.Add(treningKonj);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TreningExists(trening.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["TrenerId"] = new SelectList(_context.Set<User>(), "Id", "Id", trening.TrenerId);
            ViewBag.Konji = _context.Konji.ToList();
            return View(trening);
        }

        // GET: Trening/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trening = await _context.Treninzi
                .Include(t => t.Trener)
                .Include(t => t.TreningKonji)
                    .ThenInclude(tk => tk.Konj)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trening == null)
            {
                return NotFound();
            }

            return View(trening);
        }

        // POST: Trening/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trening = await _context.Treninzi.FindAsync(id);
            if (trening != null)
            {
                // Prvo ukloni povezane entitete
                var treningKonji = _context.TreningKonji.Where(tk => tk.TreningId == id);
                _context.TreningKonji.RemoveRange(treningKonji);

                var treningUsers = _context.TreningUsers.Where(tu => tu.TreningId == id);
                _context.TreningUsers.RemoveRange(treningUsers);

                // Zatim ukloni trening
                _context.Treninzi.Remove(trening);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TreningExists(int id)
        {
            return _context.Treninzi.Any(e => e.Id == id);
        }

        public async Task<IActionResult> AvailableTrainings()
        {
            // Dohvati trenutnog korisnika i prosledji njegov nivo
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                ViewBag.CurrentUserLevel = currentUser.Nivo;
                ViewBag.CurrentUserName = $"{currentUser.Ime} {currentUser.Prezime}";
            }
            else
            {
                ViewBag.CurrentUserLevel = Nivo.Pocetnik; // Default vrednost
                ViewBag.CurrentUserName = "Nepoznat korisnik";
            }

            var treninzi = await _context.Treninzi
                .Include(t => t.Trener)
                .Include(t => t.TreningKonji)
                    .ThenInclude(tk => tk.Konj)
                .Where(t => t.Datum >= DateTime.Today) // Prikaži samo buduće treninge
                .OrderBy(t => t.Datum)
                .ToListAsync();

            return View(treninzi);
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
                return RedirectToAction("AvailableTrainings");
            }

            // Proveri da li je trening popunjen
            if (trening.TreningUsers.Count >= trening.MaxBrClanova)
            {
                TempData["Error"] = "Trening je popunjen!";
                return RedirectToAction("AvailableTrainings");
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
            return RedirectToAction("AvailableTrainings");
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
        public async Task<IActionResult> TrenerView()
        {
            // Učitaj sve treninge sa povezanim podacima za trenerski prikaz
            var treninzi = await _context.Treninzi
                .Include(t => t.Trener)
                .Include(t => t.TreningKonji)
                    .ThenInclude(tk => tk.Konj)
                .Include(t => t.TreningUsers)
                    .ThenInclude(tu => tu.User)
                .OrderBy(t => t.Datum)
                .ToListAsync();

            return View(treninzi);
        }
    }
}