using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Plataforma.Models;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Plataforma.Models.Administracion;

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
            return View(new LoginModel()); 
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> login(LoginModel model)
            {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, model.RememberMe);

                        var roles = await _userManager.GetRolesAsync(user);

                        if (roles.Contains("Administrador"))
                            return RedirectToAction("panel", "administrador");
                        if (roles.Contains("Estudiante"))
                            return RedirectToAction("Index", "inicio");
                        if (roles.Contains("Profesor"))
                            return RedirectToAction("Inicio", "cursos");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Inicio de sesión inválido.");
                        return View("Index", model); 
                        
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos."); // to not reveal that there is not a user with that email
                    return View("Index", model); 
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
        // Reseatear contraseña
        [HttpGet]
        [Route("Cuenta/ResetearContraseña")]
        public async Task<IActionResult> SetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Link inválido.");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            // ✅ Prevent link reuse
            if (user.PasswordHash != null)
            {
                TempData["Message"] = "Tu cuenta ya fue activada.";
                return RedirectToAction(nameof(Index));
            }

            var model = new SetPasswordViewModel
            {
                UserId = Guid.Parse(userId),
                Token = token
            };

            return View("ResetearContraseña", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Cuenta/ResetearContraseña")]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View("ResetearContraseña", model);

            var user = await _userManager.FindByIdAsync(model.UserId.ToString());

            if (user == null)
                return NotFound();

            // ✅ Prevent reuse again (security layer)
            if (user.PasswordHash != null)
            {
                TempData["Message"] = "La contraseña ya fue creada.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ Decode token
            var decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(model.Token));

            var result = await _userManager.ResetPasswordAsync(
                user,
                decodedToken,
                model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(PasswordCreated));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View("ResetearContraseña", model);
        }
        [HttpGet]
        [Route("Cuenta/ContraseñaCreada")]
        public IActionResult PasswordCreated()
        {
            return View("ContraseñaCreada");
        }
    }
}
