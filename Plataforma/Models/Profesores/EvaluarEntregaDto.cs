using System.Text.Json.Serialization;

namespace Plataforma.Models.Profesores
{
    public class EvaluarEntregaDto
    {
        public Guid EntregaId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Entrega.EstadoEntrega Estado { get; set; }
        public string? ComentariosProfesor { get; set; }
    }
}
