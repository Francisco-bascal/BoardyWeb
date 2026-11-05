# Boardy

Aplicación web de gestión de tareas estilo kanban, pensada para equipos de desarrollo. Cada usuario crea tableros con listas y tareas, invita a su equipo y trabaja con permisos según su rol en cada tablero.

## Stack

| Capa | Tecnología |
|---|---|
| Plataforma | ASP.NET Core 8 (MVC + Razor Pages para Identity) |
| Datos | Entity Framework Core 8 + SQL Server (migraciones automáticas al iniciar) |
| Autenticación | ASP.NET Core Identity (`Usuario : IdentityUser`) con confirmación de cuenta |
| Frontend | Bootstrap 5.3, Bootstrap Icons, jQuery, jQuery Validation/Unobtrusive y Unobtrusive Ajax |
| Arrastrar y soltar | SortableJS |

Las librerías de cliente están en `wwwroot/lib` y se registran en `libman.json`.

## Arquitectura

Flujo en capas: **Controlador → Servicio → EF Core (`AppDbContext`) → SQL Server**.

```
BoardyWeb/
├── Controllers/     Controladores MVC delgados (sin lógica de negocio ni acceso a datos)
├── Services/        Lógica de negocio: una interfaz + implementación por área
├── Models/          Entidades EF Core, enums y ViewModels
├── DTOs/            Contratos de entrada/salida con validaciones
├── Data/            AppDbContext (auditoría automática), DbSeeder, Migrations/
├── Autorizacion/    Políticas y handlers de permisos por tablero y por tarea
├── Comun/           ResultadoOperacion, RespuestaAjax, proveedores de validación, utilidades
├── Views/ · Areas/Identity/   Vistas Razor y páginas de cuenta
└── wwwroot/         CSS, JS propio y librerías
```

Decisiones clave:

- **`ResultadoOperacion` / `ResultadoOperacion<T>`**: los servicios informan éxito, "no existe", "sin permiso" o errores por campo sin depender de HTTP; el controlador los traduce (`ControladorBase.Responder`).
- **DTOs** para todas las entradas y salidas; las entidades no se exponen a las vistas.
- **Auditoría**: `AuditableEntity` registra fecha y usuario de creación/modificación, completados en `SaveChangesAsync`.
- **Todo acceso a datos es asíncrono** y propaga `CancellationToken`.

## Dominio

`Usuario` · `Tablero` · `ListaTarea` · `Tarea` · `EstadoTarea` · `Etiqueta` (por tablero) · `Comentario` · `InvitacionTablero` · `TableroMiembro` + `MiembrosPorRol` + `RolMiembro` (cada miembro tiene un único rol por tablero, garantizado con un índice único) · `TareaMiembro` (encargados) · `TareaEtiqueta`.

Enums guardados como texto: `PrioridadTarea` (Baja, Media, Alta, Urgente) y `EstadoInvitacion`.

## Seguridad y permisos

Hay dos niveles de roles:

- **Globales (Identity)**: `Administrador` accede a los catálogos (estados y roles de miembro).
- **Por tablero**: Propietario, Administrador, Operario y Lector, resueltos con **autorización por políticas** (`AuthorizationHandler`) que consultan la base.

| Acción | Propietario | Administrador | Operario | Lector |
|---|:-:|:-:|:-:|:-:|
| Ver tablero, miembros y etiquetas | ✓ | ✓ | ✓ | ✓ |
| Comentar | ✓ | ✓ | ✓ | – |
| Mover tarea, cambiar estado y etiquetas | cualquiera | cualquiera | solo asignadas | – |
| Crear/editar/eliminar tareas, listas y etiquetas; asignar encargados | ✓ | ✓ | – | – |
| Miembros, roles e invitaciones; editar tablero | ✓ | ✓ | – | – |
| Eliminar tablero; transferir propiedad | ✓ | – | – | – |

Políticas: `VerTablero`, `ColaborarEnTablero`, `AdministrarTablero`, `GestionarTablero` (recurso: id del tablero) y `TrabajarEnTarea` (recurso: id de la tarea). Los controladores autorizan con `IAuthorizationService`; la interfaz solo oculta lo que no corresponde, el servidor es la autoridad. Formularios y acciones POST usan token antiforgery.

## Interacción sin recarga (AJAX)

- Cada acción detecta `X-Requested-With`: en una petición normal devuelve la vista o redirige; por AJAX devuelve una **parcial HTML** (formularios y regiones) o **JSON** `{exito, mensaje}`.
- `wwwroot/js/ajax-crud.js`: modales Bootstrap para alta/edición, acciones con confirmación (`data-accion-url`) y refresco automático de las regiones de la página (`data-region-url`).
- Validación en cliente (jQuery Unobtrusive) y en servidor, mostrada dentro del modal.
- `site.js` centraliza avisos (`notificar`) y confirmaciones (`confirmar`), único punto a cambiar si se incorpora SweetAlert2.
- `kanban-dnd.js`: arrastre de tarjetas y columnas con cola de peticiones, interfaz optimista y resincronización cuando algo falla.

## Interfaz

- Tema **diurno/nocturno** con `data-bs-theme`, preferencia guardada en `localStorage` (sigue al sistema si no se eligió).
- Diseño propio sobre Bootstrap en `wwwroot/css/site.css`: acento índigo, efectos de hover, componentes reutilizables y `prefers-reduced-motion`.
- Kanban adaptable: 1–2 listas ocupan mitades, 3 tercios, 4 cuartos; con más de 4 aparece scroll horizontal.
- Panel de tableros en grilla (1, 2 o 3 por fila). Chip de vencimiento con semáforo: vencida (negro), hoy (rojo), hasta 3 días (amarillo), más de 3 días (verde).
- Textos, validaciones y mensajes de Identity en español (cultura `es-AR`).

## Puesta en marcha

1. Copiar `BoardyWeb/appsettings.example.json` como `appsettings.json` (no se versiona, está en `.gitignore`) y configurar la cadena de conexión `DefaultConnection`.
2. Ejecutar `dotnet run` dentro de `BoardyWeb/`: aplica las migraciones y siembra roles, estados de tarea, un usuario administrador y un tablero de demostración.
3. Usuario semilla: `admin@boardy.local` (la contraseña está definida en `Data/DbSeeder.cs`; conviene moverla a configuración antes de publicar).

## Pendiente

Adjuntos de archivos, SweetAlert2, envío real de correos de confirmación, pruebas automatizadas y mover credenciales y cadena de conexión a configuración segura.