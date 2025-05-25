namespace Plataforma.Models.Cursos
{
    public class TareaDetailsViewModel
    {
        public Guid TareaId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaLimite { get; set; }
        public string TipoContenido { get; set; } // "Video", "Documento", "Texto", "Cuestionario"
        public string ContenidoUrl { get; set; } // URL for content (e.g., video source, document path)
        public string? TextoContenido { get; set; } // For text-based tasks

        public bool? RequiereArchivo { get; set; } // Does this task require a file submission?

        // Submission Status for the current student
        public bool HasSubmitted { get; set; }
        public string SubmissionStatusText { get; set; } // "Pendiente", "EnRevision", "Aprobado", "Reprobado"
        public bool IsSubmittedApproved { get; set; }
        public string SubmittedFileUrl { get; set; } // URL of the file submitted by the student
        public string SubmissionComentarios { get; set; } // Comments from the professor
        public DateTime? SubmissionFecha { get; set; } // Date of submission
    }
}
