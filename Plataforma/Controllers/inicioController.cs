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

        public inicioController(UserManager<UsuarioIdentidad> userManager, PlataformaContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is not Estudiante estudiante)
            {
                return Unauthorized();
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
                return Unauthorized();
            }

            var cursosRegistrados = _context.CursoEstudiantes
                .Where(ce => ce.EstudianteId == user.Id)
                .Select(c => c.CursoId)
                .ToList();
            var cursos = _context.cursos
                .Where(c => !cursosRegistrados.Contains(c.CursoId))
                .ToList();


            var cursosActuales = await _context.CursoEstudiantes
                .Where(ce => ce.EstudianteId == estudiante.Id)
                .Include(ce => ce.Curso)
                .ToListAsync();

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
                return Unauthorized();
            }

            var cursos = await _context.CursoEstudiantes
                .Where(ce => ce.EstudianteId == estudiante.Id)
                .Select(ce => ce.Curso)
                .ToListAsync();

            var MisCursos = new Models.Estudiantes.MisCursosViewModel
            {
                cursos = cursos
            };

            return View(MisCursos);
        }
    }
}
