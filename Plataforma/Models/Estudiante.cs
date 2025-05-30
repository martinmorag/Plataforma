namespace Plataforma.Models
{
    public class Estudiante : UsuarioIdentidad
    {
        public ICollection<ProfesorCursoDto> CursoEstudiantes { get; set; } = new List<ProfesorCursoDto>();
    }
}
