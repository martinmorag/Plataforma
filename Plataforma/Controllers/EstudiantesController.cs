using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Administracion;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Plataforma.Controllers
{
    public class EstudiantesController : Controller
    {
        private readonly PlataformaContext _context;
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public EstudiantesController(PlataformaContext context, UserManager<UsuarioIdentidad> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdministracionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = new Estudiante
                {
                    Id = Guid.NewGuid(),
                    Nombre = model.RegistroEstudiante.Nombre,
                    Apellido = model.RegistroEstudiante.Apellido,
                    Email = model.RegistroEstudiante.Email,
                    UserName = model.RegistroEstudiante.Email
                };

                var result = await _userManager.CreateAsync(usuario);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Estudiante"))
                    {
                        var roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>("Estudiante"));
                        if (!roleResult.Succeeded)
                        {
                            ModelState.AddModelError(string.Empty, "Error al crear el rol de Estudiante.");
                            return View("~/Views/administrador/estudiantes/agregar.cshtml", model);
                        }
                    }
                    // Asigna el rol "Estudiante" al usuario creado
                    var roleAssignmentResult = await _userManager.AddToRoleAsync(usuario, "Estudiante");
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
                        TempData["SetupLink"] = setupLink;

                        await _context.SaveChangesAsync();
                        //var estudiantesLista = await ObtenerListaEstudiantes();
                        //var viewModel = new AdministracionViewModel { ListaEstudiantes = estudiantesLista };
                        return RedirectToAction("panel", "administrador");
                    }
                    else
                    {
                        foreach (var error in roleAssignmentResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View("~/Views/administrador/estudiantes/agregar.cshtml", model);
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("~/Views/administrador/estudiantes/agregar.cshtml", model);
                }
            }
            return View("~/Views/administrador/estudiantes/agregar.cshtml", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdministracionViewModel model)
        {
            if (model.EstudianteAEditar?.Id != model.EstudianteAEditar?.Id)
            {
                return NotFound();
            }

            Guid id = model.EstudianteAEditar.Id;

            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByIdAsync(id.ToString());
                if (usuario == null)
                {
                    return NotFound();
                }

                usuario.Nombre = model.EstudianteAEditar.Nombre;
                usuario.Apellido = model.EstudianteAEditar.Apellido;
                usuario.Email = model.EstudianteAEditar.Email;
                usuario.UserName = model.EstudianteAEditar.Email;

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
                    return View("~/Views/administrador/estudiantes/editar.cshtml", model);
                }
            }
            return View("~/Views/administrador/estudiantes/editar.cshtml", model);
        }
        public async Task<IActionResult> GenerarResetearContraseña(Guid id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString());

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "ingreso");
            }

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
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "ingreso");
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
        private async Task<List<Estudiante>> ObtenerListaEstudiantes()
        {
            var estudiantesUsuarios = await _userManager.GetUsersInRoleAsync("Estudiante");
            return estudiantesUsuarios.Select(u => new Estudiante
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email
            }).ToList();
        }
    }
}
