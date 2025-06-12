using Jahacki_klub_Zeljeznicar.Data;
using Jahacki_klub_Zeljeznicar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jahacki_klub_Zeljeznicar.Controllers
{
    public class KonjController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KonjController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Konj
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Konji.ToListAsync());
        }

        // GET: Konj/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konj = await _context.Konji
                .FirstOrDefaultAsync(m => m.Id == id);
            if (konj == null)
            {
                return NotFound();
            }

            return View(konj);
        }

        // GET: Konj/Create
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create()
        {
            // Proslijedi prazan model za binding
            return View(new Konj());
        }

        // POST: Konj/Create
        // POST: Konj/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([Bind("Ime,Opis,Spol,Boja")] Konj konj)
        {
            if (string.IsNullOrWhiteSpace(konj.Ime))
            {
                ModelState.AddModelError(nameof(konj.Ime), "Ime konja je obavezno.");
            }

            if (konj.Ime?.Length > 50)
            {
                ModelState.AddModelError(nameof(konj.Ime), "Ime konja ne može biti duže od 50 karaktera.");
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    System.Diagnostics.Debug.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return View(konj);
            }

            try
            {
                var noviKonj = new Konj
                {
                    Ime = konj.Ime,
                    Opis = konj.Opis,
                    Spol = konj.Spol,
                    Boja = konj.Boja
                };

                _context.Konji.Add(noviKonj);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Konj je uspješno dodan!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Došlo je do greške prilikom čuvanja: {ex.Message}");
                return View(konj);
            }
        }

        // GET: Konj/Edit/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konj = await _context.Konji.FindAsync(id);
            if (konj == null)
            {
                return NotFound();
            }
            return View(konj);
        }

        // POST: Konj/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ime,Opis,Spol,Boja")] Konj konj)
        {
            if (id != konj.Id)
            {
                ModelState.AddModelError(string.Empty, "ID konja se ne poklapa.");
                return View(konj);
            }

            if (string.IsNullOrWhiteSpace(konj.Ime))
            {
                ModelState.AddModelError(nameof(konj.Ime), "Ime konja je obavezno.");
            }

            if (konj.Ime?.Length > 50)
            {
                ModelState.AddModelError(nameof(konj.Ime), "Ime konja ne može biti duže od 50 karaktera.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(konj);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Konj je uspješno ažuriran!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!KonjExists(konj.Id))
                    {
                        ModelState.AddModelError(string.Empty, "Konj nije pronađen u bazi podataka.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"Došlo je do greške prilikom ažuriranja: {ex.Message}");
                    }
                }
            }
            return View(konj);
        }

        // GET: Konj/Delete/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konj = await _context.Konji
                .FirstOrDefaultAsync(m => m.Id == id);
            if (konj == null)
            {
                return NotFound();
            }

            return View(konj);
        }

        // POST: Konj/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var konj = await _context.Konji.FindAsync(id);
                if (konj == null)
                {
                    ModelState.AddModelError(string.Empty, "Konj nije pronađen.");
                    return RedirectToAction(nameof(Index));
                }

                _context.Konji.Remove(konj);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Konj je uspješno uklonjen!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Došlo je do greške prilikom brisanja: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool KonjExists(int id)
        {
            return _context.Konji.Any(e => e.Id == id);
        }
    }
}