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
        public IActionResult Create()
        {
            ViewData["RezervatorId"] = new SelectList(_context.Set<User>(), "Id", "Id");
            return View();
        }

        // POST: Trail/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Naziv,Opis,Datum,RezervatorId")] Trail trail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RezervatorId"] = new SelectList(_context.Set<User>(), "Id", "Id", trail.RezervatorId);
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
