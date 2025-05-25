namespace Plataforma.Models
{
    public class Profesor : UsuarioIdentidad
    {
        public ICollection<CursoProfesor> CursoProfesores { get; set; } = new List<CursoProfesor>();
    }
}
