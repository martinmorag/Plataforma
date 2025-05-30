namespace Plataforma.Models.Cursos
{
    public class SubmissionFormViewModel
    {
        public Guid TareaId { get; set; }
        public IFormFile? SubmittedFile { get; set; } // Use IFormFile for file uploads
        public string? Comments { get; set; } // Optional comments
    }
}
