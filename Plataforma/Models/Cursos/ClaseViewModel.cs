namespace Plataforma.Models.Cursos
{
    public class ClaseViewModel
    {
        public Guid ClaseId { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int Order { get; set; }
        public bool HasTareas { get; set; }
        public bool IsCompleted { get; set; }
        public List<TareaViewModel> Tareas { get; set; } = new List<TareaViewModel>(); // NEW: List of tasks for this class
    }
}
