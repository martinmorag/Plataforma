using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Inicio;

namespace Plataforma.Controllers
{
    public class inicioController : Controller
    {
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly PlataformaContext _context;
        private readonly SignInManager<UsuarioIdentidad> _signInManager;
        private readonly CloudFrontService _cloudFrontService;

        public inicioController(UserManager<UsuarioIdentidad> userManager, PlataformaContext context, SignInManager<UsuarioIdentidad> signInManager, CloudFrontService cloudFrontService)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _cloudFrontService = cloudFrontService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is not Estudiante estudiante)
            {
                await _signInManager.SignOutAsync();

                return RedirectToAction("Index", "ingreso");
            }

            var cursos = await _context.CursoEstudiantes
                .Where(ce => ce.EstudianteId == estudiante.Id)
                .Include(ce => ce.Curso)
                    .ThenInclude(c => c.Modulos)
                .Select(ce => ce.Curso)
                .ToListAsync();

            return View(cursos);
        }
        public async Task<IActionResult> cursos()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is not Estudiante estudiante)
            {
                await _signInManager.SignOutAsync();

                return RedirectToAction("Index", "ingreso");
            }

            var cursosRegistrados = _context.CursoEstudiantes
                .Where(ce => ce.EstudianteId == user.Id)
                .Select(c => c.CursoId)
                .ToList();
            var cursos = _context.cursos
                .Where(c => !cursosRegistrados.Contains(c.CursoId))
                .ToList();

            foreach (var curso in cursos)
            {
                if (!string.IsNullOrEmpty(curso.ImageUrl))
                {
                    curso.ImageUrl = _cloudFrontService.GenerateSignedUrl(curso.ImageUrl);
                }
            }


            var cursosActuales = await _context.CursoEstudiantes
                .Where(ce => ce.EstudianteId == estudiante.Id)
                .Include(ce => ce.Curso)
                .ToListAsync();

            foreach (var ce in cursosActuales)
            {
                if (!string.IsNullOrEmpty(ce.Curso.ImageUrl))
                {
                    ce.Curso.ImageUrl = _cloudFrontService.GenerateSignedUrl(ce.Curso.ImageUrl);
                }
            }

            var viewModel = new SeleccionCursos
            {
                Cursos = cursos,
                CursosEstudiante = cursosActuales
            };

            return View(viewModel);
        }
        public async Task<IActionResult> miscursos()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is not Estudiante estudiante)
            {
                await _signInManager.SignOutAsync();

                return RedirectToAction("Index", "ingreso");
            }

            var cursos = await _context.CursoEstudiantes
                .Where(ce => ce.EstudianteId == estudiante.Id)
                .Select(ce => ce.Curso)
                .ToListAsync();

            foreach (var curso in cursos)
            {
                if (!string.IsNullOrEmpty(curso.ImageUrl))
                {
                    curso.ImageUrl = _cloudFrontService.GenerateSignedUrl(curso.ImageUrl);
                }
            }

            var MisCursos = new Models.Estudiantes.MisCursosViewModel
            {
                cursos = cursos
            };

            return View(MisCursos);
        }
    }
}
