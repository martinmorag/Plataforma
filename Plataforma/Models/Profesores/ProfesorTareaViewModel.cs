namespace Plataforma.Models.Profesores
{
    public class ProfesorTareaViewModel
    {
        public Guid TareaId { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaLimite { get; set; }
        public string ClaseNombre { get; set; }
        public int TotalEntregas { get; set; }
        public int EntregasPendientes { get; set; }
    }
}
