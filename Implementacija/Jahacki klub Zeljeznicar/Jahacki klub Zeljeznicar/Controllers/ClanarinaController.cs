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
    public class ClanarinaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClanarinaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clanarina
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Clanarine.Include(c => c.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Clanarina/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clanarina = await _context.Clanarine
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clanarina == null)
            {
                return NotFound();
            }

            return View(clanarina);
        }

        // GET: Clanarina/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id");
            return View();
        }

        // POST: Clanarina/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PocetakClanarine,IstekClanarine,UserId")] Clanarina clanarina)
        {
            if (ModelState.IsValid)
            {
                _context.Add(clanarina);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", clanarina.UserId);
            return View(clanarina);
        }

        // GET: Clanarina/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clanarina = await _context.Clanarine.FindAsync(id);
            if (clanarina == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", clanarina.UserId);
            return View(clanarina);
        }

        // POST: Clanarina/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PocetakClanarine,IstekClanarine,UserId")] Clanarina clanarina)
        {
            if (id != clanarina.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clanarina);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClanarinaExists(clanarina.Id))
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
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", clanarina.UserId);
            return View(clanarina);
        }

        // GET: Clanarina/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clanarina = await _context.Clanarine
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clanarina == null)
            {
                return NotFound();
            }

            return View(clanarina);
        }

        // POST: Clanarina/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clanarina = await _context.Clanarine.FindAsync(id);
            if (clanarina != null)
            {
                _context.Clanarine.Remove(clanarina);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClanarinaExists(int id)
        {
            return _context.Clanarine.Any(e => e.Id == id);
        }
    }
}
