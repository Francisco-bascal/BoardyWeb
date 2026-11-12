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

`Usuario` · `Tablero` · `ListaTarea` · `Tarea` · `EstadoTarea` y `Etiqueta` (por tablero) · `Comentario` · `InvitacionTablero` · `TableroMiembro` + `MiembrosPorRol` + `RolMiembro` (cada miembro tiene un único rol por tablero, garantizado con un índice único) · `TareaMiembro` (encargados) · `TareaEtiqueta`.

Enums guardados como texto: `PrioridadTarea` (Baja, Media, Alta, Urgente) y `EstadoInvitacion`.

## Seguridad y permisos

Hay dos niveles de roles:

- **Globales (Identity)**: `Administrador` gestiona usuarios y su rol global, y ve los logs de error. Los 4 roles de miembro son fijos.
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

## Esquemas, panel y plataforma

- **Esquema del tablero** (`Tablero.EsKanban`, se elige al crearlo): en Kanban cada lista tiene un estado obligatorio (`ListaTarea.EstadoTareaId`), mover una tarea cambia su estado y cambiar el estado la mueve a la lista correspondiente. En esquema libre las listas no tienen estado.
- **Mi panel**: tareas abiertas asignadas al usuario, por tablero; el clic abre la tarea en su tablero.
- **Estados de tarea por tablero**: cada tablero nace con Pendiente, En progreso y Finalizada, que son inmutables. Propietario y Administrador agregan estados propios, que solo se eliminan si no los usa ninguna tarea ni lista. En Kanban, si no queda un estado sin lista, no se puede crear otra lista.
- **Plataforma** (rol global Administrador): usuarios (activar/desactivar, rol Administrador) y logs de error.
- **Auditoría del tablero** (Propietario y Administrador): lista cronológica con filtro por usuario.

## Auditoría, errores, fotos y reportes

- **Auditoría**: cada operación llama a `IAuditoriaService.Registrar`, que agrega la fila sin guardar; el `SaveChangesAsync` de la operación persiste ambas en una transacción.
- **Errores**: `RegistroErroresMiddleware` guarda las excepciones no controladas, visibles en Plataforma → Logs.
- **Foto de perfil**: en `wwwroot/uploads` (no versionada), hasta 2 MB, JPG/PNG/WebP validados por contenido.
- **PDF** (QuestPDF, licencia Community): reporte del tablero, mis tareas y auditoría filtrada.

## Eliminar la cuenta propia

En Mi cuenta → Perfil, con contraseña. Se rechaza si el usuario es propietario de un tablero o el único administrador global. La cuenta se anonimiza: sus comentarios y su auditoría quedan como "Usuario eliminado" y el correo queda libre.

## Correo y asistente

- **Correo**: `CorreoSmtpService` implementa `IEmailSender` (confirmación, recuperación de contraseña y aviso de invitación). El aviso de invitación se envía en segundo plano: un fallo no anula la invitación y solo queda en el log de la aplicación.
- **Asistente**: burbuja de chat con Gemini por REST. Solo explica el uso y resume las tareas pendientes del usuario, con opción de PDF; no tiene herramientas, así que no puede modificar datos. Límite: 20 preguntas por minuto por usuario.
- **Configuración**: secciones `Smtp` y `Gemini` en `appsettings.Development.json` (no versionado). En el hosting, variables de entorno como `Smtp__Clave` y `Gemini__ApiKey`.

## Puesta en marcha

1. Crear `BoardyWeb/appsettings.Development.json` (no se versiona, está en `.gitignore`) con `ConnectionStrings:DefaultConnection` y las secciones `Smtp` y `Gemini`.
2. Ejecutar `dotnet run` dentro de `BoardyWeb/`: aplica las migraciones y siembra roles, un usuario administrador y un tablero de demostración.
3. Usuario semilla: `admin@boardy.local`.
