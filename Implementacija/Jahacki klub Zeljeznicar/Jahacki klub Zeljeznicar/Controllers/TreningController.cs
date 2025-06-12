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
            ModelState.Remove("TrenerId");
            ModelState.Remove("Trener");

            var selectedDate = trening.Datum.Date;

            if (SelectedHorseIds == null || SelectedHorseIds.Length == 0)
            {
                ModelState.AddModelError("", "Morate izabrati najmanje jednog konja za trening.");
            }
            else
            {
                foreach (var horseId in SelectedHorseIds)
                {
                    bool isOccupiedInTrening = await _context.TreningKonji
                        .Include(tk => tk.Trening)
                        .AnyAsync(tk => tk.KonjId == horseId && tk.Trening.Datum.Date == selectedDate);

                    bool isOccupiedInTrail = await _context.TrailKonji
                        .Include(tk => tk.Trail)
                        .AnyAsync(tk => tk.KonjId == horseId && tk.Trail.Datum.Date == selectedDate);

                    if (isOccupiedInTrening || isOccupiedInTrail)
                    {
                        var horse = await _context.Konji.FindAsync(horseId);
                        ModelState.AddModelError("", $"Konj {horse?.Ime} je već rezervisan za trail/trening na dan {selectedDate:dd.MM.yyyy}.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                trening.TrenerId = string.IsNullOrEmpty(currentUserId)
                    ? _context.Users.Select(u => u.Id).FirstOrDefault()
                    : currentUserId;

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
                return RedirectToAction("Index", "Dashboard");
            }

            // Ako validacija ne prođe, ponovo postavi ViewBag podatke
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
                return NotFound();
            }

            var selectedDate = trening.Datum.Date;

            if (SelectedHorseIds == null || SelectedHorseIds.Length == 0)
            {
                ModelState.AddModelError("", "Morate izabrati najmanje jednog konja za trening.");
            }
            else
            {
                foreach (var horseId in SelectedHorseIds)
                {
                    bool isOccupiedInTrening = await _context.TreningKonji
                        .Include(tk => tk.Trening)
                        .AnyAsync(tk => tk.KonjId == horseId && tk.Trening.Datum.Date == selectedDate && tk.TreningId != id);

                    bool isOccupiedInTrail = await _context.TrailKonji
                        .Include(tk => tk.Trail)
                        .AnyAsync(tk => tk.KonjId == horseId && tk.Trail.Datum.Date == selectedDate);

                    if (isOccupiedInTrening || isOccupiedInTrail)
                    {
                        var horse = await _context.Konji.FindAsync(horseId);
                        ModelState.AddModelError("", $"Konj {horse?.Ime} je već rezervisan za trail/trening na dan {selectedDate:dd.MM.yyyy}.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trening);

                    var postojeciTreningKonji = _context.TreningKonji.Where(tk => tk.TreningId == id);
                    _context.TreningKonji.RemoveRange(postojeciTreningKonji);

                    foreach (var konjId in SelectedHorseIds)
                    {
                        _context.TreningKonji.Add(new Trening_Konj
                        {
                            TreningId = id,
                            KonjId = konjId
                        });
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
                return RedirectToAction("Index", "Dashboard");
            }

            // Ako validacija ne prođe, ponovo postavi ViewBag podatke
            var treneri2 = await _userManager.GetUsersInRoleAsync("Trener");
            var trenerList2 = treneri2.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.Ime} {u.Prezime}",
                Selected = u.Id == trening.TrenerId
            }).ToList();

            ViewBag.TrenerId = trenerList2;
            ViewBag.Konji = await _context.Konji.ToListAsync();
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
            return RedirectToAction("Index", "Dashboard");
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