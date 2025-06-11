using Jahacki_klub_Zeljeznicar.Models;
using Jahacki_klub_Zeljeznicar.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Jahacki_klub_Zeljeznicar.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
                return RedirectToAction("Index", "Home");
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
                Debug.WriteLine("Login successful for user: " + model.Email);
                return RedirectToAction("Index", "Home");
            }
            if (result.IsLockedOut)
                
            {
                Debug.WriteLine("Login failed for user: " + model.Email);
                return View("Lockout");
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
                // You might want to assign clan member role here
                // await _userManager.AddToRoleAsync(user, "ClanMember");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }
        [HttpGet]
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
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }
        
        [HttpGet]
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
                return RedirectToAction("Index", "Home");
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
                return RedirectToAction("Index", "Home");
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
    }

}
