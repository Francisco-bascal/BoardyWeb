# Boardy

Aplicación web de tableros de tareas para equipos de desarrollo: cada usuario crea tableros, invita a su equipo y trabaja con permisos según su rol en cada tablero.

## Funcionalidades

- **Tableros** en esquema Kanban (cada lista tiene un estado) o libre, con arrastrar y soltar.
- **Tareas** con prioridad, vencimiento, etiquetas, encargados y comentarios.
- **Roles por tablero:** Propietario, Administrador, Operario y Lector, con transferencia de propiedad.
- **Invitaciones** con aviso por correo.
- **Mi panel** con las tareas abiertas asignadas al usuario.
- **Auditoría** por tablero, con filtro por usuario.
- **Reportes PDF** del tablero, de mis tareas y de la auditoría.
- **Asistente con Gemini:** explica el uso y resume tareas; no puede modificar datos.
- **Cuenta:** confirmación por correo, recuperación de contraseña, foto de perfil y eliminación de la cuenta propia.
- **Estados de tarea por tablero:** 3 base inmutables y estados propios.
- **Plataforma** para administradores: usuarios y logs de error.
- Tema diurno y nocturno.

## Tecnologías

ASP.NET Core 8 (MVC y Razor Pages), Entity Framework Core con SQL Server, ASP.NET Core Identity, Bootstrap 5, jQuery, SortableJS y QuestPDF.

Arquitectura en capas: Controlador → Servicio → EF Core → SQL Server.

## Puesta en marcha

1. Crear `BoardyWeb/appsettings.Development.json` (Git lo ignora) con las claves de la tabla siguiente, en formato JSON (`Smtp:Clave`, `Gemini:ApiKey`, etc.).
2. Dentro de `BoardyWeb/`, ejecutar `dotnet run`. Aplica las migraciones y crea roles, un usuario administrador y un tablero de demostración.

## Configuración

| Clave | Uso |
|---|---|
| `ConnectionStrings:DefaultConnection` | Base de datos |
| `Smtp:Servidor`, `Smtp:Puerto`, `Smtp:Usuario`, `Smtp:Clave`, `Smtp:Remitente` | Envío de correos |
| `Gemini:ApiKey` | Asistente |
| `Gemini:Modelo` | Opcional: reemplaza el modelo por defecto |

En el hosting, los secretos van en variables de entorno, con `__` en lugar de `:` (por ejemplo, `Smtp__Clave` y `Gemini__ApiKey`). Las credenciales nunca se versionan.

## Credenciales

Solicitadas por el docente como requisito de aprobación.

| Usuario | Nombre de usuario | Contraseña |
|---|---|---|
| Administrador | `admin@boardy.local` | `Admin123$` |
| Usuario de prueba | `prueba@gmail.com` | `Prueba123$` |
| Otro usuario de prueba | `desarrollo@gmail.com` | `Desarrollo123$` |

## Documentación

El detalle de arquitectura, permisos y decisiones está en [docs/Boardy.md](docs/Boardy.md).
