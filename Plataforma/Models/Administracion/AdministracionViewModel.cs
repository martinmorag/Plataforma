namespace Plataforma.Models.Administracion
{
    public class AdministracionViewModel
    {
        public RegistroEstudianteViewModel? RegistroEstudiante { get; set; }
        public List<Plataforma.Models.Estudiante>? ListaEstudiantes { get; set; }


        // Editar estudiante
        public Plataforma.Models.Estudiante? EstudianteAEditar { get; set; }


        // Editar profesor
        public Plataforma.Models.Profesor? ProfesorAEditar { get; set; }



        public RegistroProfesorViewModel? RegistroProfesor { get; set; }
        public List<Profesor>? ListaProfesores { get; set; }
    }
}
