namespace Plataforma.Models.Profesores
{
    public class ProfesorCursosViewModel
    {
        public List<Curso> Cursos { get; set; }

        public Guid? CursoId { get; set; }
        public Guid? ModuloId { get; set; }
        public string? Nombre { get; set; }
    }
}
