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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([Bind("Ime,Opis,Spol,Boja")] Konj konj)
        {
            // Debugging
            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
    
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
                // Kreiraj novi objekt samo s osnovnim svojstvima
                var noviKonj = new Konj
                {
                    Ime = konj.Ime,
                    Opis = konj.Opis,
                    Spol = konj.Spol,
                    Boja = konj.Boja
                };

                _context.Konji.Add(noviKonj);
                var result = await _context.SaveChangesAsync();
        
                System.Diagnostics.Debug.WriteLine($"Rows affected: {result}");

                TempData["SuccessMessage"] = "Konj je uspješno dodan!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
        
                ModelState.AddModelError("", $"Došlo je do greške prilikom čuvanja: {ex.Message}");
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ime,Opis,Spol,Boja")] Konj konj)
        {
            if (id != konj.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(konj);
                    await _context.SaveChangesAsync();

                    // Dodaj success poruku
                    TempData["SuccessMessage"] = "Konj je uspješno ažuriran!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KonjExists(konj.Id))
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
            var konj = await _context.Konji.FindAsync(id);
            if (konj != null)
            {
                _context.Konji.Remove(konj);
            }

            await _context.SaveChangesAsync();

            // Dodaj success poruku
            TempData["SuccessMessage"] = "Konj je uspješno uklonjen!";

            return RedirectToAction(nameof(Index));
        }

        private bool KonjExists(int id)
        {
            return _context.Konji.Any(e => e.Id == id);
        }
    }
}