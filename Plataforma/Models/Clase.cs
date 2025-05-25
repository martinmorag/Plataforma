using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Plataforma.Models
{
    public class Clase
    {
        [Key]
        public Guid ClaseId { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; }
        public int Order { get; set; }

        // Foreign Key: Connects Class to Module
        [ForeignKey("Modulo")]
        public Guid ModuloId { get; set; }

        // Navigation property:
        [ValidateNever]
        public Modulo Modulo { get; set; }
        [ValidateNever]
        public ICollection<Tarea> Tareas { get; set; }
    }
}
