using Jahacki_klub_Zeljeznicar.Data;
using Jahacki_klub_Zeljeznicar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
        [Authorize(Policy = "TrenerOrAdmin")]
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
        [Authorize(Policy = "TrenerOrAdmin")]
        public async Task<IActionResult> Create()
        {
            // Postavi ViewBag za trenere
            var treneri = await _userManager.GetUsersInRoleAsync("Trener");
            ViewBag.TrenerId = new SelectList(
                treneri.Select(u => new {
                    Id = u.Id,
                    ImePrezime = u.Ime + " " + u.Prezime
                }).ToList(),
                "Id",
                "ImePrezime"
            );

            ViewBag.Konji = await _context.Konji.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "TrenerOrAdmin")]
        public async Task<IActionResult> Create(Trening trening, int[] SelectedHorseIds)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(trening.Naziv))
            {
                ModelState.AddModelError(nameof(trening.Naziv), "Naziv treninga je obavezan.");
            }

            if (trening.Datum < DateTime.Today)
            {
                ModelState.AddModelError(nameof(trening.Datum), "Datum treninga ne može biti u prošlosti.");
            }

            if (trening.MaxBrClanova <= 0)
            {
                ModelState.AddModelError(nameof(trening.MaxBrClanova), "Maksimalan broj članova mora biti veći od 0.");
            }

            // Validate horse selection
            if (SelectedHorseIds == null || SelectedHorseIds.Length == 0)
            {
                ModelState.AddModelError(nameof(SelectedHorseIds), "Morate odabrati najmanje jednog konja.");
            }
            else
            {
                foreach (var horseId in SelectedHorseIds)
                {
                    bool isOccupied = await _context.TreningKonji
                        .Include(tk => tk.Trening)
                        .AnyAsync(tk => tk.KonjId == horseId && tk.Trening.Datum.Date == trening.Datum.Date) ||
                        await _context.TrailKonji
                        .Include(tk => tk.Trail)
                        .AnyAsync(tk => tk.KonjId == horseId && tk.Trail.Datum.Date == trening.Datum.Date);

                    if (isOccupied)
                    {
                        var horse = await _context.Konji.FindAsync(horseId);
                        ModelState.AddModelError(nameof(SelectedHorseIds), $"Konj {horse?.Ime} je već rezervisan za taj dan.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    trening.TrenerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _context.Treninzi.Add(trening);
                    await _context.SaveChangesAsync();

                    foreach (var konjId in SelectedHorseIds)
                    {
                        _context.TreningKonji.Add(new Trening_Konj
                        {
                            TreningId = trening.Id,
                            KonjId = konjId
                        });
                    }
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Trening je uspešno kreiran!";
                    return RedirectToAction("Index", "Dashboard");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Greška pri kreiranju treninga: {ex.Message}");
                }
            }

            // Re-populate view data if validation fails
            await PopulateCreateViewData();
            return View(trening);
        }


        // GET: Trening/Edit/5
        [Authorize(Policy = "TrenerOrAdmin")]
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

            // Postavi ViewBag za trenere (samo korisnici sa ulogom Trener)
            var treneri = await _userManager.GetUsersInRoleAsync("Trener");

            // Debug - ispiši koliko trenera ima
            System.Diagnostics.Debug.WriteLine($"Broj trenera: {treneri.Count}");
            foreach (var t in treneri)
            {
                System.Diagnostics.Debug.WriteLine($"Trener: {t.Ime} {t.Prezime}, ID: {t.Id}");

            }
            // Dodaj ovo u Edit GET metodu
            ViewBag.DebugTreneri = $"Broj trenera: {treneri.Count}";

            // Kreiraj listu trenera
            var trenerList = treneri.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.Ime} {u.Prezime}",
                Selected = u.Id == trening.TrenerId
            }).ToList();

            ViewBag.TrenerId = trenerList;

            // Postavi ViewBag za konje
            ViewBag.Konji = await _context.Konji.ToListAsync();
            ViewBag.SelectedHorseIds = trening.TreningKonji.Select(tk => tk.KonjId).ToArray();

            return View(trening);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "TrenerOrAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Naziv,Nivo,Datum,MaxBrClanova,TrenerId")] Trening trening, int[] SelectedHorseIds)
        {
            if (id != trening.Id)
            {
                ModelState.AddModelError(string.Empty, "ID treninga se ne poklapa.");
                return View(trening);
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(trening.Naziv))
            {
                ModelState.AddModelError(nameof(trening.Naziv), "Naziv treninga je obavezan.");
            }

            if (trening.Datum < DateTime.Today)
            {
                ModelState.AddModelError(nameof(trening.Datum), "Datum treninga ne može biti u prošlosti.");
            }

            if (trening.MaxBrClanova <= 0)
            {
                ModelState.AddModelError(nameof(trening.MaxBrClanova), "Maksimalan broj članova mora biti veći od 0.");
            }

            // Validate horse selection
            if (SelectedHorseIds == null || SelectedHorseIds.Length == 0)
            {
                ModelState.AddModelError(nameof(SelectedHorseIds), "Morate odabrati najmanje jednog konja.");
            }
            else
            {
                foreach (var horseId in SelectedHorseIds)
                {
                    bool isOccupied = await _context.TreningKonji
                        .Include(tk => tk.Trening)
                        .AnyAsync(tk => tk.KonjId == horseId &&
                                       tk.Trening.Datum.Date == trening.Datum.Date &&
                                       tk.TreningId != id) ||
                        await _context.TrailKonji
                        .Include(tk => tk.Trail)
                        .AnyAsync(tk => tk.KonjId == horseId && tk.Trail.Datum.Date == trening.Datum.Date);

                    if (isOccupied)
                    {
                        var horse = await _context.Konji.FindAsync(horseId);
                        ModelState.AddModelError(nameof(SelectedHorseIds), $"Konj {horse?.Ime} je već rezervisan za taj dan.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update training
                    _context.Update(trening);

                    // Update horses
                    var existingAssignments = _context.TreningKonji.Where(tk => tk.TreningId == id);
                    _context.TreningKonji.RemoveRange(existingAssignments);

                    foreach (var horseId in SelectedHorseIds)
                    {
                        _context.TreningKonji.Add(new Trening_Konj
                        {
                            TreningId = id,
                            KonjId = horseId
                        });
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Trening je uspešno ažuriran!";
                    return RedirectToAction("Index", "Dashboard");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TreningExists(trening.Id))
                    {
                        return NotFound();
                    }
                    ModelState.AddModelError(string.Empty, "Greška pri ažuriranju - podaci su promijenjeni od strane drugog korisnika.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Greška pri ažuriranju treninga: {ex.Message}");
                }
            }

            // Re-populate view data if validation fails
            await PopulateEditViewData(trening.TrenerId, SelectedHorseIds);
            return View(trening);
        }


        // GET: Trening/Delete/5
        [Authorize(Policy = "TrenerOrAdmin")]
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
        [Authorize(Policy = "TrenerOrAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trening = await _context.Treninzi.FindAsync(id);
            if (trening == null)
            {
                ModelState.AddModelError(string.Empty, "Trening nije pronađen.");
                return RedirectToAction("Index");
            }

            try
            {
                // Remove related entities first
                var treningKonji = _context.TreningKonji.Where(tk => tk.TreningId == id);
                _context.TreningKonji.RemoveRange(treningKonji);

                var treningUsers = _context.TreningUsers.Where(tu => tu.TreningId == id);
                _context.TreningUsers.RemoveRange(treningUsers);

                // Then remove the training
                _context.Treninzi.Remove(trening);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Trening je uspešno obrisan!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Greška pri brisanju treninga: {ex.Message}");
                return View("Delete", await _context.Treninzi
                    .Include(t => t.Trener)
                    .Include(t => t.TreningKonji)
                        .ThenInclude(tk => tk.Konj)
                    .FirstOrDefaultAsync(t => t.Id == id));
            }

            return RedirectToAction("Index", "Dashboard");
        }

        private async Task PopulateCreateViewData()
        {
            var treneri = await _userManager.GetUsersInRoleAsync("Trener");
            ViewBag.TrenerId = new SelectList(
                treneri.Select(u => new {
                    Id = u.Id,
                    ImePrezime = $"{u.Ime} {u.Prezime}"
                }),
                "Id",
                "ImePrezime",
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );
            ViewBag.Konji = await _context.Konji.ToListAsync();
        }

        private async Task PopulateEditViewData(string trenerId, int[] selectedHorseIds)
        {
            var treneri = await _userManager.GetUsersInRoleAsync("Trener");
            ViewBag.TrenerId = new SelectList(
                treneri.Select(u => new {
                    Id = u.Id,
                    ImePrezime = $"{u.Ime} {u.Prezime}"
                }),
                "Id",
                "ImePrezime",
                trenerId
            );
            ViewBag.Konji = await _context.Konji.ToListAsync();
            ViewBag.SelectedHorseIds = selectedHorseIds;
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