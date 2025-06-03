using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Profesores;

namespace Plataforma.Controllers
{
    [Route("api/[controller]")] // This means all API actions in this controller will start with /api/Profesores
    [ApiController]
    public class ApiProfesoresController : ControllerBase
    {
        private readonly PlataformaContext _context;
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ApiProfesoresController(PlataformaContext context, UserManager<UsuarioIdentidad> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // API Endpoint for Professor to Download a Submitted File
        // The route here will be /api/Profesores/DownloadSubmittedFile/{entregaId}
        [HttpGet("DownloadSubmittedFile/{entregaId}")]
        public async Task<IActionResult> DownloadSubmittedFile(Guid entregaId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            Guid profesorId = user.Id;

            var entrega = await _context.entregas
                                 .Include(e => e.Archivo)
                                 .FirstOrDefaultAsync(e => e.EntregaId == entregaId);

            if (entrega == null || entrega.Archivo == null)
            {
                return NotFound("Entrega o archivo no encontrado.");
            }

            // Construct the absolute path to the file
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, entrega.Archivo.ArchivoUrl.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("El archivo no existe en el servidor.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = entrega.Archivo.ContentType ?? "application/octet-stream";
            var fileName = entrega.Archivo.FileName;

            return File(fileBytes, contentType, fileName);
        }
        [HttpPost("EvaluarEntrega")]
        public async Task<IActionResult> EvaluarEntrega([FromBody] EvaluarEntregaDto evaluacionDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            Guid profesorId = user.Id;

            var entrega = await _context.entregas
                                        .Include(e => e.Tarea).ThenInclude(t => t.Clase)
                                        .FirstOrDefaultAsync(e => e.EntregaId == evaluacionDto.EntregaId);

            if (entrega == null)
            {
                return NotFound("Entrega no encontrada.");
            }

            entrega.Estado = evaluacionDto.Estado;
            entrega.ComentariosProfesor = evaluacionDto.ComentariosProfesor;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Entrega evaluada con éxito.", newStatus = evaluacionDto.Estado.ToString() });
        }
    }
}
