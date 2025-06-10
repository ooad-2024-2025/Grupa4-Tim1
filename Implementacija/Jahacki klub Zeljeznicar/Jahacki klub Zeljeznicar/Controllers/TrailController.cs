using Jahacki_klub_Zeljeznicar.Data;
using Jahacki_klub_Zeljeznicar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jahacki_klub_Zeljeznicar.Controllers
{
    public class TrailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrailController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trail
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Trails.Include(t => t.Rezervator);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Trail/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trail = await _context.Trails
                .Include(t => t.Rezervator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trail == null)
            {
                return NotFound();
            }

            return View(trail);
        }

        // GET: Trail/Create
        public async Task<IActionResult> Create()
        {
            var konji = await _context.Konji.ToListAsync();
            ViewBag.Konji = konji;
            return View();
        }

        // POST: Trail/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Naziv,Opis,Datum")] Trail trail, List<int> SelectedHorseIds)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Naziv: {trail.Naziv}");
                System.Diagnostics.Debug.WriteLine($"Datum: {trail.Datum}");
                System.Diagnostics.Debug.WriteLine($"Opis: {trail.Opis}");
                System.Diagnostics.Debug.WriteLine($"Selected horses count: {SelectedHorseIds?.Count ?? 0}");

                // Trail is not reserved by default
                trail.RezervatorId = null;

                ModelState.Remove("RezervatorId");
                ModelState.Remove("Rezervator");
                ModelState.Remove("TrailKonji");

                if (SelectedHorseIds == null || !SelectedHorseIds.Any())
                {
                    ModelState.AddModelError("", "Morate izabrati najmanje jednog konja za trail.");
                }
                var selectedDate = trail.Datum.Date;

                foreach (var horseId in SelectedHorseIds)
                {
                    bool isOccupiedOnTrail = await _context.TrailKonji
                        .Include(tk => tk.Trail)
                        .AnyAsync(tk => tk.KonjId == horseId && tk.Trail.Datum.Date == selectedDate);

                    bool isOccupiedOnTraining = await _context.TreningKonji
                        .Include(tk => tk.Trening)
                        .AnyAsync(tk => tk.KonjId == horseId && tk.Trening.Datum.Date == selectedDate);

                    if (isOccupiedOnTrail || isOccupiedOnTraining)
                    {
                        var horse = await _context.Konji.FindAsync(horseId);
                        ModelState.AddModelError("", $"Konj {horse?.Ime} je već rezervisan za trail ili trening na dan {selectedDate:dd.MM.yyyy}.");
                    }
                }


                if (ModelState.IsValid)
                {
                    _context.Add(trail);
                    await _context.SaveChangesAsync();

                    if (SelectedHorseIds != null && SelectedHorseIds.Any())
                    {
                        foreach (var konjId in SelectedHorseIds)
                        {
                            var trailKonj = new Trail_Konj
                            {
                                TrailId = trail.Id,
                                KonjId = konjId
                            };
                            _context.TrailKonji.Add(trailKonj);
                        }

                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    foreach (var modelState in ModelState)
                    {
                        foreach (var error in modelState.Value.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Greška za {modelState.Key}: {error.ErrorMessage}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Greška pri dodavanju traila: {ex.Message}");
                ModelState.AddModelError("", "Dogodila se greška pri dodavanju traila. Molimo pokušajte ponovo.");
            }

            var konji = await _context.Konji.ToListAsync();
            ViewBag.Konji = konji;
            return View(trail);
        }

        // GET: Trail/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trail = await _context.Trails
                .Include(t => t.TrailKonji)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trail == null)
            {
                return NotFound();
            }

            ViewData["RezervatorId"] = new SelectList(_context.Set<User>(), "Id", "Id", trail.RezervatorId);

            var allHorses = await _context.Konji.ToListAsync();
            var selectedHorseIds = trail.TrailKonji.Select(tk => tk.KonjId).ToList();

            ViewBag.Konji = allHorses;
            ViewBag.SelectedHorseIds = selectedHorseIds;

            return View(trail);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Naziv,Opis,Datum,RezervatorId")] Trail trail, List<int> SelectedHorseIds)
        {
            if (id != trail.Id)
            {
                return NotFound();
            }

            ModelState.Remove("TrailKonji");

            if (SelectedHorseIds == null || !SelectedHorseIds.Any())
            {
                ModelState.AddModelError("", "Morate izabrati najmanje jednog konja za trail.");
            }

            var selectedDate = trail.Datum.Date;

            foreach (var horseId in SelectedHorseIds)
            {
                bool isOccupiedOnTrail = await _context.TrailKonji
                    .Include(tk => tk.Trail)
                    .AnyAsync(tk =>
                        tk.KonjId == horseId &&
                        tk.Trail.Datum.Date == selectedDate &&
                        tk.TrailId != trail.Id); // exclude current trail

                bool isOccupiedOnTraining = await _context.TreningKonji
                    .Include(tk => tk.Trening)
                    .AnyAsync(tk =>
                        tk.KonjId == horseId &&
                        tk.Trening.Datum.Date == selectedDate);

                if (isOccupiedOnTrail || isOccupiedOnTraining)
                {
                    var horse = await _context.Konji.FindAsync(horseId);
                    ModelState.AddModelError("", $"Konj {horse?.Ime} je već rezervisan za trail ili trening na dan {selectedDate:dd.MM.yyyy}.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update trail
                    _context.Update(trail);
                    await _context.SaveChangesAsync();

                    // Remove old assignments
                    var existingTrailKonji = _context.TrailKonji.Where(tk => tk.TrailId == trail.Id);
                    _context.TrailKonji.RemoveRange(existingTrailKonji);
                    await _context.SaveChangesAsync();

                    // Add new assignments
                    foreach (var horseId in SelectedHorseIds)
                    {
                        var newTk = new Trail_Konj
                        {
                            TrailId = trail.Id,
                            KonjId = horseId
                        };
                        _context.TrailKonji.Add(newTk);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrailExists(trail.Id))
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

            // Re-populate horses for View in case of errors
            ViewData["RezervatorId"] = new SelectList(_context.Set<User>(), "Id", "Id", trail.RezervatorId);
            ViewBag.Konji = await _context.Konji.ToListAsync();
            ViewBag.SelectedHorseIds = SelectedHorseIds;

            return View(trail);
        }


        // GET: Trail/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trail = await _context.Trails
                .Include(t => t.Rezervator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trail == null)
            {
                return NotFound();
            }

            return View(trail);
        }

        // POST: Trail/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trail = await _context.Trails.FindAsync(id);
            if (trail != null)
            {
                _context.Trails.Remove(trail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Dashboard");
        }

        private bool TrailExists(int id)
        {
            return _context.Trails.Any(e => e.Id == id);
        }
    }
}
