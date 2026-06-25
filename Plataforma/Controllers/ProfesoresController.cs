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
        private readonly CloudFrontService _cloudFrontService;

        public ProfesoresController(PlataformaContext context, UserManager<UsuarioIdentidad> userManager, RoleManager<IdentityRole<Guid>> roleManager, CloudFrontService cloudFrontService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _cloudFrontService = cloudFrontService;
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
        [Route("profesor/tareas")]
        public async Task<IActionResult> Tareas()
        {
            var user = await _userManager.GetUserAsync(User);

            var cursos = await _context.cursos
                .Where(c => c.CursoProfesores.Any(cp => cp.ProfesorId == user.Id))
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Cursos = cursos;

            return View("~/Views/profesor/tareas/tareas.cshtml");
        }
        [HttpGet]
        [Route("profesor/tareas/GetByCurso")]
        public async Task<IActionResult> GetByCurso(Guid cursoId)
        {
            var user = await _userManager.GetUserAsync(User);

            var tareas = await _context.tareas
                .Where(t =>
                    t.Clase.Modulo.CursoId == cursoId &&
                    t.Clase.Modulo.Curso.CursoProfesores
                        .Any(cp => cp.ProfesorId == user.Id))
                .Select(t => new
                {
                    t.TareaId,
                    t.Nombre,
                    t.TipoEntregaEsperado,
                    Clase = t.Clase.Nombre,
                    Fecha = t.FechaVencimiento
                })
                .OrderBy(t => t.Fecha)
                .ToListAsync();

            return Ok(tareas);
        }
        [HttpGet]
        [Route("profesor/tareas/GetDetalle")]
        public async Task<IActionResult> GetDetalle(Guid tareaId)
        {
            var user = await _userManager.GetUserAsync(User);

            var tarea = await _context.tareas
                .Include(t => t.Archivo)
                .Include(t => t.Clase)
                    .ThenInclude(c => c.Modulo)
                .FirstOrDefaultAsync(t => t.TareaId == tareaId);

            if (tarea == null)
                return NotFound();

            var autorizado = await _context.CursoProfesores.AnyAsync(cp =>
                cp.CursoId == tarea.Clase.Modulo.CursoId &&
                cp.ProfesorId == user.Id);

            if (!autorizado)
                return Forbid();

            return Ok(new
            {
                tarea.Nombre,
                tarea.Descripcion,
                Tipo = tarea.TipoEntregaEsperado,
                ArchivoUrl = tarea.Archivo != null
                    ? _cloudFrontService.GenerateSignedUrl(tarea.Archivo.ArchivoUrl)
                    : null,
                tarea.ReunionUrl,
                Clase = tarea.Clase.Nombre,
                Fecha = tarea.FechaVencimiento
            });
        }
    }
}
