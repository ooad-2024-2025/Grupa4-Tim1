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
    public class Trening_UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Trening_UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trening_User
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TreningUsers.Include(t => t.Trening).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Trening_User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trening_User = await _context.TreningUsers
                .Include(t => t.Trening)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trening_User == null)
            {
                return NotFound();
            }

            return View(trening_User);
        }

        // GET: Trening_User/Create
        public IActionResult Create()
        {
            ViewData["TreningId"] = new SelectList(_context.Treninzi, "Id", "Naziv");
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id");
            return View();
        }

        // POST: Trening_User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TreningId,UserId")] Trening_User trening_User)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trening_User);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TreningId"] = new SelectList(_context.Treninzi, "Id", "Naziv", trening_User.TreningId);
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", trening_User.UserId);
            return View(trening_User);
        }

        // GET: Trening_User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trening_User = await _context.TreningUsers.FindAsync(id);
            if (trening_User == null)
            {
                return NotFound();
            }
            ViewData["TreningId"] = new SelectList(_context.Treninzi, "Id", "Naziv", trening_User.TreningId);
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", trening_User.UserId);
            return View(trening_User);
        }

        // POST: Trening_User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TreningId,UserId")] Trening_User trening_User)
        {
            if (id != trening_User.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trening_User);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Trening_UserExists(trening_User.Id))
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
            ViewData["TreningId"] = new SelectList(_context.Treninzi, "Id", "Naziv", trening_User.TreningId);
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", trening_User.UserId);
            return View(trening_User);
        }

        // GET: Trening_User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trening_User = await _context.TreningUsers
                .Include(t => t.Trening)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trening_User == null)
            {
                return NotFound();
            }

            return View(trening_User);
        }

        // POST: Trening_User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trening_User = await _context.TreningUsers.FindAsync(id);
            if (trening_User != null)
            {
                _context.TreningUsers.Remove(trening_User);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Trening_UserExists(int id)
        {
            return _context.TreningUsers.Any(e => e.Id == id);
        }
    }
}
