using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Plataforma.Models
{
    public class CursoProfesor
    {
        [Key]
        public Guid CursoProfesorId { get; set; }

        [ForeignKey("Curso")]
        public Guid CursoId { get; set; }
        public Curso Curso { get; set; }

        [ForeignKey("Profesor")]
        public Guid ProfesorId { get; set; }
        public Profesor Profesor { get; set; }
    }
}
