namespace Plataforma.Models.Administracion
{
    public class CursoAdminIndexViewModel
    {
        public Guid CursoId { get; set; }
        public string Nombre { get; set; }
        public bool Habilitado { get; set; }
        public string ImageUrl { get; set; }

        public int CantidadEstudiantes { get; set; }

        public List<string> Profesores { get; set; } = new();
        public List<ModuloAdminViewModel> Modulos { get; set; } = new();
    }

    public class ModuloAdminViewModel
    {
        public string Nombre { get; set; }
        public List<string> Clases { get; set; } = new();
    }
}
