using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Jahacki_klub_Zeljeznicar.Data;
using Jahacki_klub_Zeljeznicar.Models;

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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Konj/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ime,Opis,Spol,Boja")] Konj konj)
        {
            if (ModelState.IsValid)
            {
                _context.Add(konj);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(konj);
        }

        // GET: Konj/Edit/5
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var konj = await _context.Konji.FindAsync(id);
            if (konj != null)
            {
                _context.Konji.Remove(konj);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KonjExists(int id)
        {
            return _context.Konji.Any(e => e.Id == id);
        }
    }
}
