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
    public class Trail_KonjController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Trail_KonjController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trail_Konj
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TrailKonji.Include(t => t.Konj).Include(t => t.Trail);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Trail_Konj/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trail_Konj = await _context.TrailKonji
                .Include(t => t.Konj)
                .Include(t => t.Trail)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trail_Konj == null)
            {
                return NotFound();
            }

            return View(trail_Konj);
        }

        // GET: Trail_Konj/Create
        public IActionResult Create()
        {
            ViewData["KonjId"] = new SelectList(_context.Konji, "Id", "Ime");
            ViewData["TrailId"] = new SelectList(_context.Trails, "Id", "Naziv");
            return View();
        }

        // POST: Trail_Konj/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TrailId,KonjId")] Trail_Konj trail_Konj)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trail_Konj);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KonjId"] = new SelectList(_context.Konji, "Id", "Ime", trail_Konj.KonjId);
            ViewData["TrailId"] = new SelectList(_context.Trails, "Id", "Naziv", trail_Konj.TrailId);
            return View(trail_Konj);
        }

        // GET: Trail_Konj/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trail_Konj = await _context.TrailKonji.FindAsync(id);
            if (trail_Konj == null)
            {
                return NotFound();
            }
            ViewData["KonjId"] = new SelectList(_context.Konji, "Id", "Ime", trail_Konj.KonjId);
            ViewData["TrailId"] = new SelectList(_context.Trails, "Id", "Naziv", trail_Konj.TrailId);
            return View(trail_Konj);
        }

        // POST: Trail_Konj/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TrailId,KonjId")] Trail_Konj trail_Konj)
        {
            if (id != trail_Konj.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trail_Konj);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Trail_KonjExists(trail_Konj.Id))
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
            ViewData["KonjId"] = new SelectList(_context.Konji, "Id", "Ime", trail_Konj.KonjId);
            ViewData["TrailId"] = new SelectList(_context.Trails, "Id", "Naziv", trail_Konj.TrailId);
            return View(trail_Konj);
        }

        // GET: Trail_Konj/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trail_Konj = await _context.TrailKonji
                .Include(t => t.Konj)
                .Include(t => t.Trail)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trail_Konj == null)
            {
                return NotFound();
            }

            return View(trail_Konj);
        }

        // POST: Trail_Konj/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trail_Konj = await _context.TrailKonji.FindAsync(id);
            if (trail_Konj != null)
            {
                _context.TrailKonji.Remove(trail_Konj);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Trail_KonjExists(int id)
        {
            return _context.TrailKonji.Any(e => e.Id == id);
        }
    }
}
