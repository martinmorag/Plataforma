using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Plataforma.Models
{
    public class CursoEstudiante
    {
        [Key]
        public Guid CursoEstudianteId { get; set; }

        [ForeignKey("Curso")]
        public Guid CursoId { get; set; }
        public Curso Curso { get; set; }

        [ForeignKey("Estudiante")]
        public Guid EstudianteId { get; set; }
        public Estudiante Estudiante { get; set; }
    }
}
