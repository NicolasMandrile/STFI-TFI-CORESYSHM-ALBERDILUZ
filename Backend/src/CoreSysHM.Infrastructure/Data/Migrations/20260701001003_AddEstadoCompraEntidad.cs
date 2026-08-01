using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoreSysHM.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoCompraEntidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Crear tabla catálogo EstadosCompra
            migrationBuilder.CreateTable(
                name: "EstadosCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosCompra", x => x.Id);
                });

            // 2. Seed de los 3 estados con IDs fijos
            migrationBuilder.InsertData(
                table: "EstadosCompra",
                columns: new[] { "Id", "Activo", "Descripcion", "FechaCreacion", "FechaModificacion" },
                values: new object[,]
                {
                    { 1, true, "Confirmada", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2, true, "Anulada",    new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3, true, "Pendiente",  new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            // 3. Agregar EstadoCompraId como nullable para poder hacer la data-migration
            migrationBuilder.AddColumn<int>(
                name: "EstadoCompraId",
                table: "Compras",
                type: "int",
                nullable: true);

            // 4. Data-migration: mapear Estado string → EstadoCompraId int
            migrationBuilder.Sql(@"
                UPDATE Compras
                SET EstadoCompraId = CASE
                    WHEN Estado = 'Anulada'   THEN 2
                    WHEN Estado = 'Pendiente' THEN 3
                    ELSE 1  -- Confirmada (default)
                END");

            // 5. Hacer EstadoCompraId NOT NULL ahora que todos tienen valor
            migrationBuilder.AlterColumn<int>(
                name: "EstadoCompraId",
                table: "Compras",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 6. Eliminar la columna de texto que ya no se usa
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Compras");

            // 7. Columna opcional para quien registró la compra
            migrationBuilder.AddColumn<int>(
                name: "RegistradoPorId",
                table: "Compras",
                type: "int",
                nullable: true);

            // 8. Índices de reportes
            migrationBuilder.CreateIndex(
                name: "IX_Compras_EstadoCompraId",
                table: "Compras",
                column: "EstadoCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_Fecha",
                table: "Compras",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_RegistradoPorId",
                table: "Compras",
                column: "RegistradoPorId");

            // 9. Claves foráneas
            migrationBuilder.AddForeignKey(
                name: "FK_Compras_EstadosCompra_EstadoCompraId",
                table: "Compras",
                column: "EstadoCompraId",
                principalTable: "EstadosCompra",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_Usuarios_RegistradoPorId",
                table: "Compras",
                column: "RegistradoPorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compras_EstadosCompra_EstadoCompraId",
                table: "Compras");

            migrationBuilder.DropForeignKey(
                name: "FK_Compras_Usuarios_RegistradoPorId",
                table: "Compras");

            migrationBuilder.DropTable(
                name: "EstadosCompra");

            migrationBuilder.DropIndex(
                name: "IX_Compras_EstadoCompraId",
                table: "Compras");

            migrationBuilder.DropIndex(
                name: "IX_Compras_Fecha",
                table: "Compras");

            migrationBuilder.DropIndex(
                name: "IX_Compras_RegistradoPorId",
                table: "Compras");


            migrationBuilder.DropColumn(
                name: "EstadoCompraId",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "RegistradoPorId",
                table: "Compras");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Compras",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
