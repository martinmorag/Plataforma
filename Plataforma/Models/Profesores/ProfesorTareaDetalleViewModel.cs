namespace Plataforma.Models.Profesores
{
    public class ProfesorTareaDetalleViewModel
    {
        public Guid TareaId { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }
        public DateTime? FechaReunion { get; set; }

        public string TipoContenido { get; set; }

        public string? ArchivoUrl { get; set; }

        public string? ReunionUrl { get; set; }

        public string ClaseNombre { get; set; }

        public DateTime? FechaLimite { get; set; }
    }
}
