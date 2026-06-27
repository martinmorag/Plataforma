namespace Plataforma.Models.Cursos
{
    public class TareaViewModel
    {
        public Guid TareaId { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string TipoContenido { get; set; }
        public string? GrabacionUrl { get; set; }
        public DateTime? FechaReunion { get; set; }
        public DateTime? FechaLimite { get; set; }
        public bool IsSubmittedApproved { get; set; } // Indicates if this task has an approved submission
        public string? SubmissionStatusText { get; set; } // e.g., "Aprobado", "En Revisión", "Pendiente"
        public bool HasFileRequirement { get; set; } // Does this task require a file submission?
        public string? AccederLink { get; set; } // Link to submit/view task
    }
}
