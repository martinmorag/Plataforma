using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Inicio;

namespace Plataforma.Controllers
{
    public class ApiCursosController : Controller
    {
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly PlataformaContext _context;
        public ApiCursosController(UserManager<UsuarioIdentidad> userManager, PlataformaContext context)
        {
            _userManager = userManager;
            _context = context;
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
