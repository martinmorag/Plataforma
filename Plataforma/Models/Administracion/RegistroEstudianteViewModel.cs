using System.ComponentModel.DataAnnotations;

namespace Plataforma.Models.Administracion
{
    public class RegistroEstudianteViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido.")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Opcional: Campo para confirmar la contraseña
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }
    }
}
