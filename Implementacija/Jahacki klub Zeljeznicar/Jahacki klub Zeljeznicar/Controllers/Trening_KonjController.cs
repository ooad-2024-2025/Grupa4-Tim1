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
    public class Trening_KonjController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Trening_KonjController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trening_Konj
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TreningKonji.Include(t => t.Konj).Include(t => t.Trening);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Trening_Konj/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trening_Konj = await _context.TreningKonji
                .Include(t => t.Konj)
                .Include(t => t.Trening)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trening_Konj == null)
            {
                return NotFound();
            }

            return View(trening_Konj);
        }

        // GET: Trening_Konj/Create
        public IActionResult Create()
        {
            ViewData["KonjId"] = new SelectList(_context.Konji, "Id", "Ime");
            ViewData["TreningId"] = new SelectList(_context.Treninzi, "Id", "Naziv");
            return View();
        }

        // POST: Trening_Konj/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TreningId,KonjId")] Trening_Konj trening_Konj)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trening_Konj);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KonjId"] = new SelectList(_context.Konji, "Id", "Ime", trening_Konj.KonjId);
            ViewData["TreningId"] = new SelectList(_context.Treninzi, "Id", "Naziv", trening_Konj.TreningId);
            return View(trening_Konj);
        }

        // GET: Trening_Konj/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trening_Konj = await _context.TreningKonji.FindAsync(id);
            if (trening_Konj == null)
            {
                return NotFound();
            }
            ViewData["KonjId"] = new SelectList(_context.Konji, "Id", "Ime", trening_Konj.KonjId);
            ViewData["TreningId"] = new SelectList(_context.Treninzi, "Id", "Naziv", trening_Konj.TreningId);
            return View(trening_Konj);
        }

        // POST: Trening_Konj/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TreningId,KonjId")] Trening_Konj trening_Konj)
        {
            if (id != trening_Konj.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trening_Konj);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Trening_KonjExists(trening_Konj.Id))
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
            ViewData["KonjId"] = new SelectList(_context.Konji, "Id", "Ime", trening_Konj.KonjId);
            ViewData["TreningId"] = new SelectList(_context.Treninzi, "Id", "Naziv", trening_Konj.TreningId);
            return View(trening_Konj);
        }

        // GET: Trening_Konj/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trening_Konj = await _context.TreningKonji
                .Include(t => t.Konj)
                .Include(t => t.Trening)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trening_Konj == null)
            {
                return NotFound();
            }

            return View(trening_Konj);
        }

        // POST: Trening_Konj/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trening_Konj = await _context.TreningKonji.FindAsync(id);
            if (trening_Konj != null)
            {
                _context.TreningKonji.Remove(trening_Konj);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Trening_KonjExists(int id)
        {
            return _context.TreningKonji.Any(e => e.Id == id);
        }
    }
}
