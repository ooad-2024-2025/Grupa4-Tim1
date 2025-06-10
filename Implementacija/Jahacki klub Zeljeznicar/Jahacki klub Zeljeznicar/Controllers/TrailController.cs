using Jahacki_klub_Zeljeznicar.Data;
using Jahacki_klub_Zeljeznicar.Models;
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

        // POST: Trail/Create - ISPRAVLJENA VERZIJA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Naziv,Opis,Datum")] Trail trail, List<int> SelectedHorseIds)
        {
            try
            {
                // Debug informacije
                System.Diagnostics.Debug.WriteLine($"Naziv: {trail.Naziv}");
                System.Diagnostics.Debug.WriteLine($"Datum: {trail.Datum}");
                System.Diagnostics.Debug.WriteLine($"Opis: {trail.Opis}");
                System.Diagnostics.Debug.WriteLine($"Selected horses count: {SelectedHorseIds?.Count ?? 0}");

                // VAŽNO: Postavi RezervatorId PRE validacije jer je Required
                trail.RezervatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Ukloni iz ModelState da se ne validira kroz Bind
                ModelState.Remove("RezervatorId");
                ModelState.Remove("Rezervator");
                ModelState.Remove("TrailKonji");

                // Validacija konja - obavezno je izabrati najmanje jednog konja
                if (SelectedHorseIds == null || !SelectedHorseIds.Any())
                {
                    ModelState.AddModelError("", "Morate izabrati najmanje jednog konja za trail.");
                }

                if (ModelState.IsValid)
                {
                    // RezervatorId je već postavljen iznad

                    // Dodaj trail u bazu
                    _context.Add(trail);
                    await _context.SaveChangesAsync();

                    // Dodaj konje ako su odabrani
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

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Debug model state greške
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
                // Debug greške
                System.Diagnostics.Debug.WriteLine($"Greška pri dodavanju traila: {ex.Message}");
                ModelState.AddModelError("", "Dogodila se greška pri dodavanju traila. Molimo pokušajte ponovo.");
            }

            // Ako dođe do greške, ponovno učitaj konje
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

            var trail = await _context.Trails.FindAsync(id);
            if (trail == null)
            {
                return NotFound();
            }
            ViewData["RezervatorId"] = new SelectList(_context.Set<User>(), "Id", "Id", trail.RezervatorId);
            return View(trail);
        }

        // POST: Trail/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Naziv,Opis,Datum,RezervatorId")] Trail trail)
        {
            if (id != trail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trail);
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["RezervatorId"] = new SelectList(_context.Set<User>(), "Id", "Id", trail.RezervatorId);
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
            return RedirectToAction(nameof(Index));
        }

        private bool TrailExists(int id)
        {
            return _context.Trails.Any(e => e.Id == id);
        }
    }
}