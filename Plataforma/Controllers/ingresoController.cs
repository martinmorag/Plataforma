using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Plataforma.Models;

namespace Plataforma.Controllers
{
    public class ingresoController : Controller
    {
        private readonly SignInManager<UsuarioIdentidad> _signInManager;
        private readonly UserManager<UsuarioIdentidad> _userManager;
        public ingresoController(SignInManager<UsuarioIdentidad> signInManager, UserManager<UsuarioIdentidad> userManager) // Corrected type here
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View(new LoginModel()); // Pass a new LoginModel to the Index view so the form works
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> login(LoginModel model)
            {
            if (ModelState.IsValid)
            {
                var normalizedEmail = _userManager.NormalizeEmail(model.Email);
                var user = await _userManager.FindByEmailAsync(normalizedEmail);

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        if (await _userManager.IsInRoleAsync(user, "Administrador"))
                            return RedirectToAction("panel", "administrador");
                        if (await _userManager.IsInRoleAsync(user, "Estudiante"))
                            return RedirectToAction("Index", "inicio");
                        if (await _userManager.IsInRoleAsync(user, "Profesor"))
                            return RedirectToAction("Dashboard", "profesor");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Inicio de sesión inválido.");
                        Console.WriteLine("failed");
                        return View("Index", model); // Or View("Index", model) if your login form is on the Index page.
                        
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ningún usuario encontrado con ese email.");
                    Console.WriteLine("failed too");
                    return View("Index", model); // Or View("Index", model) if your login form is on the Index page.
                }
            }
            return View("Index", model); // Or View("Index", model) if your login form is on the Index page.
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "ingreso");
        }
    }
}
