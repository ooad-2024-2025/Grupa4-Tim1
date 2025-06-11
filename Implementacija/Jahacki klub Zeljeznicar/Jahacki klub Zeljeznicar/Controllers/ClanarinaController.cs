using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Jahacki_klub_Zeljeznicar.Data;
using Jahacki_klub_Zeljeznicar.Models;

namespace Jahacki_klub_Zeljeznicar.Controllers
{
    public class ClanarinaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public ClanarinaController(ApplicationDbContext context, UserManager<User> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        // GET: Clanarina - Prikaz članarine trenutnog korisnika
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Fetch the latest clanarina by IstekClanarine descending
            var clanarina = await _context.Clanarine
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IstekClanarine)
                .FirstOrDefaultAsync();

            if (clanarina == null)
            {
                // If no clanarina exists, create one with expired dates
                clanarina = new Clanarina
                {
                    UserId = userId,
                    PocetakClanarine = DateTime.Now.AddDays(-1),
                    IstekClanarine = DateTime.Now.AddDays(-1),
                    User = await _userManager.FindByIdAsync(userId)
                };
            }

            return View(clanarina);
        }


        // POST: Clanarina/ProduziClanarinu
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProduziClanarinu(int mjeseci)
        {
            if (mjeseci != 1 && mjeseci != 6 && mjeseci != 12)
            {
                TempData["Error"] = "Nevaljan broj mjeseci za produžavanje.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);

            // Get all clanarinas for this user, ordered by expiration date descending
            var allClanarinas = await _context.Clanarine
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IstekClanarine)
                .ToListAsync();

            DateTime noviPocetak;
            DateTime noviIstek;

            if (allClanarinas.Any())
            {
                // Get the latest expiration date from all clanarinas
                var najnoviji_istek = allClanarinas.First().IstekClanarine;

                // If the latest expiration is in the future, extend from that date
                // Otherwise, start from today
                noviPocetak = najnoviji_istek > DateTime.Now ? najnoviji_istek : DateTime.Now;
                noviIstek = noviPocetak.AddMonths(mjeseci);

                // Update the most recent clanarina
                var postojecaClanarina = allClanarinas.First();
                postojecaClanarina.IstekClanarine = noviIstek;

                // If the start date needs updating (when extending expired membership)
                if (postojecaClanarina.IstekClanarine <= DateTime.Now)
                {
                    postojecaClanarina.PocetakClanarine = DateTime.Now;
                }

                _context.Update(postojecaClanarina);
            }
            else
            {
                // Create new clanarina if none exists
                noviPocetak = DateTime.Now;
                noviIstek = noviPocetak.AddMonths(mjeseci);
                var novaClanarina = new Clanarina
                {
                    UserId = userId,
                    PocetakClanarine = noviPocetak,
                    IstekClanarine = noviIstek
                };

                _context.Add(novaClanarina);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Članarina je uspješno produžena za {mjeseci} mjesec(i). Važeća do: {noviIstek:dd.MM.yyyy}";
            return RedirectToAction(nameof(Index));
        }

        // GET: Clanarina/Potvrdi - Stranica za potvrdu produžavanja
        [Authorize]
        public async Task<IActionResult> Potvrdi(int mjeseci)
        {
            if (mjeseci != 1 && mjeseci != 6 && mjeseci != 12)
            {
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);
            var postojecaClanarina = await _context.Clanarine
                .FirstOrDefaultAsync(c => c.UserId == userId);

            decimal cijena = mjeseci switch
            {
                1 => 70.00m,
                6 => 400.00m,
                12 => 700.00m,
                _ => 0
            };

            DateTime noviIstek;
            if (postojecaClanarina != null && postojecaClanarina.IstekClanarine > DateTime.Now)
            {
                noviIstek = postojecaClanarina.IstekClanarine.AddMonths(mjeseci);
            }
            else
            {
                noviIstek = DateTime.Now.AddMonths(mjeseci);
            }

            var model = new
            {
                Mjeseci = mjeseci,
                Cijena = cijena,
                NoviIstek = noviIstek,
                TrenutniIstek = postojecaClanarina?.IstekClanarine
            };

            ViewBag.Model = model;
            return View();
        }

        // Admin funkcije - samo za administratore
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var applicationDbContext = _context.Clanarine.Include(c => c.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Clanarina/Details/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var users = await _context.Users.ToListAsync();
            ViewData["UserId"] = new SelectList(users, "Id", "UserName");
            return View();
        }

        // POST: Clanarina/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,PocetakClanarine,IstekClanarine,UserId")] Clanarina clanarina)
        {
            if (ModelState.IsValid)
            {
                // Provjeri da li korisnik već ima članarinu
                var postojecaClanarina = await _context.Clanarine
                    .FirstOrDefaultAsync(c => c.UserId == clanarina.UserId);

                if (postojecaClanarina != null)
                {
                    ModelState.AddModelError("UserId", "Korisnik već ima članarinu. Koristite opciju uređivanja.");
                }
                else
                {
                    _context.Add(clanarina);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Članarina je uspješno kreirana.";
                    return RedirectToAction(nameof(AdminIndex));
                }
            }

            var users = await _context.Users.ToListAsync();
            ViewData["UserId"] = new SelectList(users, "Id", "UserName", clanarina.UserId);
            return View(clanarina);
        }

        // GET: Clanarina/Edit/5
        [Authorize(Roles = "Admin")]
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

            var users = await _context.Users.ToListAsync();
            ViewData["UserId"] = new SelectList(users, "Id", "UserName", clanarina.UserId);
            return View(clanarina);
        }

        // POST: Clanarina/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
                    TempData["Success"] = "Članarina je uspješno ažurirana.";
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
                return RedirectToAction(nameof(AdminIndex));
            }

            var users = await _context.Users.ToListAsync();
            ViewData["UserId"] = new SelectList(users, "Id", "UserName", clanarina.UserId);
            return View(clanarina);
        }

        // GET: Clanarina/Delete/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clanarina = await _context.Clanarine.FindAsync(id);
            if (clanarina != null)
            {
                _context.Clanarine.Remove(clanarina);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Članarina je uspješno obrisana.";
            }

            return RedirectToAction(nameof(AdminIndex));
        }

        // GET: Clanarina/MojeProduzavanje - Historija produžavanja za korisnika
        [Authorize]
        public async Task<IActionResult> MojeProduzavanje()
        {
            var userId = _userManager.GetUserId(User);
            var clanarina = await _context.Clanarine
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return View(clanarina);
        }

        // API metoda za provjeru statusa članarine
        [Authorize]
        public async Task<IActionResult> StatusClanarine()
        {
            var userId = _userManager.GetUserId(User);
            var clanarina = await _context.Clanarine
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (clanarina == null)
            {
                return Json(new { status = "nema_clanarinu", message = "Nemate aktivnu članarinu" });
            }

            if (clanarina.IstekClanarine < DateTime.Now)
            {
                return Json(new { status = "istekla", message = "Članarina je istekla", datum = clanarina.IstekClanarine.ToString("dd.MM.yyyy") });
            }

            var danaDoIsteka = (clanarina.IstekClanarine - DateTime.Now).Days;
            if (danaDoIsteka <= 30)
            {
                return Json(new { status = "uskoro_istice", message = $"Članarina ističe za {danaDoIsteka} dana", datum = clanarina.IstekClanarine.ToString("dd.MM.yyyy") });
            }

            return Json(new { status = "aktivna", message = "Članarina je aktivna", datum = clanarina.IstekClanarine.ToString("dd.MM.yyyy") });
        }

        private bool ClanarinaExists(int id)
        {
            return _context.Clanarine.Any(e => e.Id == id);
        }

        // GET: Clanarina/Placanje
        [Authorize]
        public IActionResult Placanje(int mjeseci)
        {
            if (mjeseci != 1 && mjeseci != 6 && mjeseci != 12)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Mjeseci = mjeseci;

            // Proslijedi cijenu na view
            decimal cijena = mjeseci switch
            {
                1 => 70.00m,
                6 => 400.00m,
                12 => 700.00m,
                _ => 0
            };

            ViewBag.Cijena = cijena;
            return View();
        }

        // POST: Clanarina/PotvrdiPlacanje
        // Corrected PotvrdiPlacanje method in ClanarinaController.cs
[Authorize]
[HttpPost]
public async Task<IActionResult> PotvrdiPlacanje(string ImePrezime, string BrojKartice, string CVV, int Opcija)
{
    if (string.IsNullOrWhiteSpace(BrojKartice) || string.IsNullOrWhiteSpace(ImePrezime) ||
        string.IsNullOrWhiteSpace(CVV) || (Opcija != 1 && Opcija != 6 && Opcija != 12))
    {
        TempData["Error"] = "Svi podaci moraju biti uneseni ispravno.";
        return RedirectToAction(nameof(Placanje), new { mjeseci = Opcija });
    }

    var userId = _userManager.GetUserId(User);
    var user = await _userManager.FindByIdAsync(userId);
        
    // Get all clanarinas for this user, ordered by expiration date descending
    var allClanarinas = await _context.Clanarine
        .Where(c => c.UserId == userId)
        .OrderByDescending(c => c.IstekClanarine)
        .ToListAsync();

    DateTime noviPocetak;
    DateTime noviIstek;

    if (allClanarinas.Any())
    {
        // Get the latest expiration date from all clanarinas
        var najnoviji_istek = allClanarinas.First().IstekClanarine;
        
        // If the latest expiration is in the future, extend from that date
        // Otherwise, start from today
        noviPocetak = najnoviji_istek > DateTime.Now ? najnoviji_istek : DateTime.Now;
        noviIstek = noviPocetak.AddMonths(Opcija);
    }
    else
    {
        // If no existing clanarinas, start from today
        noviPocetak = DateTime.Now;
        noviIstek = noviPocetak.AddMonths(Opcija);
    }

    // Always create a new clanarina for each payment
    var novaClanarina = new Clanarina
    {
        UserId = userId,
        PocetakClanarine = noviPocetak,
        IstekClanarine = noviIstek
    };

    _context.Add(novaClanarina);
    await _context.SaveChangesAsync();

    // Send email confirmation
    try
    {
        await PosaljiEmailPotvrdu(user.Email, user.UserName, Opcija, noviIstek);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Greška pri slanju email-a: {ex.Message}");
    }

    TempData["Success"] = $"Plaćanje je uspješno izvršeno! Članarina je produžena za {Opcija} mjesec(i). Važeća do: {noviIstek:dd.MM.yyyy}. Potvrda je poslana na vaš email.";
    return RedirectToAction(nameof(Index));
}

        private async Task PosaljiEmailPotvrdu(string emailAdresa, string korisnickoIme, int mjeseci, DateTime datumIsteka)
        {
            try
            {
                Console.WriteLine($"DEBUG EMAIL: Započinje slanje email-a na adresu: {emailAdresa}");

                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPortStr = _configuration["EmailSettings:SmtpPort"] ?? "587";
                var emailFrom = _configuration["EmailSettings:FromEmail"];
                var emailPassword = _configuration["EmailSettings:Password"];

                Console.WriteLine($"DEBUG EMAIL: SMTP Server: {smtpServer}");
                Console.WriteLine($"DEBUG EMAIL: SMTP Port: {smtpPortStr}");
                Console.WriteLine($"DEBUG EMAIL: From Email: {emailFrom}");
                Console.WriteLine($"DEBUG EMAIL: Password length: {emailPassword?.Length ?? 0}");

                if (string.IsNullOrEmpty(emailFrom) || string.IsNullOrEmpty(emailPassword))
                {
                    throw new InvalidOperationException("Email konfiguracija nije postavljena.");
                }

                if (!int.TryParse(smtpPortStr, out int smtpPort))
                {
                    smtpPort = 587; // default port
                }

                Console.WriteLine($"DEBUG EMAIL: Kreiranje SMTP client-a...");

                using var client = new SmtpClient(smtpServer, smtpPort);

                // Gmail specifične postavke
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(emailFrom, emailPassword);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Timeout = 30000; // 30 sekundi timeout

                Console.WriteLine($"DEBUG EMAIL: SMTP client konfigurisan");

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailFrom, "Jahački klub Željezničar"),
                    Subject = "Potvrda plaćanja članarine - Jahački klub Željezničar",
                    Body = KreirajEmailSadrzaj(korisnickoIme, mjeseci, datumIsteka),
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };

                mailMessage.To.Add(new MailAddress(emailAdresa));

                Console.WriteLine($"DEBUG EMAIL: Mail message kreiran, pokušavam slanje...");

                await client.SendMailAsync(mailMessage);

                Console.WriteLine($"DEBUG EMAIL: Email uspješno poslan!");
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"DEBUG EMAIL - SMTP Greška: {smtpEx.Message}");
                Console.WriteLine($"DEBUG EMAIL - SMTP Status Code: {smtpEx.StatusCode}");
                Console.WriteLine($"DEBUG EMAIL - SMTP Stack Trace: {smtpEx.StackTrace}");
                throw new Exception($"SMTP greška pri slanju email-a: {smtpEx.Message} (Status: {smtpEx.StatusCode})", smtpEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG EMAIL - Opća greška: {ex.Message}");
                Console.WriteLine($"DEBUG EMAIL - Stack Trace: {ex.StackTrace}");
                throw new Exception($"Greška pri slanju email-a: {ex.Message}", ex);
            }
        }

        private string KreirajEmailSadrzaj(string korisnickoIme, int mjeseci, DateTime datumIsteka)
        {
            decimal cijena = mjeseci switch
            {
                1 => 70.00m,
                6 => 400.00m,
                12 => 700.00m,
                _ => 0
            };

            return $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; background-color: #f8f9fa; margin: 0; padding: 20px; }}
                .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); }}
                .header {{ text-align: center; border-bottom: 2px solid #343a40; padding-bottom: 20px; margin-bottom: 30px; }}
                .logo {{ color: #343a40; font-size: 24px; font-weight: bold; }}
                .content {{ line-height: 1.6; }}
                .info-box {{ background-color: #e9ecef; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6; color: #6c757d; font-size: 14px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <div class='logo'>🐎 Jahački klub Željezničar 🔵</div>
                </div>
                
                <div class='content'>
                    <h2>Potvrda plaćanja članarine</h2>
                    
                    <p>Poštovani/a <strong>{korisnickoIme}</strong>,</p>
                    
                    <p>Zahvaljujemo Vam što ste produžili članarinu u našem jahačkom klubu. Vaše plaćanje je uspješno obrađeno.</p>
                    
                    <div class='info-box'>
                        <h3>Detalji plaćanja:</h3>
                        <ul>
                            <li><strong>Paket:</strong> {mjeseci} {(mjeseci == 1 ? "mjesec" : "mjeseci")}</li>
                            <li><strong>Iznos:</strong> {cijena:F2} KM</li>
                            <li><strong>Datum plaćanja:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</li>
                            <li><strong>Važeća do:</strong> {datumIsteka:dd.MM.yyyy}</li>
                        </ul>
                    </div>
                    
                    <p>Vaša članarina je sada aktivna i možete koristiti sve usluge našeg kluba.</p>
                    
                    <p>Ako imate bilo kakvih pitanja, slobodno nas kontaktirajte.</p>
                    
                    <p>Srdačan pozdrav,<br>
                    <strong>Tim Jahačkog kluba Željezničar</strong></p>
                </div>
                
                <div class='footer'>
                    <p>Ova poruka je automatski generirana. Molimo ne odgovarajte na ovaj email.</p>
                    <p>&copy; 2025 Jahački klub Željezničar. Sva prava zadržana.</p>
                </div>
            </div>
        </body>
        </html>";
        }
    }
}