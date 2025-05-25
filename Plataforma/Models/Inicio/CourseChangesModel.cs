namespace Plataforma.Models.Inicio
{
    public class CourseChangesModel
    {
        public List<string> CoursesToRegister { get; set; } = new List<string>();
        public List<string> CoursesToDeregister { get; set; } = new List<string>();
    }
}
