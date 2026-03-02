using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Plataforma.Models.Administracion
{
    public class CrearCursoViewModel
    {
        [Required]
        [StringLength(200)]
        public string Nombre { get; set; }

        public bool Disponible { get; set; } = true;

        [ValidateNever]
        public string? ImageUrl { get; set; }
    }
}
