using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Cursos;

namespace Plataforma.Controllers
{
    [ApiController] // Keep this
    [Route("api/[controller]")]
    public class TareaController : Controller
    {
        private readonly PlataformaContext _context;
        private readonly UserManager<UsuarioIdentidad> _userManager;

        public TareaController(PlataformaContext context, UserManager<UsuarioIdentidad> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                SubmittedFileUrl = submission?.Archivo.ArchivoUrl,
                SubmissionComentarios = submission?.ComentariosProfesor,
                SubmissionFecha = submission?.FechaEntrega
            };

            // Pass video progress data to the View (via ViewBag or by extending TareaDetailsViewModel)
            ViewBag.InitialVideoTime = progresoVideo;
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
                    ArchivoId = entrega.ArchivoId
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

            var entrega = await _context.entregas
                .FirstOrDefaultAsync(e => e.TareaId == completionDto.TareaId && e.EstudianteId == estudianteId);

            if (entrega == null)
            {
                entrega = new Entrega
                {
                    EntregaId = Guid.NewGuid(),
                    TareaId = completionDto.TareaId,
                    ArchivoId = entrega.ArchivoId,
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
            return Ok();
        }

    }
}
