using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Administracion;

namespace Plataforma.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class administradorController : Controller
    {
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly PlataformaContext _context;
        public administradorController(UserManager<UsuarioIdentidad> userManager, PlataformaContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> panel()
        {
            // Get students
            var estudiantesUsuarios = await _userManager.GetUsersInRoleAsync("Estudiante");
            var estudiantesLista = estudiantesUsuarios.Select(u => new Estudiante
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email
            }).ToList();

            // Get teachers
            var profesoresUsuarios = await _userManager.GetUsersInRoleAsync("Profesor");
            var profesoresLista = profesoresUsuarios.Select(u => new Profesor
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email
            }).ToList();

            // Crear una instancia del ViewModel combinado
            var viewModel = new AdministracionViewModel
            {
                ListaEstudiantes = estudiantesLista,
                ListaProfesores = profesoresLista,
            };

            // Pasar el ViewModel combinado a la vista
            return View("panel/Index", viewModel);
        }
        // Gestionar profesores y cursos
        [HttpGet("administrador/profesores/cursos")]
        public async Task<IActionResult> GestionarInscripciones(Guid profesorId)
        {
            // 1. Verify the professor exists
            var profesor = await _context.profesores.FindAsync(profesorId);
            if (profesor == null)
            {
                return NotFound($"Profesor con ID {profesorId} no encontrado.");
            }

            // 2. Get courses the specific professor is already assigned to
            var cursosActualesDelProfesor = await _context.CursoProfesores
                                                        .Where(pc => pc.ProfesorId == profesorId)
                                                        .Include(pc => pc.Curso) // Eager load Curso details
                                                        .ToListAsync();

            // Get a list of CourseIds that the professor is currently assigned to
            var cursosAsignadosIds = cursosActualesDelProfesor.Select(pc => pc.CursoId).ToList();

            // 3. Get all courses that the professor is NOT currently assigned to
            var cursosDisponiblesParaAsignar = await _context.cursos
                                                           .Where(c => !cursosAsignadosIds.Contains(c.CursoId))
                                                           .ToListAsync();

            var viewModel = new SeleccionCursosProfesor
            {
                ProfesorId = profesorId,
                Cursos = cursosDisponiblesParaAsignar, // This now only contains non-assigned courses
                CursoProfesor = cursosActualesDelProfesor // This contains currently assigned courses
            };

            ViewData["ProfesorName"] = profesor.Nombre + " " + profesor.Apellido;

            return View("~/Views/administrador/profesores/cursos.cshtml", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarAsignacionesProfesor(SeleccionCursosProfesor model)
        {
            Guid profesorId = model.ProfesorId;

            // Optional: Re-verify professor exists
            var profesor = await _context.profesores.FindAsync(profesorId);
            if (profesor == null)
            {
                return NotFound($"Profesor con ID {profesorId} no encontrado.");
            }

            // 1. Handle De-assignment (remove courses the professor is no longer assigned to)
            foreach (var cursoIdToDeassign in model.CursosSeleccionadosParaDesinscripcion)
            {
                var existingAssignment = await _context.CursoProfesores // Assuming _context.ProfesorCursos is your DbSet for ProfesorCursoDto
                                                       .FirstOrDefaultAsync(pc => pc.ProfesorId == profesorId && pc.CursoId == cursoIdToDeassign);
                if (existingAssignment != null)
                {
                    _context.CursoProfesores.Remove(existingAssignment);
                }
            }

            // 2. Handle Assignment (add courses the professor is now assigned to teach)
            foreach (var cursoIdToAssign in model.CursosSeleccionadosParaInscripcion)
            {
                // Check if professor is already assigned to this course to prevent duplicates
                var alreadyAssigned = await _context.CursoProfesores
                                                    .AnyAsync(pc => pc.ProfesorId == profesorId && pc.CursoId == cursoIdToAssign);
                if (!alreadyAssigned)
                {
                    var newAssignment = new CursoProfesor // This DTO or entity should represent the Professor-Course relationship
                    {
                        ProfesorId = profesorId,
                        CursoId = cursoIdToAssign
                    };
                    _context.CursoProfesores.Add(newAssignment);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Asignaciones para {profesor.Nombre} {profesor.Apellido} actualizadas exitosamente.";

            // Redirect back to the admin panel or a professor details page
            return RedirectToAction("panel", "administrador");
        }
        // ESTUDIANTES
        [Route("administrador/estudiantes/agregar")]
        public IActionResult agregar_estudiante()
        {           
            var viewModel = new AdministracionViewModel
            {
                RegistroEstudiante = new RegistroEstudianteViewModel(), // Si también tienes el formulario aquí, inicialízalo
            };

            return View("~/Views/administrador/estudiantes/agregar.cshtml", viewModel);
        }

        public async Task<IActionResult> editar_estudiantes(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _userManager.FindByIdAsync(id.ToString());
            if (usuario == null)
            {
                return NotFound();
            }
            var estudianteAEditar = new Estudiante
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email
            };

            var viewModel = new AdministracionViewModel
            {
                EstudianteAEditar = estudianteAEditar
                // No necesitas RegistroEstudiante aquí para la edición
            };

            return View("~/Views/administrador/estudiantes/editar.cshtml", viewModel);
        }

        //PROFESORES
        [Route("administrador/profesores/agregar")]
        public IActionResult agregar_profesor()
        {
            var viewModel = new AdministracionViewModel
            {
                RegistroProfesor = new RegistroProfesorViewModel(), // Si también tienes el formulario aquí, inicialízalo
            };

            return View("~/Views/administrador/profesores/agregar.cshtml", viewModel);
        }
        [Route("administrador/profesores/editar")]
        public async Task<IActionResult> editar_profesores(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _userManager.FindByIdAsync(id.ToString());
            if (usuario == null)
            {
                return NotFound();
            }
            var profesoraEditar = new Profesor
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email
            };

            var viewModel = new AdministracionViewModel
            {
                ProfesorAEditar = profesoraEditar
                // No necesitas RegistroEstudiante aquí para la edición
            };

            return View("~/Views/administrador/profesores/editar.cshtml", viewModel);
        }
    }
}
