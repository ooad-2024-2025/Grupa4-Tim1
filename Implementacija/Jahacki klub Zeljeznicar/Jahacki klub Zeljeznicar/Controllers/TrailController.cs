﻿using Jahacki_klub_Zeljeznicar.Data;
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
    public class TrailController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        private bool IsCurrentUserAdmin()
        {
            return User.IsInRole("Admin");
        }


        public TrailController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Trail

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var trails = await _context.Trails
                .Include(t => t.Rezervator)
                .OrderBy(t => t.Datum)
                .ToListAsync();

            var now = DateTime.Now;

            var viewModel = new ViewModels.TrailsIndexViewModel
            {
                RegisteredTrails = trails.Where(t => t.RezervatorId == currentUserId && t.Datum >= now).ToList(),
                AvailableTrails = trails.Where(t => t.RezervatorId == null && t.Datum >= now).ToList(),
                PastTrails = trails.Where(t => t.RezervatorId == currentUserId && t.Datum < now).ToList(),
                LoggedInUserId = currentUserId
            };

            return View(viewModel);
        }



        public async Task<IActionResult> Details(int id)
        {
            var trail = await _context.Trails
                .Include(t => t.Rezervator)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trail == null)
                return NotFound();

            // Count horses for this trail
            int maxHorses = await _context.TrailKonji.CountAsync(tk => tk.TrailId == id);

            var viewModel = new ViewModels.TrailDetailsViewModel
            {
                Trail = trail,
                MaxHorses = maxHorses,
                LoggedInUserId = _userManager.GetUserId(User)
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rezervisi(int trailId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Save return URL and redirect to RegisterGuest
                return RedirectToAction("RegisterGuest", "Account", new { returnUrl = Url.Action("Details", "Trail", new { id = trailId }) });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var trail = await _context.Trails.FindAsync(trailId);
            if (trail == null)
                return NotFound();

            trail.RezervatorId = userId;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }
        // GET: Trail/Create
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create()
        {
            var konji = await _context.Konji.ToListAsync();
            ViewBag.Konji = konji;
            return View();
        }

        // POST: Trail/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([Bind("Naziv,Opis,Datum")] Trail trail, List<int> SelectedHorseIds)
        {
            try
            {
                // Trail is not reserved by default
                trail.RezervatorId = null;

                ModelState.Remove("RezervatorId");
                ModelState.Remove("Rezervator");
                ModelState.Remove("TrailKonji");

                if (string.IsNullOrWhiteSpace(trail.Naziv))
                {
                    ModelState.AddModelError(nameof(trail.Naziv), "Naziv traila je obavezan.");
                }
                if (trail.Datum < DateTime.Today)
                {
                    ModelState.AddModelError(nameof(trail.Datum), "Datum traila ne može biti u prošlosti.");
                }
                if (string.IsNullOrWhiteSpace(trail.Opis))
                {
                    ModelState.AddModelError(nameof(trail.Opis), "Opis traila je obavezan.");
                }
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

                    if (IsCurrentUserAdmin())
                        return RedirectToAction("Index", "Dashboard");
                    else
                        return RedirectToAction("Index");

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
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Naziv,Opis,Datum,RezervatorId")] Trail trail, List<int> SelectedHorseIds)
        {
            if (id != trail.Id)
            {
                return NotFound();
            }

            ModelState.Remove("TrailKonji");

            if (string.IsNullOrWhiteSpace(trail.Naziv))
            {
                ModelState.AddModelError(nameof(trail.Naziv), "Naziv traila je obavezan.");
            }
            if (trail.Datum < DateTime.Today)
            {
                ModelState.AddModelError(nameof(trail.Datum), "Datum traila ne može biti u prošlosti.");
            }
            if (string.IsNullOrWhiteSpace(trail.Opis))
            {
                ModelState.AddModelError(nameof(trail.Opis), "Opis traila je obavezan.");
            }
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
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trail = await _context.Trails.FindAsync(id);
            if (trail != null)
            {
                _context.Trails.Remove(trail);
            }

            await _context.SaveChangesAsync();
            if (IsCurrentUserAdmin())
                return RedirectToAction("Index", "Dashboard");
            else
                return RedirectToAction("Index");

        }

        private bool TrailExists(int id)
        {
            return _context.Trails.Any(e => e.Id == id);
        }
    }
}
