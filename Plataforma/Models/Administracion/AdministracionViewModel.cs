namespace Plataforma.Models.Administracion
{
    public class AdministracionViewModel
    {
        public RegistroEstudianteViewModel? RegistroEstudiante { get; set; }
        public List<Plataforma.Models.Estudiante>? ListaEstudiantes { get; set; }


        // Editar estudiante
        public Plataforma.Models.Estudiante? EstudianteAEditar { get; set; }

        // Propiedades para la nueva contraseña en la edición
        public string? NuevaPassword { get; set; }

        public string? ConfirmarNuevaPassword { get; set; }


        // Editar profesor
        public Plataforma.Models.Profesor? ProfesorAEditar { get; set; }



        public RegistroProfesorViewModel? RegistroProfesor { get; set; }
        public List<Profesor>? ListaProfesores { get; set; }
    }
}
