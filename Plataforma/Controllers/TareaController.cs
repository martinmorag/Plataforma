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
    [ApiController]
    [Route("api/[controller]")]
    public class TareaController : Controller
    {
        private readonly PlataformaContext _context;
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender _emailSender;
        private readonly S3Service _s3Service;
        private readonly CloudFrontService _cloudFrontService;

        public TareaController(PlataformaContext context, UserManager<UsuarioIdentidad> userManager, IWebHostEnvironment environment, IEmailSender emailSender, S3Service s3Service, CloudFrontService cloudFrontService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _emailSender = emailSender;
            _s3Service = s3Service;
            _cloudFrontService = cloudFrontService;
        }

        [HttpGet("GetTareaDetails")]
        public async Task<IActionResult> GetTareaDetails(Guid tareaId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "ingreso");
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
                ContenidoUrl = tarea.Archivo != null
                    ? _cloudFrontService.GenerateSignedUrl(tarea.Archivo.ArchivoUrl)
                    : null,
                HasSubmitted = submission != null,
                SubmissionStatusText = submission?.Estado?.GetDisplayName() ?? "Pendiente",
                IsSubmittedApproved = submission?.Estado == Entrega.EstadoEntrega.Aprobado, // Using new enum name, adjust if yours is different
                SubmittedFileUrl = submission?.Archivo != null
                    ? _cloudFrontService.GenerateSignedUrl(submission.Archivo.ArchivoUrl)
                    : null,
                SubmissionComentarios = submission?.ComentariosProfesor,
                SubmissionFecha = submission?.FechaEntrega,
                ClaseId = tarea?.ClaseId
            };

            bool isDeadlinePassed = tarea?.FechaVencimiento < DateTime.UtcNow;
            ViewBag.IsBlockedByDeadline = isDeadlinePassed;

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
                return RedirectToAction("Index", "ingreso");
            }

            var tarea = await _context.tareas
                .FirstOrDefaultAsync(t => t.TareaId == progressDto.TareaId);

            if (tarea == null)
            {
                return NotFound();
            }

            if (tarea.FechaVencimiento < DateTime.UtcNow)
            {
                return BadRequest("La fecha límite para ver este video ha vencido.");
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
                return RedirectToAction("Index", "ingreso");
            }

            var estudianteId = user.Id;

            var tarea = await _context.tareas
                            .Include(t => t.Clase)
                                .ThenInclude(c => c.Modulo)
                                    .ThenInclude(m => m.Curso)
                                        .ThenInclude(curso => curso.CursoProfesores)
                                            .ThenInclude(cp => cp.Profesor)
                            .FirstOrDefaultAsync(t => t.TareaId == completionDto.TareaId);

            if (tarea == null)
            {
                return NotFound("Tarea no encontrada.");
            }

            if (tarea.FechaVencimiento < DateTime.UtcNow)
            {
                return BadRequest("La fecha límite para completar esta actividad ha vencido.");
            }

            // Now search for existing submission
            var entrega = await _context.entregas
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
                <body style='margin:0;padding:0;background-color:#f4f6f8;font-family:Arial, Helvetica, sans-serif;'>

                <table align='center' width='600' style='background:white;border-radius:8px;padding:30px;margin-top:30px;box-shadow:0 2px 6px rgba(0,0,0,0.05);'>

                <tr>
                <td>

                <h2 style='color:#2c3e50;margin-bottom:10px;'>Nueva Actividad Completada</h2>

                <p style='font-size:14px;color:#555;'>Hola,</p>

                <p style='font-size:14px;color:#555;line-height:1.6;'>
                Se ha completado la visualización de un video correspondiente a la tarea
                <strong>{taskName}</strong>.
                </p>

                <table width='100%' style='margin-top:20px;border-collapse:collapse;font-size:14px;'>

                <tr>
                <td style='padding:8px;border-bottom:1px solid #eee;'><strong>Estudiante</strong></td>
                <td style='padding:8px;border-bottom:1px solid #eee;'>{studentName}</td>
                </tr>

                <tr>
                <td style='padding:8px;border-bottom:1px solid #eee;'><strong>Tarea</strong></td>
                <td style='padding:8px;border-bottom:1px solid #eee;'>{taskName}</td>
                </tr>

                <tr>
                <td style='padding:8px;border-bottom:1px solid #eee;'><strong>Clase</strong></td>
                <td style='padding:8px;border-bottom:1px solid #eee;'>{className}</td>
                </tr>

                <tr>
                <td style='padding:8px;border-bottom:1px solid #eee;'><strong>Curso</strong></td>
                <td style='padding:8px;border-bottom:1px solid #eee;'>{courseName}</td>
                </tr>

                <tr>
                <td style='padding:8px;'><strong>Fecha de Entrega</strong></td>
                <td style='padding:8px;'>{submissionDate}</td>
                </tr>

                </table>

                <p style='margin-top:30px;font-size:14px;color:#555;'>
                Atentamente,<br>
                <strong>Equipo de la Plataforma Educativa</strong>
                </p>

                </td>
                </tr>

                </table>

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
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "ingreso");
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

            if (tarea.FechaVencimiento < DateTime.UtcNow)
            {
                return BadRequest("La fecha límite de entrega ya ha vencido.");
            }

            if (model.SubmittedFile == null)
            {
                return BadRequest("Una entrega ya ha sido hecha para esta tarea.");
            }

            // Check if the submitted file is a PDF (or other allowed types)
            if (model.SubmittedFile != null)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" }; 
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
            Guid? fileId = null;
            if (model.SubmittedFile != null)
            {
                try
                {
                    var file = model.SubmittedFile;

                    var safeName = file.FileName.Replace(" ", "_");

                    string s3Key = await _s3Service.UploadFileAsync(
                        file,
                        "entregas",
                        safeName
                    );

                    var archivo = new Archivo
                    {
                        ArchivoId = Guid.NewGuid(),
                        ArchivoUrl = s3Key, 
                        FileName = safeName,
                        ContentType = file.ContentType,
                        SizeInBytes = file.Length,
                        FechaSubida = DateTime.UtcNow
                    };

                    _context.archivos.Add(archivo);
                    await _context.SaveChangesAsync();

                    fileId = archivo.ArchivoId;

                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error uploading file: {ex.Message}");
                    return StatusCode(500, "Error uploading file.");
                }
            }   

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
                existingSubmission.Estado = Entrega.EstadoEntrega.EnRevision; // Reset to pending if re-submitting
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
                <body style='margin:0;padding:0;background-color:#f4f6f8;font-family:Arial, Helvetica, sans-serif;'>

                <table align='center' width='600' style='background:white;border-radius:8px;padding:30px;margin-top:30px;box-shadow:0 2px 6px rgba(0,0,0,0.05);'>

                <tr>
                <td>

                <h2 style='color:#2c3e50;margin-bottom:10px;'>Nueva Entrega Recibida</h2>

                <p style='font-size:14px;color:#555;'>Hola,</p>

                <p style='font-size:14px;color:#555;line-height:1.6;'>
                Se ha recibido una nueva entrega correspondiente a la tarea 
                <strong>{taskName}</strong>.
                </p>

                <table width='100%' style='margin-top:20px;border-collapse:collapse;font-size:14px;'>

                <tr>
                <td style='padding:8px;border-bottom:1px solid #eee;'><strong>Estudiante</strong></td>
                <td style='padding:8px;border-bottom:1px solid #eee;'>{studentName}</td>
                </tr>

                <tr>
                <td style='padding:8px;border-bottom:1px solid #eee;'><strong>Tarea</strong></td>
                <td style='padding:8px;border-bottom:1px solid #eee;'>{taskName}</td>
                </tr>

                <tr>
                <td style='padding:8px;border-bottom:1px solid #eee;'><strong>Clase</strong></td>
                <td style='padding:8px;border-bottom:1px solid #eee;'>{className}</td>
                </tr>

                <tr>
                <td style='padding:8px;border-bottom:1px solid #eee;'><strong>Curso</strong></td>
                <td style='padding:8px;border-bottom:1px solid #eee;'>{courseName}</td>
                </tr>

                <tr>
                <td style='padding:8px;'><strong>Fecha de Entrega</strong></td>
                <td style='padding:8px;'>{submissionDate}</td>
                </tr>

                </table>

                <div style='text-align:center;margin-top:30px;'>

                <a href='{Url.Action("VerEntregas", "tareas", new { tareaId = tarea.TareaId }, Request.Scheme)}'
                style='background-color:#2e86de;color:white;padding:12px 24px;text-decoration:none;border-radius:6px;font-size:14px;font-weight:bold;display:inline-block;'>
                Revisar Entrega
                </a>

                </div>

                <p style='margin-top:30px;font-size:14px;color:#555;'>
                Atentamente,<br>
                <strong>Equipo de la Plataforma Educativa</strong>
                </p>

                </td>
                </tr>

                </table>

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
