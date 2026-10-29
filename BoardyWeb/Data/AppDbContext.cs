using BoardyWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Security.Claims;

namespace BoardyWeb.Data
{
    public class AppDbContext : IdentityDbContext<Usuario>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Comentario> Comentarios { get; set; } = null!;
        public DbSet<Tablero> Tableros { get; set; } = null!;
        public DbSet<TableroMiembro> TableroMiembros { get; set; } = null!;
        public DbSet<RolMiembro> RolMiembros { get; set; } = null!;
        public DbSet<MiembrosPorRol> MiembrosPorRol { get; set; } = null!;
        public DbSet<InvitacionTablero> InvitacionTableros { get; set; } = null!;
        public DbSet<ListaTarea> ListaTareas { get; set; } = null!;
        public DbSet<Tarea> Tareas { get; set; } = null!;
        public DbSet<EstadoTarea> EstadoTareas { get; set; } = null!;
        public DbSet<TareaMiembro> TareaMiembros { get; set; } = null!;
        public DbSet<Etiqueta> Etiquetas { get; set; } = null!;
        public DbSet<TareaEtiqueta> TareaEtiquetas { get; set; } = null!;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplyAuditInformation();
            return base.SaveChanges();
        }

        private void ApplyAuditInformation()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.FechaCreacion = DateTime.UtcNow;
                    entry.Entity.UsuarioCreacionId = userId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.FechaModificacion = DateTime.UtcNow;
                    entry.Entity.UsuarioModificacionId = userId;

                    entry.Property(nameof(AuditableEntity.FechaCreacion)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.UsuarioCreacionId)).IsModified = false;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Comentario>(entity =>
            {
                entity.HasKey(c => c.ComentarioId);
                entity.HasOne(c => c.Usuario).WithMany(u => u.Comentarios).HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.Tarea).WithMany(t => t.Comentarios).HasForeignKey(c => c.TareaId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Tablero>(entity =>
            {
                entity.HasKey(t => t.TableroId);
                entity.HasOne(t => t.Propietario).WithMany(u => u.TablerosPropios).HasForeignKey(t => t.PropietarioId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TableroMiembro>(entity =>
            {
                entity.HasKey(tm => tm.TableroMiembroId);
                entity.HasIndex(tm => new { tm.TableroId, tm.UsuarioId }).IsUnique();
                entity.HasOne(tm => tm.Tablero).WithMany(t => t.Miembros).HasForeignKey(tm => tm.TableroId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(tm => tm.Usuario).WithMany(u => u.TablerosMiembro).HasForeignKey(tm => tm.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RolMiembro>(entity =>
            {
                entity.HasKey(r => r.RolMiembroId);
                entity.HasIndex(r => r.Nombre).IsUnique();
            });

            modelBuilder.Entity<MiembrosPorRol>(entity =>
            {
                entity.HasKey(m => m.MiembroPorRolId);
                // Un miembro tiene un único rol en el tablero.
                entity.HasIndex(m => m.TableroMiembroId).IsUnique();
                entity.HasOne(m => m.TableroMiembro).WithMany(tm => tm.Roles).HasForeignKey(m => m.TableroMiembroId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(m => m.RolMiembro).WithMany(r => r.Miembros).HasForeignKey(m => m.RolMiembroId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<InvitacionTablero>(entity =>
            {
                entity.HasKey(i => i.InvitacionTableroId);
                entity.Property(i => i.Estado).HasConversion<string>().HasMaxLength(50);
                entity.HasOne(i => i.UsuarioInvitado).WithMany(u => u.InvitacionesRecibidas).HasForeignKey(i => i.UsuarioInvitadoId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(i => i.UsuarioInvitador).WithMany(u => u.InvitacionesEnviadas).HasForeignKey(i => i.UsuarioInvitadorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(i => i.Tablero).WithMany(t => t.Invitaciones).HasForeignKey(i => i.TableroId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ListaTarea>(entity =>
            {
                entity.HasKey(l => l.ListaTareaId);
                entity.HasOne(l => l.Tablero).WithMany(t => t.ListasTarea).HasForeignKey(l => l.TableroId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Tarea>(entity =>
            {
                entity.HasKey(t => t.TareaId);
                entity.Property(t => t.Prioridad).HasConversion<string>().HasMaxLength(50);
                entity.HasOne(t => t.ListaTarea).WithMany(l => l.Tareas).HasForeignKey(t => t.ListaTareaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.EstadoTarea).WithMany(e => e.Tareas).HasForeignKey(t => t.EstadoTareaId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EstadoTarea>(entity =>
            {
                entity.HasKey(e => e.EstadoTareaId);
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            modelBuilder.Entity<TareaMiembro>(entity =>
            {
                entity.HasKey(tm => tm.TareaMiembroId);
                entity.HasIndex(tm => new { tm.TareaId, tm.TableroMiembroId }).IsUnique();
                entity.HasOne(tm => tm.Tarea).WithMany(t => t.MiembrosAsignados).HasForeignKey(tm => tm.TareaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(tm => tm.TableroMiembro).WithMany(tm => tm.TareasAsignadas).HasForeignKey(tm => tm.TableroMiembroId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Etiqueta>(entity =>
            {
                entity.HasKey(e => e.EtiquetaId);
                entity.HasIndex(e => new { e.TableroId, e.Nombre }).IsUnique();
                entity.HasOne(e => e.Tablero).WithMany(t => t.Etiquetas).HasForeignKey(e => e.TableroId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TareaEtiqueta>(entity =>
            {
                entity.HasKey(te => te.TareaEtiquetaId);
                entity.HasIndex(te => new { te.TareaId, te.EtiquetaId }).IsUnique();
                entity.HasOne(te => te.Tarea).WithMany(t => t.Etiquetas).HasForeignKey(te => te.TareaId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(te => te.Etiqueta).WithMany(e => e.Tareas).HasForeignKey(te => te.EtiquetaId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
