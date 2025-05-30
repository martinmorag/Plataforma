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
        [HttpPost]
        public async Task<IActionResult> ManageCourses([FromBody] CourseChangesModel changes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Return a 401 Unauthorized status if the user is not logged in
                return Unauthorized(new { message = "Usuario no autenticado." });
            }

            if (changes == null)
            {
                return BadRequest(new { message = "Datos de cambios no proporcionados en el cuerpo de la solicitud." });
            }

            var errors = new List<string>();

            // --- Process Deregistrations First (often safer to remove before adding) ---
            if (changes.CoursesToDeregister != null && changes.CoursesToDeregister.Any())
            {
                foreach (var courseIdString in changes.CoursesToDeregister)
                {
                    if (!Guid.TryParse(courseIdString, out Guid courseGuid))
                    {
                        errors.Add($"ID de curso inválido para anular registro: '{courseIdString}'.");
                        continue;
                    }

                    var cursoEstudiante = await _context.CursoEstudiantes
                                                        .FirstOrDefaultAsync(ce => ce.EstudianteId == user.Id && ce.CursoEstudianteId == courseGuid);
                    // FIX: Changed from CursoEstudianteId to CursoId

                    if (cursoEstudiante != null)
                    {
                        _context.CursoEstudiantes.Remove(cursoEstudiante);
                    }
                    else
                    {
                        errors.Add($"No se encontró registro para el curso '{courseIdString}' para anular.");
                    }
                }
            }

            if (changes.CoursesToRegister != null && changes.CoursesToRegister.Any())
            {
                foreach (var courseIdString in changes.CoursesToRegister)
                {
                    if (!Guid.TryParse(courseIdString, out Guid courseGuid))
                    {
                        errors.Add($"ID de curso inválido para registrar: '{courseIdString}'.");
                        continue;
                    }

                    // Check if already registered to prevent duplicates
                    var alreadyRegistered = await _context.CursoEstudiantes
                                                        .AnyAsync(ce => ce.EstudianteId == user.Id && ce.CursoId == courseGuid);

                    if (!alreadyRegistered)
                    {
                        // Assuming CursoEstudiante needs a unique ID too, if it's not handled by EF automatically
                        // If CursoEstudianteId is a Guid primary key for the join table itself
                        // you might need to generate it. If it's identity, EF handles it.
                        _context.CursoEstudiantes.Add(new Models.ProfesorCursoDto
                        {
                            CursoEstudianteId = Guid.NewGuid(), // Only if this is the PK and needs explicit GUID
                            EstudianteId = user.Id,
                            CursoId = courseGuid
                            // Any other properties like FechaInscripcion = DateTime.UtcNow etc.
                        });
                    }
                    else
                    {
                        errors.Add($"Ya estás registrado en el curso '{courseIdString}'.");
                    }
                }
            }

            try
            {
                // IMPORTANT: Call SaveChangesAsync() ONCE after all desired additions/removals
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Cambios guardados con éxito.", errors = errors });
            }
            catch (DbUpdateException ex)
            {
                // Log the exception, e.g., using ILogger
                Console.WriteLine($"Database update error: {ex.Message}");
                // Consider more specific error handling for concurrency or constraint violations
                return StatusCode(500, new { message = "Error al guardar cambios en la base de datos.", errorDetails = ex.Message, internalErrors = errors });
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return StatusCode(500, new { message = "Ocurrió un error inesperado al guardar los cambios.", errorDetails = ex.Message, internalErrors = errors });
            }
        }
    }
}
