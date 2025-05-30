using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Cursos;
using Plataforma.Servicios;
using System.Threading;

namespace Plataforma.Controllers
{
    [ApiController] // Keep this
    [Route("api/[controller]")]
    public class TareaController : Controller
    {
        private readonly PlataformaContext _context;
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender _emailSender; // Inject the email sender

        public TareaController(PlataformaContext context, UserManager<UsuarioIdentidad> userManager, IWebHostEnvironment environment, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _emailSender = emailSender;
        }

        [HttpGet("GetTareaDetails")]
        public async Task<IActionResult> GetTareaDetails(Guid tareaId) // 'id' will be the TareaId
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var estudianteId = user.Id;

            var tarea = await _context.tareas
                                    .Include(t => t.Clase)
                                    .Include(t => t.Archivo)
                                    .FirstOrDefaultAsync(t => t.TareaId == tareaId);

            if (tarea == null)
            {
                return NotFound();
            }

            var submission = await _context.entregas
                                .Include(e => e.Archivo)
                                .FirstOrDefaultAsync(e => e.TareaId == tareaId && e.EstudianteId == estudianteId);

            TimeSpan? progresoVideo = null;
            bool videoCompletado = false;

            if (tarea.TipoEntregaEsperado == "video" && tarea.Archivo.DuracionVideo.HasValue && submission?.ProgresoVideo.HasValue == true)
            {
                var progress = submission.ProgresoVideo.Value.TotalSeconds;
                progresoVideo = submission.ProgresoVideo.Value;
                var duration = tarea.Archivo.DuracionVideo.Value.TotalSeconds;

                videoCompletado = (progress / duration) >= 0.95;
            }

            var tareaDetailsViewModel = new TareaDetailsViewModel
            {
                TareaId = tarea.TareaId,
                Nombre = tarea.Nombre,
                Descripcion = tarea.Descripcion,
                FechaLimite = tarea.FechaVencimiento,
                TipoContenido = tarea.TipoEntregaEsperado,
                ContenidoUrl = tarea.Archivo?.ArchivoUrl,

                HasSubmitted = submission != null,
                SubmissionStatusText = submission?.Estado.ToString() ?? "Pendiente",
                IsSubmittedApproved = submission?.Estado == Entrega.EstadoEntrega.Aprobado, // Using new enum name, adjust if yours is different
                SubmittedFileUrl = submission?.Archivo?.ArchivoUrl,
                SubmissionComentarios = submission?.ComentariosProfesor,
                SubmissionFecha = submission?.FechaEntrega,
                ClaseId = tarea?.ClaseId
            };

            // Pass video progress data to the View (via ViewBag or by extending TareaDetailsViewModel)
            ViewBag.InitialVideoTime = progresoVideo?.TotalSeconds ?? 0;
            ViewBag.VideoCompleted = videoCompletado;

            return PartialView("_TareaDetailsPartial", tareaDetailsViewModel);
        }

        // Video progress
        [HttpPost("SaveVideoProgress")]
        public async Task<IActionResult> SaveVideoProgress([FromBody] VideoProgressDto progressDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var estudianteId = user.Id;

            var entrega = await _context.entregas
                .FirstOrDefaultAsync(e => e.TareaId == progressDto.TareaId && e.EstudianteId == estudianteId);

            if (entrega == null)
            {
                entrega = new Entrega
                {
                    EntregaId = Guid.NewGuid(),
                    TareaId = progressDto.TareaId,
                    EstudianteId = estudianteId,
                    ProgresoVideo = TimeSpan.FromSeconds(progressDto.CurrentTime),
                    Estado = Entrega.EstadoEntrega.EnProgreso,
                    FechaEntrega = null,
                    ArchivoId = null
                };
                _context.entregas.Add(entrega);
            }
            else
            {
                if (entrega.ProgresoVideo == null || progressDto.CurrentTime > entrega.ProgresoVideo.Value.TotalSeconds)
                {
                    entrega.ProgresoVideo = TimeSpan.FromSeconds(progressDto.CurrentTime);
                    if (entrega.Estado != Entrega.EstadoEntrega.Aprobado)
                    {
                        entrega.Estado = Entrega.EstadoEntrega.EnProgreso;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }


        [HttpPost("MarkVideoTaskCompleted")]
        public async Task<IActionResult> MarkVideoTaskCompleted([FromBody] TareaCompletionDto completionDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var estudianteId = user.Id;

            var entrega = await _context.entregas.Include(t => t.Tarea)
                                            .ThenInclude(c => c.Clase)
                                                .ThenInclude(c => c.Modulo) // Then includes the Modulo related to the Clase
                                                    .ThenInclude(m => m.Curso) // Then includes the Curso related to the Modulo
                                                        .ThenInclude(curso => curso.CursoProfesores) // Then includes the join entity CursoProfesores
                                                            .ThenInclude(cp => cp.Profesor)
                                            .FirstOrDefaultAsync(e => e.TareaId == completionDto.TareaId && e.EstudianteId == estudianteId);

            if (entrega == null)
            {
                entrega = new Entrega
                {
                    EntregaId = Guid.NewGuid(),
                    TareaId = completionDto.TareaId,
                    ArchivoId = null,
                    EstudianteId = estudianteId,
                    ProgresoVideo = TimeSpan.FromSeconds(completionDto.VideoDuration),
                    Estado = Entrega.EstadoEntrega.Aprobado,
                    FechaEntrega = DateTime.UtcNow,
                    ComentariosProfesor = "Tarea completada automáticamente al ver el video."

                };
                _context.entregas.Add(entrega);
            }
            else
            {
                entrega.ProgresoVideo = TimeSpan.FromSeconds(completionDto.VideoDuration);

                if (entrega.Estado != Entrega.EstadoEntrega.Aprobado)
                {
                    entrega.Estado = Entrega.EstadoEntrega.Aprobado;
                    entrega.FechaEntrega = DateTime.UtcNow;
                    entrega.ComentariosProfesor = (entrega.ComentariosProfesor ?? "") +
                        "\n(Actualizado: Video completado automáticamente)";
                }
            }

            await _context.SaveChangesAsync();

            var studentName = $"{user.Nombre} {user.Apellido}"; // Or get from a profile property like user.NombreCompleto
            string courseName = entrega.Tarea.Clase.Modulo.Curso.Nombre;
            string className = entrega.Tarea.Clase.Nombre;
            string taskName = entrega.Tarea.Nombre;
            string submissionDate = DateTime.Now.ToString();

            string subject = $"Nueva Entrega: {taskName} ({className} - {courseName})";
            string messageBody = $@"
            <html>
            <body>
                <p>Hola,</p>
                <p>Se ha completado de ver un video para la tarea <strong>{taskName}</strong>.</p>
                <ul>
                    <li><strong>Estudiante:</strong> {studentName}</li>
                    <li><strong>Tarea:</strong> {taskName}</li>
                    <li><strong>Clase:</strong> {className}</li>
                    <li><strong>Curso:</strong> {courseName}</li>
                    <li><strong>Fecha de Entrega:</strong> {submissionDate}</li>
                </ul>
               
                <p>Atentamente,<br/>El equipo de la Plataforma Educativa</p>
            </body>
            </html>";

            // Get all professors associated with the course
            var professors = entrega.Tarea.Clase.Modulo.Curso.CursoProfesores.Select(cp => cp.Profesor).ToList();

            foreach (var professor in professors)
            {
                if (!string.IsNullOrEmpty(professor.Email))
                {
                    try
                    {
                        await _emailSender.SendEmailAsync(professor.Email, subject, messageBody);
                        Console.WriteLine($"Email sent to professor '{professor.Email}' for submission of TareaId: {entrega.Tarea.TareaId}, EntregaId: {entrega.EntregaId}.");
                    }
                    catch (Exception ex)
                    {
                        // --- MORE DETAILED ERROR MESSAGE ---
                        Console.Error.WriteLine("------------------------------------------------------------------------------------------------------------------------------------------------------");
                        Console.Error.WriteLine($"ERROR: Failed to send email to professor '{professor.Email}' for TareaId: {entrega.Tarea.TareaId}, EntregaId: {entrega.EntregaId}.");
                        Console.Error.WriteLine($"Exception Type: {ex.GetType().Name}");
                        Console.Error.WriteLine($"Message: {ex.Message}");

                        if (ex.InnerException != null)
                        {
                            Console.Error.WriteLine($"Inner Exception Type: {ex.InnerException.GetType().Name}");
                            Console.Error.WriteLine($"Inner Exception Message: {ex.InnerException.Message}");
                        }

                        Console.Error.WriteLine($"Stack Trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine($"WARNING: Professor '{professor.Nombre}' has no email address for TareaId: {entrega.Tarea.TareaId}, EntregaId: {entrega.EntregaId}.");
                }
            }


            TempData["SuccessMessage"] = "Entrega realizada con éxito.";

            return Ok();
        }
        [HttpPost("SubmitAssignment")]
        [Consumes("multipart/form-data")] // Important for file uploads
        public async Task<IActionResult> SubmitAssignment([FromForm] SubmissionFormViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not authenticated.");
            }

            if (!ModelState.IsValid)
            {
                // Log model state errors if needed for debugging
                return BadRequest(ModelState);
            }

            var estudianteId = user.Id;

            var tarea = await _context.tareas.Include(t => t.Clase) // Includes the Clase related to the Tarea
                                .ThenInclude(c => c.Modulo) // Then includes the Modulo related to the Clase
                                    .ThenInclude(m => m.Curso) // Then includes the Curso related to the Modulo
                                        .ThenInclude(curso => curso.CursoProfesores) // Then includes the join entity CursoProfesores
                                            .ThenInclude(cp => cp.Profesor) // Finally, includes the actual Profesor details from CursoProfesores
                                .FirstOrDefaultAsync(t => t.TareaId == model.TareaId);
            if (tarea == null)
            {
                return NotFound("Tarea not found.");
            }

            // Check if a file is required and provided
            if (model.SubmittedFile == null)
            {
                return BadRequest("A file submission is required for this task.");
            }

            // Check if the submitted file is a PDF (or other allowed types)
            if (model.SubmittedFile != null)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" }; // Define allowed types
                var fileExtension = Path.GetExtension(model.SubmittedFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest($"Unsupported file type. Allowed types are: {string.Join(", ", allowedExtensions)}");
                }

                // Optional: Check file size
                if (model.SubmittedFile.Length > 10 * 1024 * 1024) // Max 10MB
                {
                    return BadRequest("File size exceeds 10MB limit.");
                }
            }


            // --- File Upload Logic ---
            string filePath = null;
            Guid? fileId = null;
            if (model.SubmittedFile != null)
            {
                try
                {
                    // Define the path to save files (e.g., wwwroot/submissions)
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "entregas");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate a unique file name to prevent collisions
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.SubmittedFile.FileName);
                    filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.SubmittedFile.CopyToAsync(fileStream);
                    }

                    // Save file metadata to the Archivo table
                    var archivo = new Archivo
                    {
                        ArchivoId = Guid.NewGuid(),
                        FileName = model.SubmittedFile.FileName,
                        ArchivoUrl = "/entregas/" + uniqueFileName, // Store relative path
                        ContentType = model.SubmittedFile.ContentType,
                        SizeInBytes = model.SubmittedFile.Length
                    };
                    _context.archivos.Add(archivo);
                    await _context.SaveChangesAsync(); // Save Archivo first to get its ID
                    fileId = archivo.ArchivoId;

                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error uploading file: {ex.Message}");
                    return StatusCode(500, "Error uploading file.");
                }
            }
            // --- End File Upload Logic ---


            // Check if an existing submission exists for this task and student
            var existingSubmission = await _context.entregas
                                                .FirstOrDefaultAsync(e => e.TareaId == model.TareaId && e.EstudianteId == estudianteId);

            Guid entregaid;
            if (existingSubmission == null)
            {
                // Create new submission
                var newSubmission = new Entrega
                {
                    EntregaId = Guid.NewGuid(),
                    TareaId = model.TareaId,
                    EstudianteId = estudianteId,
                    FechaEntrega = DateTime.Now,
                    Estado = Entrega.EstadoEntrega.EnRevision, // Initial status is pending review
                    ArchivoId = fileId // Link to the uploaded file if any
                };
                entregaid = newSubmission.EntregaId;
                _context.entregas.Add(newSubmission);
            }
            else
            {
                entregaid = existingSubmission.EntregaId;
                existingSubmission.FechaEntrega = DateTime.Now;
                existingSubmission.Estado = Entrega.EstadoEntrega.EnProgreso; // Reset to pending if re-submitting
                existingSubmission.ArchivoId = fileId; // Update linked file
                // Optional: Remove old file if it was replaced
                // if (existingSubmission.ArchivoId != null && existingSubmission.ArchivoId != fileId) { /* Delete old file logic */ }
            }

            await _context.SaveChangesAsync();

            var studentName = $"{user.Nombre} {user.Apellido}"; // Or get from a profile property like user.NombreCompleto
            string courseName = tarea.Clase.Modulo.Curso.Nombre;
            string className = tarea.Clase.Nombre;
            string taskName = tarea.Nombre;
            string submissionDate = DateTime.Now.ToString();

            string subject = $"Nueva Entrega: {taskName} ({className} - {courseName})";
            string messageBody = $@"
            <html>
            <body>
                <p>Hola,</p>
                <p>Se ha realizado una nueva entrega para la tarea <strong>{taskName}</strong>.</p>
                <ul>
                    <li><strong>Estudiante:</strong> {studentName}</li>
                    <li><strong>Tarea:</strong> {taskName}</li>
                    <li><strong>Clase:</strong> {className}</li>
                    <li><strong>Curso:</strong> {courseName}</li>
                    <li><strong>Fecha de Entrega:</strong> {submissionDate}</li>
                </ul>
                <p>Puedes revisar la entrega en la plataforma:</p>
                <p><a href='{Url.Action("VerEntregas", "tareas", new { tareaId = tarea.TareaId }, Request.Scheme)}'>Revisar Entrega</a></p>
                
                <p>Atentamente,<br/>El equipo de la Plataforma Educativa</p>
            </body>
            </html>";

            // Get all professors associated with the course
            var professors = tarea.Clase.Modulo.Curso.CursoProfesores.Select(cp => cp.Profesor).ToList();

            foreach (var professor in professors)
            {
                if (!string.IsNullOrEmpty(professor.Email))
                {
                    try
                    {
                        await _emailSender.SendEmailAsync(professor.Email, subject, messageBody);
                        Console.WriteLine($"Email sent to professor '{professor.Email}' for submission of TareaId: {tarea.TareaId}, EntregaId: {entregaid}.");
                    }
                    catch (Exception ex)
                    {
                        // --- MORE DETAILED ERROR MESSAGE ---
                        Console.Error.WriteLine("------------------------------------------------------------------------------------------------------------------------------------------------------");
                        Console.Error.WriteLine($"ERROR: Failed to send email to professor '{professor.Email}' for TareaId: {tarea.TareaId}, EntregaId: {entregaid}.");
                        Console.Error.WriteLine($"Exception Type: {ex.GetType().Name}");
                        Console.Error.WriteLine($"Message: {ex.Message}");

                        if (ex.InnerException != null)
                        {
                            Console.Error.WriteLine($"Inner Exception Type: {ex.InnerException.GetType().Name}");
                            Console.Error.WriteLine($"Inner Exception Message: {ex.InnerException.Message}");
                        }

                        Console.Error.WriteLine($"Stack Trace: {ex.StackTrace}");                    }
                }
                else
                {
                    Console.WriteLine($"WARNING: Professor '{professor.Nombre}' has no email address for TareaId: {tarea.TareaId}, EntregaId: {entregaid}.");
                }
            }


            TempData["SuccessMessage"] = "Entrega realizada con éxito.";

            return Ok(new { message = "Assignment submitted successfully!", submissionStatus = Entrega.EstadoEntrega.EnRevision.ToString() });
        }

    }
}
