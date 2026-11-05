using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardyWeb.Migrations
{
    /// <inheritdoc />
    public partial class EsquemaTableroYRegistros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsKanban",
                table: "Tableros",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoTareaId",
                table: "ListaTareas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RegistrosAuditoria",
                columns: table => new
                {
                    RegistroAuditoriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Accion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TableroId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAuditoria", x => x.RegistroAuditoriaId);
                    table.ForeignKey(
                        name: "FK_RegistrosAuditoria_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosAuditoria_Tableros_TableroId",
                        column: x => x.TableroId,
                        principalTable: "Tableros",
                        principalColumn: "TableroId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosError",
                columns: table => new
                {
                    RegistroErrorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ruta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosError", x => x.RegistroErrorId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListaTareas_EstadoTareaId",
                table: "ListaTareas",
                column: "EstadoTareaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_TableroId_Fecha",
                table: "RegistrosAuditoria",
                columns: new[] { "TableroId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_UsuarioId",
                table: "RegistrosAuditoria",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosError_Fecha",
                table: "RegistrosError",
                column: "Fecha");

            migrationBuilder.AddForeignKey(
                name: "FK_ListaTareas_EstadoTareas_EstadoTareaId",
                table: "ListaTareas",
                column: "EstadoTareaId",
                principalTable: "EstadoTareas",
                principalColumn: "EstadoTareaId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListaTareas_EstadoTareas_EstadoTareaId",
                table: "ListaTareas");

            migrationBuilder.DropTable(
                name: "RegistrosAuditoria");

            migrationBuilder.DropTable(
                name: "RegistrosError");

            migrationBuilder.DropIndex(
                name: "IX_ListaTareas_EstadoTareaId",
                table: "ListaTareas");

            migrationBuilder.DropColumn(
                name: "EsKanban",
                table: "Tableros");

            migrationBuilder.DropColumn(
                name: "EstadoTareaId",
                table: "ListaTareas");
        }
    }
}
