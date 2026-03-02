using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Plataforma.Models
{
    public class CursoProfesor
    {
        public Guid CursoId { get; set; }
        public Curso Curso { get; set; }

        public Guid ProfesorId { get; set; }
        public Profesor Profesor { get; set; }
    }
}
