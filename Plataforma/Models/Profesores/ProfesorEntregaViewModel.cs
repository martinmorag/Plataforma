namespace Plataforma.Models.Profesores
{
    public class ProfesorEntregaViewModel
    {
        public Guid EntregaId { get; set; }
        public string EstudianteNombre { get; set; }
        public Entrega.EstadoEntrega? Estado { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string? ComentariosEstudiante { get; set; }
        public string? ComentariosProfesor { get; set; }
        public string? ArchivoUrl { get; set; } // The public URL to download
        public string? ArchivoNombreOriginal { get; set; }
        public EvaluarEntregaDto? Evaluar { get; set; }
    }
}
