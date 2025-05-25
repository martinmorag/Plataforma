namespace Plataforma.Models.Cursos
{
    public class ModuloViewModel
    {
        public Guid ModuloId { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int Order { get; set; }
        public bool IsCompleted { get; set; } // New: Indicates if all classes in module are completed

        public List<ClaseViewModel> Clases { get; set; } = new List<ClaseViewModel>();
    }
}
