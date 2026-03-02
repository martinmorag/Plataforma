namespace Plataforma.Models.Profesores
{
    public class EditarTareaViewModel
    {
        public Guid TareaId { get; set; }
        public Guid ClaseId { get; set; }
        public Guid CursoId { get; set; }

        public string? ClaseNombre { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string TipoEntregaEsperado { get; set; }

        public string? ArchivoNombre { get; set; }
    }
}
