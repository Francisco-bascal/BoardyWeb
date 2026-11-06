using BoardyWeb.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BoardyWeb.Services
{
    /// <summary>
    /// Genera los reportes PDF con QuestPDF.
    /// </summary>
    public class ReporteService : IReporteService
    {
        // PDF del tablero: una tabla de tareas por lista.
        public byte[] GenerarTablero(TableroDetalleDto tablero, IReadOnlyList<ListaTareaDto> listas, IReadOnlyList<TareaDto> tareas)
        {
            return Generar($"Tablero: {tablero.Nombre}", $"Propietario: {tablero.PropietarioNombre}", contenido =>
            {
                foreach (var lista in listas)
                {
                    contenido.Item().PaddingTop(12).Text(lista.Nombre).FontSize(13).SemiBold();
                    TablaTareas(contenido, tareas.Where(t => t.ListaTareaId == lista.ListaTareaId).OrderBy(t => t.Orden));
                }
            });
        }

        // PDF de las tareas asignadas al usuario, agrupadas por tablero.
        public byte[] GenerarMisTareas(string nombreUsuario, IReadOnlyList<TareaDto> tareas)
        {
            return Generar("Mis tareas", nombreUsuario, contenido =>
            {
                foreach (var grupo in tareas.GroupBy(t => t.TableroNombre))
                {
                    contenido.Item().PaddingTop(12).Text(grupo.Key).FontSize(13).SemiBold();
                    TablaTareas(contenido, grupo);
                }
            });
        }

        // PDF de las acciones de auditoría recibidas.
        public byte[] GenerarAuditoria(string nombreTablero, IReadOnlyList<RegistroAuditoriaDto> registros)
        {
            return Generar($"Auditoría: {nombreTablero}", $"{registros.Count} acciones", contenido =>
            {
                Tabla(contenido, new[] { "Fecha (UTC)", "Usuario", "Acción" }, new float[] { 2, 2, 5 },
                    registros.Select(r => new[] { r.Fecha.ToString("dd/MM/yyyy HH:mm"), r.UsuarioNombre, r.Accion }));
            });
        }

        // Arma las columnas de una tabla de tareas.
        private static void TablaTareas(ColumnDescriptor contenido, IEnumerable<TareaDto> tareas)
        {
            Tabla(contenido, new[] { "Tarea", "Estado", "Prioridad", "Vence", "Asignados" }, new float[] { 4, 2, 1.5f, 1.5f, 3 },
                tareas.Select(t => new[]
                {
                    t.Titulo,
                    t.EstadoNombre,
                    t.Prioridad.ToString(),
                    t.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "-",
                    string.Join(", ", t.Miembros.Select(m => m.NombreCompleto))
                }));
        }

        // Dibuja una tabla genérica con encabezados, anchos y filas.
        private static void Tabla(ColumnDescriptor contenido, string[] encabezados, float[] anchos, IEnumerable<string[]> filas)
        {
            contenido.Item().PaddingTop(4).Table(tabla =>
            {
                tabla.ColumnsDefinition(columnas =>
                {
                    foreach (var ancho in anchos)
                    {
                        columnas.RelativeColumn(ancho);
                    }
                });

                tabla.Header(encabezado =>
                {
                    foreach (var texto in encabezados)
                    {
                        encabezado.Cell().BorderBottom(1).PaddingVertical(3).Text(texto).SemiBold();
                    }
                });

                foreach (var celda in filas.SelectMany(fila => fila))
                {
                    tabla.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingRight(4).Text(celda);
                }
            });
        }

        // Plantilla común: página A4, encabezado, contenido y numeración.
        private static byte[] Generar(string titulo, string subtitulo, Action<ColumnDescriptor> contenido)
        {
            return Document.Create(documento => documento.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(36);
                pagina.DefaultTextStyle(estilo => estilo.FontSize(10));

                pagina.Header().Column(encabezado =>
                {
                    encabezado.Item().Text(titulo).FontSize(18).Bold().FontColor(Colors.Indigo.Darken2);
                    encabezado.Item().Text($"{subtitulo} · Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").FontColor(Colors.Grey.Darken1);
                });

                pagina.Content().Column(columna => contenido(columna));

                pagina.Footer().AlignCenter().Text(texto =>
                {
                    texto.CurrentPageNumber();
                    texto.Span(" / ");
                    texto.TotalPages();
                });
            })).GeneratePdf();
        }
    }
}
