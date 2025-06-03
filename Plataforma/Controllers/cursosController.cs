using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Inicio;
using Plataforma.Models.Profesores;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Threading;

namespace Plataforma.Controllers
{
    [Authorize(Roles = "Profesor")]
    public class cursosController : Controller
    {
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly PlataformaContext _context;
        public cursosController(UserManager<UsuarioIdentidad> userManager, PlataformaContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        [Route("profesor/cursos")]
        public async Task<IActionResult> Index()
        {
            var cursos = await _context.cursos.ToListAsync();
            var modulos = await _context.modulos.ToListAsync();
            var clases = await _context.clases.ToListAsync();

            Console.WriteLine($"Cursos: {_context.cursos.Count()}");
            Console.WriteLine($"Modulos: {_context.modulos.Count()}");
            Console.WriteLine($"Clases: {_context.clases.Count()}");

            ViewBag.Cursos = cursos;
            ViewBag.Modulos = modulos;
            ViewBag.Clases = clases;

            return View("~/Views/profesor/cursos/Index.cshtml");
        }
        [Route("profesor")]
        public async Task<IActionResult> Inicio()
        {
            var user = await _userManager.GetUserAsync(User);

            // Load courses associated with the current professor
            var profesorCourses = await _context.CursoProfesores
                                            .Where(cp => cp.ProfesorId == user.Id)
                                            .Include(cp => cp.Curso) // Eagerly load the Curso details
                                            .OrderBy(cp => cp.Curso.Nombre)
                                            .Select(cp => cp.Curso) // Select just the Curso objects
                                            .ToListAsync();

            return View("~/Views/profesor/Index.cshtml", profesorCourses);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCourseAvailability(Guid cursoId, bool isAvailable)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) // Check if user is logged in
            {
                return Json(new { success = false, message = "Usuario no autenticado." });
            }

            var curso = await _context.cursos.FindAsync(cursoId);

            if (curso == null)
            {
                return Json(new { success = false, message = "Curso no encontrado." }); // Return JSON for AJAX
            }

            var isProfesorAuthorized = await _context.CursoProfesores
                                                    .AnyAsync(cp => cp.ProfesorId == user.Id && cp.CursoId == cursoId);

            if (!isProfesorAuthorized)
            {
                return Json(new { success = false, message = "No autorizado para modificar este curso." }); // Return JSON for AJAX
            }

            curso.Disponible = isAvailable;
            await _context.SaveChangesAsync();

            // Still return JSON, don't redirect
            return Json(new { success = true, newStatus = curso.Disponible });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearModulo(Modulo modulo)
        {
            var curso = await _context.cursos.FindAsync(modulo.CursoId);
            if (curso != null)
            {
                modulo.Curso = curso;
                modulo.ModuloId = Guid.NewGuid();
            }

            if (ModelState.IsValid)
            {
                var maxExistingOrder = await _context.modulos
                                     .Where(m => m.CursoId == modulo.CursoId)
                                     .Select(m => (int?)m.Order) // Cast to int? to handle no existing modules
                                     .MaxAsync();

                // Assign the new order: maxOrder + 1, or 0 if no modules exist yet for this curso
                modulo.Order = (maxExistingOrder.HasValue ? maxExistingOrder.Value : -1) + 1;

                _context.modulos.Add(modulo);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "cursos");
            }
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model state is invalid. Errors:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"- {error.ErrorMessage}");
                }
                ViewBag.Cursos = _context.cursos.ToList();
                return View(modulo);
            }
            Console.WriteLine("not worked");
            // If there are errors, you might want to repopulate ViewBag.Cursos and return the view
            ViewBag.Cursos = _context.cursos.ToList();
            return View(modulo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearClase(Clase clase)
        {
            var modulo = await _context.modulos.FindAsync(clase.ModuloId);
            if (modulo != null)
            {
                clase.Modulo = modulo;
                clase.ClaseId = Guid.NewGuid();
            }
            if (ModelState.IsValid)
            {
                var maxExistingOrder = await _context.clases
                                             .Where(c => c.ModuloId == clase.ModuloId)
                                             .OrderByDescending(c => c.Order)
                                             .Select(c => (int?)c.Order)
                                             .MaxAsync();

                // Assign the new order:
                clase.Order = (maxExistingOrder.HasValue ? maxExistingOrder.Value : -1) + 1;

                _context.clases.Add(clase);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "cursos");
            }
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model state is invalid. Errors:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"- {error.ErrorMessage}");
                }
                ViewBag.Cursos = _context.cursos.ToList();
                return View(clase);
            }
            // If there are errors, you might want to repopulate ViewBag.Modulos and return the view
            ViewBag.Modulos = _context.modulos.ToList();
            return View(clase);
        }
    }
}
