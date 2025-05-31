namespace Plataforma.Models.Administracion
{
    public class SeleccionCursosProfesor
    {
        public Guid ProfesorId { get; set; }
        public List<Curso> Cursos { get; set; } // All available courses
        public List<CursoProfesor> CursoProfesor { get; set; }
        public List<Guid> CursosSeleccionadosParaInscripcion { get; set; } = new List<Guid>();
        public List<Guid> CursosSeleccionadosParaDesinscripcion { get; set; } = new List<Guid>();
    }
}
