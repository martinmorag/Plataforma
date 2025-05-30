using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Plataforma.Models
{
    public class Curso
    {
        [Key]
        public Guid CursoId { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; }
        public bool Disponible { get; set; }
        // Navigation property: A course can have many modules
        public ICollection<Modulo> Modulos { get; set; } = new List<Modulo>();

        // Navigation property: Many-to-many with Estudiante through CursoEstudiante
        public ICollection<ProfesorCursoDto> CursoEstudiantes { get; set; } = new List<ProfesorCursoDto>();

        // Navigation property: Many-to-many with Profesor through CursoProfesor
        public ICollection<CursoProfesor> CursoProfesores { get; set; } = new List<CursoProfesor>();
    }
}
