using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Plataforma.Models;
using System.Reflection;
using System.Security.Claims;

namespace Plataforma.Data
{
    public class PlataformaContext : IdentityDbContext<UsuarioIdentidad, IdentityRole<Guid>, Guid>
    {
        public PlataformaContext(DbContextOptions<PlataformaContext> options)
            : base(options)
        {
        }
        public DbSet<Estudiante> estudiantes { get; set; }
        public DbSet<Profesor> profesores { get; set; }
        public DbSet<Administrador> administradores { get; set; }
        public DbSet<Curso> cursos { get; set; }
        public DbSet<Modulo> modulos { get; set; }
        public DbSet<Clase> clases { get; set; }
        public DbSet<Tarea> tareas { get; set; }
        public DbSet<Entrega> entregas { get; set; }
        public DbSet<Archivo> archivos { get; set; }
        public DbSet<CursoEstudiante> CursoEstudiantes { get; set; }  
        public DbSet<CursoProfesor> CursoProfesores { get; set; } 
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UsuarioIdentidad>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<UsuarioIdentidad>("UsuarioIdentidad")
            .HasValue<Administrador>("Administrador")
            .HasValue<Estudiante>("Estudiante")
            .HasValue<Profesor>("Profesor");

            // Adds estudiante discriminator when adding estudiantes
            builder.Entity<UsuarioIdentidad>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<UsuarioIdentidad>("UsuarioIdentidad") // Valor para usuarios genéricos
                .HasValue<Estudiante>("Estudiante");
            // Adds profesor discriminator when adding profesores
            builder.Entity<UsuarioIdentidad>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<UsuarioIdentidad>("UsuarioIdentidad") // Valor para usuarios genéricos
                .HasValue<Profesor>("Profesor");

            builder.Entity<CursoEstudiante>()
                .HasKey(ce => new { ce.CursoId, ce.EstudianteId }); // Composite key

            builder.Entity<CursoEstudiante>()
                .HasOne(ce => ce.Curso)
                .WithMany(c => c.CursoEstudiantes)
                .HasForeignKey(ce => ce.CursoId);

            builder.Entity<CursoEstudiante>()
                .HasOne(ce => ce.Estudiante)
                .WithMany(s => s.CursoEstudiantes)
                .HasForeignKey(ce => ce.EstudianteId);

            // Configure the many-to-many relationship between Curso and Profesor
            builder.Entity<CursoProfesor>()
                .HasKey(cp => new { cp.CursoId, cp.ProfesorId }); // Composite key

            builder.Entity<CursoProfesor>()
                .HasOne(cp => cp.Curso)
                .WithMany(c => c.CursoProfesores)
                .HasForeignKey(cp => cp.CursoId);

            builder.Entity<CursoProfesor>()
                .HasOne(cp => cp.Profesor)
                .WithMany(p => p.CursoProfesores)
                .HasForeignKey(cp => cp.ProfesorId);


            builder.Entity<Modulo>()
                .HasOne(m => m.Curso)
                .WithMany(c => c.Modulos)
                .HasForeignKey(m => m.CursoId);

            builder.Entity<Clase>()
                .HasOne(cl => cl.Modulo)
                .WithMany(m => m.Clases)
                .HasForeignKey(cl => cl.ModuloId);

            // **CRITICAL FIX:** Only ONE configuration block for Tarea to Clase
            builder.Entity<Tarea>()
                .HasOne(a => a.Clase) // A Tarea belongs to one Clase
                .WithMany(c => c.Tareas) // A Clase has many Tareas, navigated via the 'Tareas' collection
                .HasForeignKey(a => a.ClaseId) // ClaseId is the foreign key in Tarea
                .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion of Clase if Tareas exist

            // --- Entrega (Submission) Relationships ---
            builder.Entity<Entrega>()
                .HasOne(e => e.Tarea) // A submission is for one assignment
                .WithMany()               // An assignment can have many submissions
                .HasForeignKey(e => e.TareaId)
                .OnDelete(DeleteBehavior.Cascade); // If an assignment is deleted, its submissions are also deleted

            builder.Entity<Entrega>()
                .HasOne(e => e.Estudiante) // A submission is made by one student
                .WithMany()                // A student can make many submissions
                .HasForeignKey(e => e.EstudianteId)
                .OnDelete(DeleteBehavior.Cascade); // If a student is deleted, their submissions are also deleted

            builder.Entity<Entrega>()
                .HasOne(e => e.Archivo)   // A submission can optionally have one file
                .WithOne()                // A file can belong to only one submission in this context
                .HasForeignKey<Entrega>(e => e.ArchivoId) // Foreign key is on Entrega
                .IsRequired(false)        // Make it optional (nullable ArchivoId)
                .OnDelete(DeleteBehavior.SetNull); // If an Archivo is deleted, set ArchivoId in Entrega to null

            // --- Archivo (File) Relationships ---
            // If Tarea can have an associated file directly:
            builder.Entity<Tarea>()
                .HasOne(a => a.Archivo) // An assignment can optionally have one file
                .WithOne()
                .HasForeignKey<Tarea>(a => a.ArchivoId) // Assuming you add ArchivoId to Tarea model
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
