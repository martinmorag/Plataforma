using System.ComponentModel.DataAnnotations;

namespace Plataforma.Models.Administracion 
{
    public class SetPasswordViewModel
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        [MinLength(6, ErrorMessage = "Debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }
    }
}