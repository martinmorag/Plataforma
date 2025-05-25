using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Cursos;

namespace Plataforma.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClaseController : Controller
    {
        private readonly PlataformaContext _context;
        private readonly UserManager<UsuarioIdentidad> _userManager;

        public ClaseController(PlataformaContext context, UserManager<UsuarioIdentidad> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("GetClassDetails")]
        public async Task<IActionResult> GetClassDetails(Guid claseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var estudianteId = user.Id; // Assuming EstudianteId is Guid

            var clase = await _context.clases
                                    .Include(c => c.Tareas) // Include all tasks for this class
                                    .FirstOrDefaultAsync(c => c.ClaseId == claseId);

            if (clase == null)
            {
                return NotFound();
            }

            // Fetch all TareaId's that the current student has submitted with an "Aprobado" status
            var approvedSubmittedTareaIds = await _context.entregas
                                                    .Where(e => e.EstudianteId == estudianteId && e.Estado == Entrega.EstadoEntrega.Aprobado)
                                                    .Select(e => e.TareaId)
                                                    .ToListAsync();

            // Fetch all current student's submissions for this class's tasks
            var studentSubmissions = await _context.entregas
                                                .Where(e => e.EstudianteId == estudianteId && e.Tarea.ClaseId == claseId)
                                                .ToDictionaryAsync(e => e.TareaId, e => e); // Dictionary for quick lookup

            var claseViewModel = new ClaseViewModel
            {
                ClaseId = clase.ClaseId,
                Nombre = clase.Nombre,
                Order = clase.Order,
                HasTareas = clase.Tareas != null && clase.Tareas.Any(),
                IsCompleted = false, // Will be set after populating tasks

                Tareas = clase.Tareas.OrderBy(t => t.Nombre).Select(t =>
                {
                    bool isSubmittedApproved = approvedSubmittedTareaIds.Contains(t.TareaId);
                    string submissionStatusText = "Pendiente";
                    if (studentSubmissions.TryGetValue(t.TareaId, out var submission))
                    {
                        submissionStatusText = submission.Estado.ToString(); // "EnRevision", "Aprobado", "Reprobado"
                    }

                    return new TareaViewModel
                    {
                        TareaId = t.TareaId,
                        Nombre = t.Nombre,
                        Descripcion = t.Descripcion,
                        FechaLimite = t.FechaVencimiento,
                        IsSubmittedApproved = isSubmittedApproved,
                        SubmissionStatusText = submissionStatusText,
                        AccederLink = Url.Action("Details", "Tarea", new { id = t.TareaId }) // Link to the Tarea details page
                    };
                }).ToList()
            };

            // Determine if the class itself is completed based on its tasks
            if (!claseViewModel.Tareas.Any())
            {
                claseViewModel.IsCompleted = true; // No tasks, so considered complete
            }
            else
            {
                claseViewModel.IsCompleted = claseViewModel.Tareas.All(t => t.IsSubmittedApproved);
            }

            return PartialView("_ClassDetailsPartial", claseViewModel);
        }
    }
}
