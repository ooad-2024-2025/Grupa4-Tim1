using Jahacki_klub_Zeljeznicar.Data;
using Jahacki_klub_Zeljeznicar.Models;
using Jahacki_klub_Zeljeznicar.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;


namespace Jahacki_klub_Zeljeznicar.Controllers
{

    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // GET: List all users
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // GET: User details
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: Create user
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Kategorija = model.Kategorija,
                Nivo = model.Nivo
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // If creating a Clan member, automatically create their clanarina
                if (model.Kategorija == Kategorija.Clan)
                {
                    await CreateInitialClanarina(user.Id);
                }

                TempData["SuccessMessage"] = "Korisnik je uspješno kreiran.";
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }


        // GET: Edit user
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Ime = user.Ime,
                Prezime = user.Prezime,
                Kategorija = user.Kategorija,
                Nivo = user.Nivo
            };

            return View(model);
        }

        // POST: Edit user
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;
            user.Ime = model.Ime;
            user.Prezime = model.Prezime;
            user.Kategorija = model.Kategorija;
            user.Nivo = model.Nivo;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Update password if provided
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                    if (!passwordResult.Succeeded)
                    {
                        foreach (var error in passwordResult.Errors)
                            ModelState.AddModelError("", error.Description);
                        return View(model);
                    }
                }

                TempData["SuccessMessage"] = "Korisnik je uspješno ažuriran.";
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // GET: Delete confirmation
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: Delete user
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Check if trying to delete current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == user.Id)
            {
                TempData["ErrorMessage"] = "Ne možete obrisati svoj vlastiti račun.";
                return RedirectToAction("Index", "Dashboard");
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Korisnik je uspješno obrisan.";
            }
            else
            {
                TempData["ErrorMessage"] = "Greška pri brisanju korisnika.";
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Kategorija = model.Kategorija,
                Nivo = model.Nivo
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);


            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError("", "Pogrešan email ili lozinka.");
            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterClan() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterClan(RegisterClanViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Kategorija = Kategorija.Clan,
                Nivo = Nivo.Pocetnik
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Automatically create initial clanarina for new clan member
                await CreateInitialClanarina(user.Id);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult RegisterTrener() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterTrener(RegisterTrenerViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Kategorija = Kategorija.Trener,
                Nivo = null //Nivo.etnik

            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // You might want to assign clan member role here
                // await _userManager.AddToRoleAsync(user, "ClanMember");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult RegisterAdmin() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterAdmin(RegisterAdminViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Kategorija = Kategorija.Admin,
                Nivo = null

            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // You might want to assign clan member role here
                // await _userManager.AddToRoleAsync(user, "ClanMember");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterGuest(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterGuest(RegisterGuestViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);


            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Kategorija = Kategorija.Guest,
                Nivo = null

            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // You might want to assign clan member role here
                // await _userManager.AddToRoleAsync(user, "ClanMember");

                await _signInManager.SignInAsync(user, isPersistent: false);
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private async Task CreateInitialClanarina(string userId)
        {
            try
            {
                var novaClanarina = new Clanarina
                {
                    UserId = userId,
                    PocetakClanarine = DateTime.Now,
                    IstekClanarine = DateTime.Now.AddMonths(1)
                };

                _context.Clanarine.Add(novaClanarina);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the registration process
                Debug.WriteLine($"Error creating initial clanarina: {ex.Message}");
            }
        }

    }
}