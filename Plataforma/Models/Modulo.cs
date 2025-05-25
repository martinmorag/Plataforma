using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Plataforma.Models
{
    public class Modulo
    {
        [Key]
        public Guid ModuloId { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; }
        public int Order { get; set; }

        // Foreign Key: Connects Module to Course
        [ForeignKey("Curso")]
        public Guid CursoId { get; set; }

        // Navigation property:
        [ValidateNever]
        public Curso Curso { get; set; }

        // Navigation property: A module can have many classes
        public ICollection<Clase> Clases { get; set; } = new List<Clase>();
    }
}
