using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using System.Net.Http;
using Xabe.FFmpeg;

namespace Plataforma.Controllers
{
    public class tareasController : Controller
    {
        private readonly PlataformaContext _context;
        private readonly HttpClient _httpClient; 
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public tareasController(PlataformaContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration, IWebHostEnvironment environment) // Modify this
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration; // Add this
            _environment = environment;
        }
        [HttpGet("profesor/tareas/crear")]
        public IActionResult crear(Guid claseId, string contentType)
        {
            var clase = _context.clases.FirstOrDefault(c => c.ClaseId == claseId);

            if (clase == null)
            {
                return NotFound();
            }

            // Prepare the data to be passed to the view
            var tarea = new Tarea
            {
                Clase = clase,
                TipoEntregaEsperado = contentType,
                ClaseId = claseId
            };

            // Pass the data to the view
            return View("~/Views/profesor/tareas/crear.cshtml", tarea);
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // Always add this for POST actions that modify data
        public async Task<IActionResult> Create(Tarea asignacionModel, IFormFile? archivoAsignacion) // Renamed 'model' to 'asignacionModel' and 'Archivo' to 'archivoAsignacion' for clarity
        {
            // First, validate basic model properties received from the form
            // Ensure ClaseId is valid and exists
            if (!ModelState.IsValid || asignacionModel.ClaseId == Guid.Empty)
            {
                // Re-fetch Clase for the view if model state is invalid
                asignacionModel.Clase = await _context.clases.FindAsync(asignacionModel.ClaseId);
                return View("Crear", asignacionModel);
            }

            // Optional: Validate TipoEntregaEsperado if you have a predefined list (e.g., enum values)
            // if (!new string[] { "pdf", "video", "image", "any" }.Contains(asignacionModel.TipoEntregaEsperado.ToLower()))
            // {
            //     ModelState.AddModelError(nameof(asignacionModel.TipoEntregaEsperado), "Tipo de entrega esperado no válido.");
            //     asignacionModel.Clase = await _context.Clases.FindAsync(asignacionModel.ClaseId);
            //     return View("Crear", asignacionModel);
            // }

            Archivo? archivoGuardado = null;

            // Handle the upload of the assignment's own file (instructions, template, etc.)
            if (archivoAsignacion != null && archivoAsignacion.Length > 0)
            {
                // --- File Type Validation for the ASSIGNMENT'S FILE ---
                var isPdf = archivoAsignacion.ContentType == "application/pdf";
                var isVideo = archivoAsignacion.ContentType.StartsWith("video/");

                // You might allow other types for the assignment's own file, or be more restrictive.
                // This validation is for the teacher's uploaded file.
                if (!isPdf && !isVideo && !archivoAsignacion.ContentType.StartsWith("image/")) // Example: allow PDF, video, image
                {
                    ModelState.AddModelError("archivoAsignacion", "Solo se permiten archivos PDF, videos o para la asignación.");
                    asignacionModel.Clase = await _context.clases.FindAsync(asignacionModel.ClaseId);
                    return View("Crear", asignacionModel);
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "asignaciones"); // Dedicated folder for assignment files
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{archivoAsignacion.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                var fileUrl = $"/uploads/asignaciones/{uniqueFileName}"; // Relative URL for database

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await archivoAsignacion.CopyToAsync(fileStream);
                }

                // Create a new Archivo record for the uploaded file
                archivoGuardado = new Archivo
                {
                    ArchivoId = Guid.NewGuid(),
                    ArchivoUrl = fileUrl,
                    FileName = archivoAsignacion.FileName,
                    ContentType = archivoAsignacion.ContentType,
                    SizeInBytes = archivoAsignacion.Length,
                    FechaSubida = DateTime.UtcNow // Store in UTC
                };
                
                // If it's a video, attempt to get duration (this is complex and usually involves external libraries/services)
                if (isVideo)
                {
                    try
                    {
                        var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
                        var videoStream = mediaInfo.VideoStreams.FirstOrDefault();

                        if (videoStream != null)
                        {
                            archivoGuardado.DuracionVideo = videoStream.Duration; // Assign directly as TimeSpan
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error calculating video duration with FFMpegCore for {filePath}: {ex.Message}");
                        archivoGuardado.DuracionVideo = null;
                    }
                }

                _context.archivos.Add(archivoGuardado);
                await _context.SaveChangesAsync(); // Save Archivo first to get its ID
            }

            // Create the Asignacion record
            var nuevaAsignacion = new Tarea
            {
                TareaId = Guid.NewGuid(),
                Nombre = asignacionModel.Nombre,
                Descripcion = asignacionModel.Descripcion,
                FechaVencimiento = asignacionModel.FechaVencimiento.ToUniversalTime(), // Ensure UTC
                ClaseId = asignacionModel.ClaseId,
                TipoEntregaEsperado = asignacionModel.TipoEntregaEsperado // Set the expected submission type
            };

            // Link the saved Archivo to the Asignacion if a file was uploaded
            if (archivoGuardado != null)
            {
                nuevaAsignacion.ArchivoId = archivoGuardado.ArchivoId;
            }

            _context.tareas.Add(nuevaAsignacion);
            await _context.SaveChangesAsync();

            // Redirect to a suitable action, e.g., the details of the new assignment or the class assignments list
            return RedirectToAction("cursos", "Index");
            // Or return RedirectToAction("Index", "Clase", new { id = nuevaAsignacion.ClaseId });
        }
    }
}
