using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Profesores;
using Plataforma.Servicios;
using System.Data;
using System.Net.Http;
using Xabe.FFmpeg;

namespace Plataforma.Controllers
{
    [Authorize]
    public class tareasController : Controller
    {
        private readonly PlataformaContext _context;
        private readonly HttpClient _httpClient; 
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly SignInManager<UsuarioIdentidad> _signInManager;
        private readonly S3Service _s3Service;

        public tareasController(PlataformaContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration, IWebHostEnvironment environment, UserManager<UsuarioIdentidad> userManager, SignInManager<UsuarioIdentidad> signInManager, S3Service s3service) 
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration; 
            _environment = environment;
            _userManager = userManager;
            _signInManager = signInManager;
            _s3Service = s3service;
        }
        [HttpGet("profesor/tareas/crear")]
        [Authorize(Roles = "Profesor")]
        public IActionResult crear(Guid claseId, string contentType)
        {
            var clase = _context.clases.FirstOrDefault(c => c.ClaseId == claseId);

            if (clase == null)
                return RedirectToAction("Index", "ingreso");
            

            var tarea = new Tarea
            {
                Clase = clase,
                TipoEntregaEsperado = contentType,
                ClaseId = claseId
            };

            return View("~/Views/profesor/tareas/crear.cshtml", tarea);
        }
        [HttpGet]
        [Authorize(Roles = "Profesor")]
        [Route("profesor/tareas/editar")]
        public async Task<IActionResult> editar(Guid tareaId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "ingreso");
            }

            var tarea = await _context.tareas
                .Include(t => t.Archivo)
                .Include(t => t.Clase)
                    .ThenInclude(c => c.Modulo)
                .FirstOrDefaultAsync(t =>
                    t.TareaId == tareaId &&
                    t.Clase.Modulo.Curso.CursoProfesores
                     .Any(cp => cp.ProfesorId == user.Id));

            if (tarea == null)
                return NotFound();

            var vm = new EditarTareaViewModel
            {
                TareaId = tarea.TareaId,
                ClaseId = tarea.ClaseId,
                CursoId = tarea.Clase.Modulo.CursoId,
                ClaseNombre = tarea.Clase.Nombre,
                Nombre = tarea.Nombre,
                Descripcion = tarea.Descripcion,
                FechaVencimiento = tarea.FechaVencimiento,
                TipoEntregaEsperado = tarea.TipoEntregaEsperado,
                ArchivoNombre = tarea.Archivo?.FileName
            };

            return View("~/Views/profesor/tareas/editar.cshtml", vm);
        }
        [HttpGet]
        [Authorize(Roles = "Profesor")]
        [Route("profesor/tareas/ver")] // Example route
        public async Task<IActionResult> MisCursosYTareas(Guid? cursoId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "ingreso");
            }

            Guid profesorId = user.Id;

            // Get courses assigned to the current professor
            var cursos = await _context.CursoProfesores
                                       .Where(cp => cp.ProfesorId == profesorId)
                                       .Select(cp => new Models.Profesores.ProfesorCursoDto
                                       {
                                           CursoId = cp.Curso.CursoId,
                                           NombreCurso = cp.Curso.Nombre,
                                           TotalClases = cp.Curso.Modulos.SelectMany(m => m.Clases).Count(), // Consider if this is needed in the UI
                                           TotalTareas = cp.Curso.Modulos.SelectMany(m => m.Clases).SelectMany(cl => cl.Tareas).Count()
                                       })
                                       .ToListAsync();

            // Pass courses to the view
            ViewBag.Cursos = cursos;

            // --- Start of Task Loading Logic ---
            List<ProfesorTareaViewModel> tareas = new List<ProfesorTareaViewModel>();
            string selectedCursoNombre = null; // Default: no course selected, tasks hidden

            // If a specific course ID is provided in the URL, try to pre-select it and load its tasks
            if (cursoId.HasValue)
            {
                var selectedCourse = cursos.FirstOrDefault(c => c.CursoId == cursoId.Value);
                if (selectedCourse != null)
                {
                    selectedCursoNombre = selectedCourse.NombreCurso;

                    var tareasQuery = _context.tareas
                        .Where(t => t.Clase.Modulo.CursoId == cursoId.Value &&
                                    t.Clase.Modulo.Curso.CursoProfesores.Any(cp => cp.ProfesorId == user.Id)); // Corrected navigation
                                                                                                               // ... rest of the select and ToListAsync()

                    tareas = await _context.tareas
                        .Where(t => t.Clase.Modulo.CursoId == cursoId &&
                                    t.Clase.Modulo.Curso.CursoProfesores.Any(cp => cp.ProfesorId == user.Id)) // Corrected navigation
                        .Select(t => new ProfesorTareaViewModel
                        {
                            TareaId = t.TareaId,
                            Nombre = t.Nombre,
                            ClaseNombre = t.Clase.Nombre,
                            FechaLimite = t.FechaVencimiento,
                            TotalEntregas = _context.entregas.Count(e => e.TareaId == t.TareaId),
                            EntregasPendientes = _context.entregas.Count(e => e.TareaId == t.TareaId &&
                                                                               (e.Estado == Entrega.EstadoEntrega.EnRevision ||
                                                                                e.Estado == Entrega.EstadoEntrega.EnProgreso))
                        })
                        .ToListAsync();
                }
                // If cursoId was provided but not found for this professor, selectedCursoNombre remains null,
                // and tareas remains empty, leading to the "select a course" message.
            }
            // --- End of Task Loading Logic ---

            ViewBag.Tareas = tareas; // This will be empty if no cursoId was provided or found
            ViewBag.SelectedCursoNombre = selectedCursoNombre; // This will be null if no course is initially selected

            return View("~/Views/profesor/tareas/ver.cshtml");
        }
        [HttpGet]
        [Authorize(Roles = "Profesor")]
        [Route("profesor/tareas/GetTareasByCurso")] 
        public async Task<IActionResult> GetTareasByCurso(Guid cursoId) 
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "ingreso");
            }

            // Ensure the course belongs to the current professor through the CursoProfesores join table
            var courseBelongsToProfessor = await _context.CursoProfesores
                .AnyAsync(cp => cp.CursoId == cursoId && cp.ProfesorId == user.Id);

            if (!courseBelongsToProfessor)
            {
                return Forbid(); // Or NotFound(), depending on desired behavior if professor doesn't own course
            }

            var tareas = await _context.tareas
                .Where(t => t.Clase.Modulo.CursoId == cursoId &&
                            t.Clase.Modulo.Curso.CursoProfesores.Any(cp => cp.ProfesorId == user.Id)) // Corrected navigation
                .Select(t => new ProfesorTareaViewModel
                {
                    TareaId = t.TareaId,
                    Nombre = t.Nombre,
                    ClaseNombre = t.Clase.Nombre,
                    FechaLimite = t.FechaVencimiento,
                    TipoContenido = t.TipoEntregaEsperado,
                    TotalEntregas = _context.entregas.Count(e => e.TareaId == t.TareaId),
                    EntregasPendientes = _context.entregas.Count(e => e.TareaId == t.TareaId &&
                                                                       (e.Estado == Entrega.EstadoEntrega.EnRevision ||
                                                                        e.Estado == Entrega.EstadoEntrega.EnProgreso))
                })
                .ToListAsync();

            return Ok(tareas); // This is the key: return JSON data
        }
        [HttpGet]
        [Route("profesor/tareas/entregas")]
        public async Task<IActionResult> VerEntregas(Guid tareaId)
        {
            if (!User.IsInRole("Profesor"))
            {
                await _signInManager.SignOutAsync();
                var returnUrl = Url.Action("VerEntregas", "tareas", new { tareaId });
                return RedirectToAction("Index", "ingreso", new { ReturnUrl = returnUrl });
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                var returnUrl = Url.Action("VerEntregas", "tareas", new { tareaId });
                return RedirectToAction("Index", "ingreso", new { ReturnUrl = returnUrl });
            }

            Guid profesorId = user.Id;

            var tarea = await _context.tareas
                                      .Include(t => t.Clase)
                                          .ThenInclude(cl => cl.Modulo)
                                              .ThenInclude(m => m.Curso)
                                                  .ThenInclude(c => c.CursoProfesores) // To check professor assignment
                                      .FirstOrDefaultAsync(t => t.TareaId == tareaId);

            if (tarea == null)
            {
                return NotFound("Tarea no encontrada.");
            }

            // Verify the professor has access to this task's course
            var isProfesorAssignedToCurso = tarea.Clase.Modulo.Curso.CursoProfesores.Any(cp => cp.ProfesorId == profesorId);
            if (!isProfesorAssignedToCurso)
            {
                return Forbid("No tienes permiso para ver esta tarea.");
            }

            var entregas = await _context.entregas
                                         .Include(e => e.Estudiante)
                                         .Include(e => e.Archivo)
                                         .Where(e => e.TareaId == tareaId)
                                         .Select(e => new ProfesorEntregaViewModel
                                         {
                                             EntregaId = e.EntregaId,
                                             EstudianteNombre = e.Estudiante.Nombre + " " + e.Estudiante.Apellido,
                                             Estado = e.Estado,
                                             FechaEntrega = e.FechaEntrega,
                                             ComentariosProfesor = e.ComentariosProfesor,
                                             // For MVC, the URL needs to point to your *API* endpoint
                                             ArchivoUrl = e.Archivo != null ? $"/api/Profesores/AccessSubmittedFile/{e.EntregaId}" : null,
                                             ArchivoNombreOriginal = e.Archivo != null ? e.Archivo.FileName : null,
                                         })
                                         .ToListAsync();

            ViewBag.TareaNombre = tarea.Nombre;
            ViewBag.TareaId = tareaId; // Pass TaskId to the view for evaluation modal
            return View("~/Views/profesor/tareas/entregas.cshtml", entregas);
        }
        [HttpPost]
        [Authorize(Roles = "Profesor")]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create(Tarea asignacionModel, IFormFile? archivoAsignacion) 
        {
            if (!ModelState.IsValid || asignacionModel.ClaseId == Guid.Empty)
            {
                asignacionModel.Clase = await _context.clases.FindAsync(asignacionModel.ClaseId);
                return View("~/Views/profesor/tareas/crear.cshtml", asignacionModel);
            }

            Archivo? archivoGuardado = null;

            if (archivoAsignacion != null && archivoAsignacion.Length > 0)
            {
                var extension = Path.GetExtension(archivoAsignacion.FileName).ToLowerInvariant();

                var allowedExtensions = new[]
                            {
                                ".pdf",
                                ".doc",
                                ".docx",
                                ".xls",
                                ".xlsx",
                                ".gif",
                                ".mp4",
                                ".mov",
                                ".avi"
                            };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("archivoAsignacion", "Tipo de archivo no permitido.");
                    asignacionModel.Clase = await _context.clases.FindAsync(asignacionModel.ClaseId);
                    return View("crear", asignacionModel);
                }

                // Optional: also validate MIME type
                var contentType = archivoAsignacion.ContentType;

                var allowedMimeTypes = new[]
                {
                    "application/pdf",
                    "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "application/vnd.ms-excel",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };

                var isVideo = contentType.StartsWith("video/");

                if (!allowedMimeTypes.Contains(contentType) && !isVideo)
                {
                    ModelState.AddModelError("archivoAsignacion", "Tipo de archivo inválido.");
                    asignacionModel.Clase = await _context.clases.FindAsync(asignacionModel.ClaseId);
                    return View("crear", asignacionModel);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";

                string s3Key = await _s3Service.UploadFileAsync(
                                    archivoAsignacion,
                                    "asignaciones",
                                    fileName
                                );

                archivoGuardado = new Archivo
                {
                    ArchivoId = Guid.NewGuid(),
                    ArchivoUrl = s3Key,
                    FileName = fileName,
                    ContentType = archivoAsignacion.ContentType,
                    SizeInBytes = archivoAsignacion.Length,
                    FechaSubida = DateTime.UtcNow
                };

                _context.archivos.Add(archivoGuardado);
                await _context.SaveChangesAsync(); // Save Archivo first to get its ID
            }

            var nuevaAsignacion = new Tarea
            {
                TareaId = Guid.NewGuid(),
                Nombre = asignacionModel.Nombre,
                Descripcion = asignacionModel.Descripcion,
                FechaVencimiento = asignacionModel.FechaVencimiento.ToUniversalTime(), // Ensure UTC
                ClaseId = asignacionModel.ClaseId,
                TipoEntregaEsperado = asignacionModel.TipoEntregaEsperado // Set the expected submission type
            };

            if (archivoGuardado != null)
            {
                nuevaAsignacion.ArchivoId = archivoGuardado.ArchivoId;
            }

            _context.tareas.Add(nuevaAsignacion);
            await _context.SaveChangesAsync();

            return RedirectToAction("Inicio", "cursos");
        }
        [HttpPost]
        [Authorize(Roles = "Profesor")]
        [ValidateAntiForgeryToken]
        [Route("profesor/tareas/editar")]
        public async Task<IActionResult> Editar(EditarTareaViewModel model, IFormFile? nuevoArchivo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "ingreso");
            }

            var tarea = await _context.tareas
                .Include(t => t.Archivo)
                .Include(t => t.Clase)
                .FirstOrDefaultAsync(t => t.TareaId == model.TareaId &&
                                          t.Clase.Modulo.Curso.CursoProfesores
                                           .Any(cp => cp.ProfesorId == user.Id));

            if (tarea == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View("~/Views/profesor/tareas/editar.cshtml", model);

            tarea.Nombre = model.Nombre;
            tarea.Descripcion = model.Descripcion;
            tarea.FechaVencimiento = model.FechaVencimiento.ToUniversalTime();
            tarea.TipoEntregaEsperado = model.TipoEntregaEsperado;

            if (nuevoArchivo != null && nuevoArchivo.Length > 0)
            {
                if (tarea.Archivo != null)
                {
                    await _s3Service.DeleteFileAsync(tarea.Archivo.ArchivoUrl);
                    _context.archivos.Remove(tarea.Archivo);
                }

                var originalName = Path.GetFileName(nuevoArchivo.FileName);
                var extension = Path.GetExtension(originalName).ToLowerInvariant();

                var storedFileName = $"{Guid.NewGuid()}{extension}";

                string newKey = await _s3Service.UploadFileAsync(
                    nuevoArchivo,
                    "asignaciones",
                    storedFileName
                );

                var nuevoArchivoDb = new Archivo
                {
                    ArchivoId = Guid.NewGuid(),
                    ArchivoUrl = newKey,
                    FileName = storedFileName,
                    ContentType = nuevoArchivo.ContentType,
                    SizeInBytes = nuevoArchivo.Length,
                    FechaSubida = DateTime.UtcNow
                };

                _context.archivos.Add(nuevoArchivoDb);
                await _context.SaveChangesAsync();

                tarea.ArchivoId = nuevoArchivoDb.ArchivoId;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("MisCursosYTareas",
                new { cursoId = model.CursoId });
        }
        [HttpPost]
        [Authorize(Roles = "Profesor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(Guid tareaId)
        {
            var tarea = await _context.tareas
                .Include(t => t.Archivo)
                .FirstOrDefaultAsync(t => t.TareaId == tareaId);

            if (tarea == null)
                return NotFound();

            if (tarea.Archivo != null)
            {
                try
                {
                    await _s3Service.DeleteFileAsync(tarea.Archivo.ArchivoUrl);

                    _context.archivos.Remove(tarea.Archivo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"S3 delete error: {ex.Message}");
                }
            }

            _context.tareas.Remove(tarea);

            await _context.SaveChangesAsync();

            return RedirectToAction("MisCursosYTareas", new { cursoId = tarea.Clase.Modulo.CursoId });
        }
    }
}
