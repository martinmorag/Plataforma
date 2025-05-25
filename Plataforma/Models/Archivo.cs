using System.ComponentModel.DataAnnotations;

namespace Plataforma.Models
{
    public class Archivo
    {
        [Key]
        public Guid ArchivoId { get; set; } // Unique identifier for the file

        [Required]
        [StringLength(2048)] // Max length for a URL
        public string ArchivoUrl { get; set; } // URL to the uploaded file in Vercel Blob Storage

        [Required]
        [StringLength(255)] // A reasonable max length for file name
        public string FileName { get; set; } // Original file name

        [Required]
        [StringLength(50)] // e.g., "video/mp4", "application/pdf"
        public string ContentType { get; set; } // MIME type of the file

        public long SizeInBytes { get; set; } // File size for better management (optional but recommended)
        public TimeSpan? DuracionVideo { get; set; }

        private DateTime _fechaSubida;
        public DateTime FechaSubida // Date and time the file was uploaded
        {
            get => _fechaSubida;
            set
            {
                if (value.Kind != DateTimeKind.Utc)
                {
                    _fechaSubida = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                }
                else
                {
                    _fechaSubida = value;
                }
            }
        }
    }
}
