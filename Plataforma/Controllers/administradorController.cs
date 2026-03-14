using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Administracion;
using Plataforma.Servicios;

namespace Plataforma.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class administradorController : Controller
    {
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly PlataformaContext _context;
        private readonly S3Service _s3Service;
        private readonly CloudFrontService _cloudFrontService;
        public administradorController(UserManager<UsuarioIdentidad> userManager, PlataformaContext context, S3Service s3Service, CloudFrontService cloudFrontService)
        {
            _userManager = userManager;
            _context = context;
            _s3Service = s3Service;
            _cloudFrontService = cloudFrontService;
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
        [Route("administrador/cursos")]
        public async Task<IActionResult> Index()
        {
            var cursos = await _context.cursos
                .Include(c => c.CursoEstudiantes)
                .Include(c => c.CursoProfesores)
                    .ThenInclude(cp => cp.Profesor)
                .Include(c => c.Modulos)
                    .ThenInclude(m => m.Clases)
                .ToListAsync();

            var model = cursos.Select(c => new CursoAdminIndexViewModel
            {
                CursoId = c.CursoId,
                Nombre = c.Nombre,
                Habilitado = c.Habilitado,
                ImageUrl = c.ImageUrl,
                CantidadEstudiantes = c.CursoEstudiantes.Count,

                Profesores = c.CursoProfesores
                    .Select(cp => $"{cp.Profesor.Nombre} {cp.Profesor.Apellido}")
                    .ToList(),

                Modulos = c.Modulos.Select(m => new ModuloAdminViewModel
                {
                    Nombre = m.Nombre,
                    Clases = m.Clases
                        .Select(cl => cl.Nombre)
                        .ToList()
                }).ToList()

            }).ToList();

            return View("~/Views/administrador/cursos/Index.cshtml", model);
        }
        // Crear un curso
        [HttpGet]
        [Route("administrador/cursos/crear")]
        public IActionResult Crear_Curso()
        {
            return View("~/Views/administrador/cursos/crear.cshtml");
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
        [HttpGet]
        [Route("administrador/cursos/editar/{id}")]
        public async Task<IActionResult> Editar_Curso(Guid id)
        {
            var curso = await _context.cursos
                .FirstOrDefaultAsync(c => c.CursoId == id);

            curso.ImageUrl = _cloudFrontService.GenerateSignedUrl(curso.ImageUrl);

            if (curso == null)
                return NotFound();

            var model = new CrearCursoViewModel
            {
                Nombre = curso.Nombre,
                Disponible = curso.Habilitado
            };

            ViewBag.CursoId = curso.CursoId;
            ViewBag.ImageUrl = curso.ImageUrl;

            return View("~/Views/administrador/cursos/editar.cshtml", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("administrador/cursos/toggle/{id}")]
        public async Task<IActionResult> ToggleHabilitado(Guid id)
        {
            var curso = await _context.cursos.FindAsync(id);

            if (curso == null)
                return NotFound();

            curso.Habilitado = !curso.Habilitado;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                habilitado = curso.Habilitado
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("administrador/cursos/crear")]
        public async Task<IActionResult> CrearCurso(
            CrearCursoViewModel model,
            IFormFile? imagenCurso)
        {
            if (!ModelState.IsValid)
                return View("~/Views/administrador/cursos/crear.cshtml", model);

            bool exists = await _context.cursos
                .AnyAsync(c => c.Nombre == model.Nombre);

            if (exists)
            {
                ModelState.AddModelError("Nombre", "Ya existe un curso con ese nombre.");
                return View("~/Views/administrador/cursos/crear.cshtml", model);
            }

            string? imageKey = null;

            var cursoId = Guid.NewGuid();

            if (imagenCurso != null && imagenCurso.Length > 0)
            {
                var extension = Path.GetExtension(imagenCurso.FileName);
                var fileName = $"curso-{cursoId}-{Guid.NewGuid()}{extension}";

                imageKey = await _s3Service.UploadFileAsync(
                    imagenCurso,
                    "cursos",
                    fileName
                );
            }
            else
            {
                var defaultImages = new List<string>
                {
                    "public/cursos/default-1.jpg",
                    "public/cursos/default-2.jpg",
                    "public/cursos/default-3.jpg",
                    "public/cursos/default-4.jpg",
                    "public/cursos/default-5.jpg"
                };

                var lastDefaultCurso = await _context.cursos
                    .Where(c => c.ImageUrl != null &&
                                c.ImageUrl.StartsWith("public/cursos/default-"))
                    .OrderByDescending(c => c.CursoId)
                    .FirstOrDefaultAsync();

                int nextIndex = 0;

                if (lastDefaultCurso != null)
                {
                    int lastIndex = defaultImages
                        .FindIndex(x => x == lastDefaultCurso.ImageUrl);

                    if (lastIndex >= 0)
                    {
                        nextIndex = (lastIndex + 1) % defaultImages.Count;
                    }
                }

                imageKey = defaultImages[nextIndex];
            }

            var curso = new Curso
            {
                CursoId = cursoId,
                Nombre = model.Nombre,
                Habilitado = model.Disponible,
                ImageUrl = imageKey
            };

            _context.cursos.Add(curso);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("administrador/cursos/editar/{id}")]
        public async Task<IActionResult> EditarCurso(
            Guid id,
            CrearCursoViewModel model,
            IFormFile? imagenCurso)
        {
            if (!ModelState.IsValid)
                return View("~/Views/administrador/cursos/editar.cshtml", model);

            var curso = await _context.cursos
                .FirstOrDefaultAsync(c => c.CursoId == id);

            if (curso == null)
                return NotFound();

            bool exists = await _context.cursos
                .AnyAsync(c => c.Nombre == model.Nombre && c.CursoId != id);

            if (exists)
            {
                ModelState.AddModelError("Nombre", "Ya existe un curso con ese nombre.");
                return View("~/Views/administrador/cursos/editar.cshtml", model);
            }

            if (imagenCurso != null && imagenCurso.Length > 0)
            {
                if (!string.IsNullOrEmpty(curso.ImageUrl) &&
                    !curso.ImageUrl.StartsWith("public/cursos/default-"))
                {
                    await _s3Service.DeleteFileAsync(curso.ImageUrl);
                }

                var extension = Path.GetExtension(imagenCurso.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";

                curso.ImageUrl = await _s3Service.UploadFileAsync(imagenCurso, "cursos", fileName);
            }

            curso.Nombre = model.Nombre;
            curso.Habilitado = model.Disponible;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("administrador/cursos/eliminar/{id}")]
        public async Task<IActionResult> EliminarCurso(Guid id)
        {
            var curso = await _context.cursos
                .FirstOrDefaultAsync(c => c.CursoId == id);

            if (curso == null)
                return NotFound();

            // Delete image if NOT default
            if (!string.IsNullOrEmpty(curso.ImageUrl) &&
                !curso.ImageUrl.StartsWith("public/cursos/default-"))
            {
                await _s3Service.DeleteFileAsync(curso.ImageUrl);
            }

            _context.cursos.Remove(curso);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
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
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "ingreso");
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
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "ingreso");
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
