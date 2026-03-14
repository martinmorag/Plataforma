using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Profesores;

namespace Plataforma.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class ApiProfesoresController : ControllerBase
    {
        private readonly PlataformaContext _context;
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly CloudFrontService _cloudFrontService;

        public ApiProfesoresController(PlataformaContext context, UserManager<UsuarioIdentidad> userManager, IWebHostEnvironment webHostEnvironment, CloudFrontService cloudFrontService)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _cloudFrontService = cloudFrontService;

        }

        // API Endpoint for Professor to Download a Submitted File
        // The route here will be /api/Profesores/DownloadSubmittedFile/{entregaId}
        [HttpGet("AccessSubmittedFile/{entregaId}")]
        public async Task<IActionResult> AccessSubmittedFile(Guid entregaId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "ingreso");
            }

            var entrega = await _context.entregas
                .Include(e => e.Archivo)
                .FirstOrDefaultAsync(e => e.EntregaId == entregaId);

            if (entrega == null || entrega.Archivo == null)
                return NotFound("Entrega o archivo no encontrado.");

            var signedUrl = _cloudFrontService.GenerateSignedUrl(entrega.Archivo.ArchivoUrl);

            return Redirect(signedUrl);
        }
        [HttpPost("EvaluarEntrega")]
        public async Task<IActionResult> EvaluarEntrega([FromBody] EvaluarEntregaDto evaluacionDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "ingreso");
            }

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
