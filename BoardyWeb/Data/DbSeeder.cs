using BoardyWeb.Autorizacion;
using BoardyWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoardyWeb.Data
{
    public static class DbSeeder
    {
        private const string EmailAdministrador = "admin@boardy.local";
        private const string PasswordAdministrador = "Admin123$";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await context.Database.MigrateAsync();

            await SeedRolesIdentityAsync(scope.ServiceProvider);
            await SeedEstadosTareaAsync(context);
            await SeedRolesMiembroAsync(context);
            var administrador = await SeedAdministradorAsync(scope.ServiceProvider);
            await SeedTableroDemoAsync(context, administrador);
            await SeedEtiquetasTableroDemoAsync(context);
        }

        private static async Task SeedRolesIdentityAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var nombre in new[] { "Administrador", "Operador" })
            {
                if (!await roleManager.RoleExistsAsync(nombre))
                {
                    await roleManager.CreateAsync(new IdentityRole(nombre));
                }
            }
        }

        private static async Task SeedEstadosTareaAsync(AppDbContext context)
        {
            if (await context.EstadoTareas.AnyAsync())
            {
                return;
            }

            context.EstadoTareas.AddRange(
                new EstadoTarea { Nombre = "Pendiente", Descripcion = "La tarea todavía no comenzó." },
                new EstadoTarea { Nombre = "En progreso", Descripcion = "La tarea está siendo trabajada." },
                new EstadoTarea { Nombre = "En revisión", Descripcion = "La tarea espera revisión." },
                new EstadoTarea { Nombre = "Finalizada", Descripcion = "La tarea fue completada." });

            await context.SaveChangesAsync();
        }

        private static async Task SeedEtiquetasTableroDemoAsync(AppDbContext context)
        {
            var tablero = await context.Tableros.OrderBy(t => t.TableroId).FirstOrDefaultAsync();

            if (tablero is null || await context.Etiquetas.AnyAsync(e => e.TableroId == tablero.TableroId))
            {
                return;
            }

            context.Etiquetas.AddRange(
                new Etiqueta { TableroId = tablero.TableroId, Nombre = "Bug", Descripcion = "Error a corregir.", Color = "#DC3545" },
                new Etiqueta { TableroId = tablero.TableroId, Nombre = "Mejora", Descripcion = "Mejora funcional.", Color = "#0D6EFD" },
                new Etiqueta { TableroId = tablero.TableroId, Nombre = "Urgente", Descripcion = "Requiere atención inmediata.", Color = "#FD7E14" },
                new Etiqueta { TableroId = tablero.TableroId, Nombre = "Documentación", Descripcion = "Trabajo de documentación.", Color = "#6C757D" });

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesMiembroAsync(AppDbContext context)
        {
            var rolesDelSistema = new Dictionary<string, string>
            {
                [RolesTablero.Propietario] = "Dueño del tablero. Administra todo, lo elimina y puede transferir la propiedad.",
                [RolesTablero.Administrador] = "Crea y administra listas, tareas, etiquetas, miembros e invitaciones del tablero.",
                [RolesTablero.Operario] = "Comenta y, en las tareas que tiene asignadas, cambia estado y etiquetas y las mueve.",
                [RolesTablero.Lector] = "Solo consulta el tablero."
            };

            var existentes = await context.RolMiembros.ToListAsync();

            // Se crean los que faltan y se mantienen al día las descripciones de los roles del sistema.
            foreach (var (nombre, descripcion) in rolesDelSistema)
            {
                var rol = existentes.FirstOrDefault(r => r.Nombre == nombre);

                if (rol is null)
                {
                    context.RolMiembros.Add(new RolMiembro { Nombre = nombre, Descripcion = descripcion });
                }
                else
                {
                    rol.Descripcion = descripcion;
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task<Usuario> SeedAdministradorAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<Usuario>>();
            var administrador = await userManager.FindByEmailAsync(EmailAdministrador);

            if (administrador is null)
            {
                administrador = new Usuario
                {
                    UserName = EmailAdministrador,
                    Email = EmailAdministrador,
                    EmailConfirmed = true,
                    Nombre = "Administrador",
                    Apellido = "Boardy",
                    FechaRegistro = DateTime.UtcNow,
                    Estado = true
                };

                var resultado = await userManager.CreateAsync(administrador, PasswordAdministrador);
                if (!resultado.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"No se pudo crear el usuario administrador: {string.Join(" ", resultado.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(administrador, "Administrador"))
            {
                await userManager.AddToRoleAsync(administrador, "Administrador");
            }

            return administrador;
        }

        private static async Task SeedTableroDemoAsync(AppDbContext context, Usuario administrador)
        {
            if (await context.Tableros.AnyAsync())
            {
                return;
            }

            var estados = await context.EstadoTareas.OrderBy(e => e.EstadoTareaId).ToListAsync();
            var rolPropietario = await context.RolMiembros.FirstAsync(r => r.Nombre == RolesTablero.Propietario);

            var tablero = new Tablero
            {
                Nombre = "Tablero de ejemplo",
                Descripcion = "Tablero de demostración con listas y tareas de muestra.",
                PropietarioId = administrador.Id
            };

            tablero.Miembros.Add(new TableroMiembro
            {
                UsuarioId = administrador.Id,
                FechaAsignacion = DateTime.UtcNow,
                Roles = new List<MiembrosPorRol>
                {
                    new MiembrosPorRol
                    {
                        RolMiembroId = rolPropietario.RolMiembroId,
                        FechaAsignacion = DateTime.UtcNow
                    }
                }
            });

            var listaPendiente = new ListaTarea { Nombre = "Pendiente", Orden = 1 };
            var listaEnProgreso = new ListaTarea { Nombre = "En progreso", Orden = 2 };
            var listaFinalizada = new ListaTarea { Nombre = "Finalizada", Orden = 3 };

            listaPendiente.Tareas.Add(new Tarea
            {
                Titulo = "Definir el alcance del tablero",
                Descripcion = "Acordar con el equipo las columnas y el flujo de trabajo.",
                Prioridad = PrioridadTarea.Alta,
                Orden = 1,
                EstadoTareaId = estados[0].EstadoTareaId
            });

            listaPendiente.Tareas.Add(new Tarea
            {
                Titulo = "Cargar las etiquetas del proyecto",
                Descripcion = "Revisar el catálogo de etiquetas disponibles.",
                Prioridad = PrioridadTarea.Media,
                Orden = 2,
                EstadoTareaId = estados[0].EstadoTareaId
            });

            listaEnProgreso.Tareas.Add(new Tarea
            {
                Titulo = "Implementar el tablero kanban",
                Descripcion = "Mostrar listas y tarjetas con su estado actual.",
                Prioridad = PrioridadTarea.Alta,
                Orden = 1,
                FechaVencimiento = DateTime.UtcNow.AddDays(7),
                EstadoTareaId = estados[1].EstadoTareaId
            });

            listaFinalizada.Tareas.Add(new Tarea
            {
                Titulo = "Configurar Identity",
                Descripcion = "Registro, inicio de sesión y roles de usuario.",
                Prioridad = PrioridadTarea.Alta,
                Orden = 1,
                EstadoTareaId = estados[3].EstadoTareaId
            });

            tablero.ListasTarea.Add(listaPendiente);
            tablero.ListasTarea.Add(listaEnProgreso);
            tablero.ListasTarea.Add(listaFinalizada);

            context.Tableros.Add(tablero);
            await context.SaveChangesAsync();
        }
    }
}
