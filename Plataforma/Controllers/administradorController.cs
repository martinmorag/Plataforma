using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Plataforma.Models.Administracion;

namespace Plataforma.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class administradorController : Controller
    {
        private readonly UserManager<UsuarioIdentidad> _userManager;
        private readonly PlataformaContext _context;
        public administradorController(UserManager<UsuarioIdentidad> userManager, PlataformaContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
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
            if (usuario == null)
            {
                return NotFound();
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
            if (usuario == null)
            {
                return NotFound();
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
