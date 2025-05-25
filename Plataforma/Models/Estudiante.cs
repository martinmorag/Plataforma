namespace Plataforma.Models
{
    public class Estudiante : UsuarioIdentidad
    {
        public ICollection<CursoEstudiante> CursoEstudiantes { get; set; } = new List<CursoEstudiante>();
    }
}
