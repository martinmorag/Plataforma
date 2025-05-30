using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Plataforma.Models
{
    public class Entrega
    {
        [Key]
        public Guid EntregaId { get; set; } // Unique identifier for the submission

        [Required]
        [ForeignKey("Tarea")]
        public Guid TareaId { get; set; }
        [ValidateNever]
        public Tarea Tarea { get; set; } // Navigation property to the assignment

        [Required]
        [ForeignKey("Estudiante")]
        public Guid EstudianteId { get; set; }
        [ValidateNever]
        public Estudiante Estudiante { get; set; } // Navigation property to the student

        // --- Status of the submission ---
        public enum EstadoEntrega
        {
            EnRevision,
            EnProgreso,
            Aprobado,
            Reprobado,
            Rehacer
        }

        [Required]
        public EstadoEntrega? Estado { get; set; } = EstadoEntrega.EnProgreso; // Default status

        private DateTime? _fechaEntrega;
        public DateTime? FechaEntrega
        {
            get => _fechaEntrega;
            set
            {
                if (value.HasValue)
                {
                    _fechaEntrega = value.Value.Kind != DateTimeKind.Utc
                        ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                        : value.Value;
                }
                else
                {
                    _fechaEntrega = null;
                }
            }
        }

        // Optional comments from the teacher
        [StringLength(1000)]
        public string? ComentariosProfesor { get; set; }
        // if it's a video
        public TimeSpan? ProgresoVideo { get; set; } // Progress of the video (if applicable)

        // --- Link to the actual submitted file (via Archivo table) ---
        [ForeignKey("Archivo")]
        public Guid? ArchivoId { get; set; } // Nullable if submission is text-based or no file is required
        [ValidateNever]
        public Archivo? Archivo { get; set; } // Navigation property to the submitted file
    }
}
