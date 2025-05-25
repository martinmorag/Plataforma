namespace Plataforma.Models.Cursos
{
    public class CursoClasesViewModel
    {
        public Guid CursoId { get; set; }
        public string? CursoNombre { get; set; }
        public string? CursoDescripcion { get; set; }
        public string? Nivel { get; set; }
        public string? InstructorNombre { get; set; }
        public int TotalClases { get; set; } // Added for progress calculation
        public int CompletedClases { get; set; } // Added for progress calculation

        public List<ModuloViewModel> Modulos { get; set; } = new List<ModuloViewModel>();
    }
}
