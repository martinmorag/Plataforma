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
        public string? ReunionUrl { get; set; }
        private DateTime? _fechaReunion;

        public DateTime? FechaReunion
        {
            get => _fechaReunion;
            set
            {
                if (value.HasValue)
                {
                    _fechaReunion = value.Value.Kind == DateTimeKind.Utc
                        ? value
                        : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                }
                else
                {
                    _fechaReunion = null;
                }
            }
        }

        [Required]
        [ForeignKey("Clase")]
        public Guid ClaseId { get; set; }
        [ValidateNever]
        public Clase Clase { get; set; }

        [StringLength(50)] // e.g., "video", "pdf", "any"
        public string TipoEntregaEsperado { get; set; } = "any";
        [Url]
        public string? UrlEntrega { get; set; }
        [ForeignKey("Archivo")] // Specify Archivo as the related table
        public Guid? ArchivoId { get; set; } // Nullable foreign key to the Archivo table
        [ValidateNever]
        public Archivo? Archivo { get; set; } // Navigation property to the assignment's file
        public string? GrabacionUrl { get; set; }

        private DateTime? _fechaVencimiento;
        public DateTime? FechaVencimiento // Due date for the assignment
        {
            get => _fechaVencimiento;
            set
            {
                if (value.HasValue)
                {
                    _fechaVencimiento = value.Value.Kind == DateTimeKind.Utc
                        ? value
                        : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                }
                else
                {
                    _fechaVencimiento = null;
                }
            }
        }
        public ICollection<Entrega>? Entregas { get; set; }
    }
}
