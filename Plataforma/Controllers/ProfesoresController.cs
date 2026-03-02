using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Models.Administracion;
using Plataforma.Models;
using Plataforma.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Plataforma.Controllers
{
    [Authorize(Roles = "Profesor, Administrador")]
    public class ProfesoresController : Controller
    {
        private readonly PlataformaContext _context;
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public ProfesoresController(PlataformaContext context, UserManager<UsuarioIdentidad> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [HttpPost]
        public async Task<IActionResult> Create(AdministracionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = new Profesor
                {
                    Id = Guid.NewGuid(),
                    Nombre = model.RegistroProfesor.Nombre,
                    Apellido = model.RegistroProfesor.Apellido,
                    Email = model.RegistroProfesor.Email,
                    UserName = model.RegistroProfesor.Email
                };

                var result = await _userManager.CreateAsync(usuario);

                if (result.Succeeded)
                {
                    // Asegúrate de que el rol "Profesor" existe
                    if (!await _roleManager.RoleExistsAsync("Profesor"))
                    {
                        var roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>("Profesor"));
                        if (!roleResult.Succeeded)
                        {
                            // Manejar error al crear el rol
                            ModelState.AddModelError(string.Empty, "Error al crear el rol 'Profesor'.");
                            return View("~/Views/administrador/profesores/agregar.cshtml", model);
                        }
                    }

                    var roleAssignmentResult = await _userManager.AddToRoleAsync(usuario, "Profesor");
                    if (roleAssignmentResult.Succeeded)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                        var encodedToken = WebEncoders.Base64UrlEncode(
                        Encoding.UTF8.GetBytes(token));
                        var setupLink = Url.Action(
                            "ResetearContraseña",
                            "Cuenta",
                            new
                            {
                                userId = usuario.Id,
                                token = encodedToken
                            },
                            Request.Scheme);
                        TempData["SetupProfesoresLink"] = setupLink;

                        await _context.SaveChangesAsync();
                        return RedirectToAction("panel", "administrador"); 
                    }
                    else
                    {
                        foreach (var error in roleAssignmentResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View("~/Views/administrador/profesores/agregar.cshtml", model);
                    }
                }
                else
                {
                    // Manejar errores de creación de usuario
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("~/Views/administrador/profesores/agregar.cshtml");
                }
            }
            return View("~/Views/administrador/profesores/agregar.cshtml", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdministracionViewModel model)
        {
            //var id = Guid.Parse(Id);
            if (model.ProfesorAEditar?.Id != model.ProfesorAEditar?.Id)
            {
                return NotFound();
            }

            Guid id = model.ProfesorAEditar.Id;

            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByIdAsync(id.ToString());
                if (usuario == null)
                {
                    return NotFound();
                }

                usuario.Nombre = model.ProfesorAEditar.Nombre;
                usuario.Apellido = model.ProfesorAEditar.Apellido;
                usuario.Email = model.ProfesorAEditar.Email;
                usuario.UserName = model.ProfesorAEditar.Email;

                var updateResult = await _userManager.UpdateAsync(usuario);
                if (updateResult.Succeeded)
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // Manejo de concurrencia
                        if (!UsuarioIdentidadExists(usuario.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction("panel", "administrador");
                }
                else
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("~/Views/administrador/profesores/editar.cshtml", model);
                }
            }
            return View("~/Views/administrador/profesores/editar.cshtml", model);
        }
        public async Task<IActionResult> GenerarResetearContraseña(Guid id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString());

            if (usuario == null)
                return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

            var encodedToken = WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(token));

            var resetLink = Url.Action(
                "ResetearContraseña",
                "Cuenta",
                new
                {
                    userId = usuario.Id,
                    token = encodedToken
                },
                Request.Scheme);

            TempData["ResetLink"] = resetLink;

            return RedirectToAction("panel", "administrador");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString());
            if (usuario == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(usuario);
            if (result.Succeeded)
            {
                // Opcional: Eliminar cualquier otra información relacionada del estudiante
                await _context.SaveChangesAsync();
                return RedirectToAction("panel", "administrador");
            }
            else
            {
                // Manejar errores de eliminación
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // Puedes redirigir de nuevo al panel o mostrar un mensaje de error
                return RedirectToAction("panel", "administrador");
            }
        }
        private bool UsuarioIdentidadExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
        private async Task<List<Profesor>> ObtenerListaProfesores()
        {
            var profesoresUsuarios = await _userManager.GetUsersInRoleAsync("Profesor");
            return profesoresUsuarios.Select(u => new Profesor
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email
            }).ToList();
        }
    }
}
