using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Plataforma.Models
{
    public class Tarea
    {
        
        [Key]
        public Guid TareaId { get; set; } // Unique identifier for the task
        public string Nombre { get; set; }
        public string Descripcion { get; set; } // Optional description of the assignment

        [Required]
        [ForeignKey("Clase")]
        public Guid ClaseId { get; set; }
        [ValidateNever]
        public Clase Clase { get; set; }

        [StringLength(50)] // e.g., "video", "pdf", "any"
        public string TipoEntregaEsperado { get; set; } = "any";

        [ForeignKey("Archivo")] // Specify Archivo as the related table
        public Guid? ArchivoId { get; set; } // Nullable foreign key to the Archivo table
        [ValidateNever]
        public Archivo? Archivo { get; set; } // Navigation property to the assignment's file

        private DateTime _fechaVencimiento;
        public DateTime FechaVencimiento // Due date for the assignment
        {
            get => _fechaVencimiento;
            set
            {
                if (value.Kind != DateTimeKind.Utc)
                {
                    _fechaVencimiento = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                }
                else
                {
                    _fechaVencimiento = value;
                }
            }
        }
    }
}
