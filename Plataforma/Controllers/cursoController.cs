using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Cursos;

namespace Plataforma.Controllers
{
    public class cursoController : Controller
    {
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly PlataformaContext _context;

        public cursoController(UserManager<UsuarioIdentidad> userManager, PlataformaContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        public async Task<IActionResult> Index(Guid cursoId)
        {
            if (cursoId == Guid.Empty)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var estudianteId = user.Id; // Assuming EstudianteId is a Guid, parse from user.Id

            // Security check: Ensure student is enrolled
            var isEnrolled = await _context.CursoEstudiantes
                                            .AnyAsync(ce => ce.EstudianteId == estudianteId && ce.CursoId == cursoId);
            if (!isEnrolled)
            {
                return Forbid();
            }

            // Fetch course details with all necessary navigations for modules, classes, tasks, and files
            var curso = await _context.cursos
                                    .Include(c => c.CursoProfesores)
                                        .ThenInclude(cp => cp.Profesor)
                                    .Include(c => c.Modulos.OrderBy(m => m.Order))
                                        .ThenInclude(m => m.Clases.OrderBy(cl => cl.Order))
                                    .Include(c => c.Modulos)
                                        .ThenInclude(m => m.Clases)
                                            .ThenInclude(cl => cl.Tareas) // For HasTareas (each class has a collection of Tareas)
                                    .FirstOrDefaultAsync(c => c.CursoId == cursoId);

            if (curso == null)
            {
                return NotFound();
            }

            // --- New Completion Logic using Entrega model ---
            // Fetch all TareaId's that the current student has submitted with an "Aprobado" status
            var approvedSubmittedTareaIds = await _context.entregas // Assuming DbSet<Entrega> in PlataformaContext
                                                    .Where(e => e.EstudianteId == estudianteId && e.Estado == Entrega.EstadoEntrega.Aprobado)
                                                    .Select(e => e.TareaId)
                                                    .ToListAsync();
            // --- End New Completion Logic ---

            string? instructorName = curso.CursoProfesores?.FirstOrDefault()?.Profesor?.Nombre;

            var viewModel = new CursoClasesViewModel
            {
                CursoId = curso.CursoId,
                CursoNombre = curso.Nombre,
                InstructorNombre = instructorName,
                TotalClases = curso.Modulos.SelectMany(m => m.Clases).Count(),
                CompletedClases = 0, // Initialize to 0, will be updated later
                Modulos = curso.Modulos.Select(m => new ModuloViewModel
                {
                    ModuloId = m.ModuloId,
                    Nombre = m.Titulo,
                    Order = m.Order,
                    IsCompleted = false, // Will be set after populating classes

                    Clases = m.Clases.Select(c =>
                    {
                        bool isClaseCompleted;
                        if (!c.Tareas.Any()) // If the class has no tasks, consider it completed by default
                        {
                            isClaseCompleted = true;
                        }
                        else
                        {
                            // A class is completed if ALL its tasks have an approved Entrega by the student
                            isClaseCompleted = c.Tareas.All(tarea => approvedSubmittedTareaIds.Contains(tarea.TareaId));
                        }

                        return new ClaseViewModel
                        {
                            ClaseId = c.ClaseId,
                            Nombre = c.Nombre,
                            Order = c.Order,
                            HasTareas = c.Tareas != null && c.Tareas.Any(),
                            IsCompleted = isClaseCompleted
                        };
                    }).ToList()
                }).ToList()
            };

            // Post-processing to calculate module completion and total completed classes for the course
            foreach (var moduloVm in viewModel.Modulos)
            {
                // A module is completed if it has classes AND ALL of its classes are completed
                moduloVm.IsCompleted = moduloVm.Clases.Any() && moduloVm.Clases.All(claseVm => claseVm.IsCompleted);
            }

            // Finally, calculate the total completed classes for the course
            viewModel.CompletedClases = viewModel.Modulos.SelectMany(m => m.Clases).Count(claseVm => claseVm.IsCompleted);

            return View(viewModel);
        }
    }
}
