namespace Plataforma.Models.Cursos
{
    public class VideoProgressDto
    {
        public Guid TareaId { get; set; }
        public double CurrentTime { get; set; }
        public double VideoDuration { get; set; }
    }
}
