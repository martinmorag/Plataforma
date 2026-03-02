using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Plataforma.Models
{
    public class CursoEstudiante
    {

        public Guid CursoId { get; set; }
        public Curso Curso { get; set; }

        public Guid EstudianteId { get; set; }
        public Estudiante Estudiante { get; set; }
    }
}
