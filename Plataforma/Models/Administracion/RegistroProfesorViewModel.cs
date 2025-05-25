using System.ComponentModel.DataAnnotations;

namespace Plataforma.Models.Administracion
{
    public class RegistroProfesorViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmación de la contraseña es requerida")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden")]
        public string ConfirmPassword { get; set; }
    }
}
