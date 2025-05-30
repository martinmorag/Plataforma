namespace Plataforma.Models.Profesores
{
    public class ProfesorCursoDto
    {
        public Guid CursoId { get; set; }
        public string NombreCurso { get; set; }
        public int TotalClases { get; set; } // Number of classes in this course
        public int TotalTareas { get; set; } // Optional: could show how many tasks are in this class
    }
}
