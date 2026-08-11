using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreSysHM.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturacionMaestrosCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Facturas existentes no tienen TipoComprobanteId/PuntoVentaId válidos (esas columnas
            // recién se crean acá) -- se limpian antes de agregar las FK NOT NULL. Son datos de
            // prueba (el servicio de Facturación nunca llegó a implementarse hasta esta iteración,
            // ver DbInitializer/SeedTestData); no hay datos reales que preservar.
            migrationBuilder.Sql("DELETE FROM Facturas;");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_VentaId",
                table: "Facturas");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Proveedores",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RazonSocial",
                table: "Proveedores",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Proveedores",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cuit",
                table: "Proveedores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CondicionFiscalId",
                table: "Proveedores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Proveedores",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Facturas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PuntoVentaId",
                table: "Facturas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Facturas",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "TipoComprobanteId",
                table: "Facturas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Dni",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cuit",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Apellido",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CondicionFiscalId",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Clientes",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "CondicionesFiscales",
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
                    table.PrimaryKey("PK_CondicionesFiscales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetallesFactura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacturaId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    DetalleVentaId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Impuesto = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesFactura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_DetallesVenta_DetalleVentaId",
                        column: x => x.DetalleVentaId,
                        principalTable: "DetallesVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialCambios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Entidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntidadId = table.Column<int>(type: "int", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialCambios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialCambios_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PuntosVenta",
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
                    table.PrimaryKey("PK_PuntosVenta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposComprobante",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AfectaStock = table.Column<bool>(type: "bit", nullable: false),
                    SignoContable = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposComprobante", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NumeracionesComprobante",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PuntoVentaId = table.Column<int>(type: "int", nullable: false),
                    TipoComprobanteId = table.Column<int>(type: "int", nullable: false),
                    UltimoNumero = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumeracionesComprobante", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NumeracionesComprobante_PuntosVenta_PuntoVentaId",
                        column: x => x.PuntoVentaId,
                        principalTable: "PuntosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NumeracionesComprobante_TiposComprobante_TipoComprobanteId",
                        column: x => x.TipoComprobanteId,
                        principalTable: "TiposComprobante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_CondicionFiscalId",
                table: "Proveedores",
                column: "CondicionFiscalId");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_Cuit",
                table: "Proveedores",
                column: "Cuit",
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_FechaEmision",
                table: "Facturas",
                column: "FechaEmision");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdempotencyKey",
                table: "Facturas",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_PuntoVentaId",
                table: "Facturas",
                column: "PuntoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_TipoComprobanteId",
                table: "Facturas",
                column: "TipoComprobanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_VentaId",
                table: "Facturas",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_CondicionFiscalId",
                table: "Clientes",
                column: "CondicionFiscalId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Cuit",
                table: "Clientes",
                column: "Cuit",
                unique: true,
                filter: "[Cuit] IS NOT NULL AND [Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Dni",
                table: "Clientes",
                column: "Dni",
                unique: true,
                filter: "[Dni] IS NOT NULL AND [Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CondicionesFiscales_Descripcion",
                table: "CondicionesFiscales",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesFactura_DetalleVentaId",
                table: "DetallesFactura",
                column: "DetalleVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesFactura_FacturaId",
                table: "DetallesFactura",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesFactura_ProductoId",
                table: "DetallesFactura",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCambios_Entidad",
                table: "HistorialCambios",
                columns: new[] { "Entidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCambios_Fecha",
                table: "HistorialCambios",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCambios_UsuarioId",
                table: "HistorialCambios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_NumeracionesComprobante_PuntoVenta_Tipo",
                table: "NumeracionesComprobante",
                columns: new[] { "PuntoVentaId", "TipoComprobanteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NumeracionesComprobante_TipoComprobanteId",
                table: "NumeracionesComprobante",
                column: "TipoComprobanteId");

            migrationBuilder.CreateIndex(
                name: "IX_PuntosVenta_Descripcion",
                table: "PuntosVenta",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposComprobante_Descripcion",
                table: "TiposComprobante",
                column: "Descripcion",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_CondicionesFiscales_CondicionFiscalId",
                table: "Clientes",
                column: "CondicionFiscalId",
                principalTable: "CondicionesFiscales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_PuntosVenta_PuntoVentaId",
                table: "Facturas",
                column: "PuntoVentaId",
                principalTable: "PuntosVenta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_TiposComprobante_TipoComprobanteId",
                table: "Facturas",
                column: "TipoComprobanteId",
                principalTable: "TiposComprobante",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Proveedores_CondicionesFiscales_CondicionFiscalId",
                table: "Proveedores",
                column: "CondicionFiscalId",
                principalTable: "CondicionesFiscales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_CondicionesFiscales_CondicionFiscalId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_PuntosVenta_PuntoVentaId",
                table: "Facturas");

            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_TiposComprobante_TipoComprobanteId",
                table: "Facturas");

            migrationBuilder.DropForeignKey(
                name: "FK_Proveedores_CondicionesFiscales_CondicionFiscalId",
                table: "Proveedores");

            migrationBuilder.DropTable(
                name: "CondicionesFiscales");

            migrationBuilder.DropTable(
                name: "DetallesFactura");

            migrationBuilder.DropTable(
                name: "HistorialCambios");

            migrationBuilder.DropTable(
                name: "NumeracionesComprobante");

            migrationBuilder.DropTable(
                name: "PuntosVenta");

            migrationBuilder.DropTable(
                name: "TiposComprobante");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_CondicionFiscalId",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_Cuit",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_FechaEmision",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_IdempotencyKey",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_PuntoVentaId",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_TipoComprobanteId",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_VentaId",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_CondicionFiscalId",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Cuit",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Dni",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CondicionFiscalId",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "PuntoVentaId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "TipoComprobanteId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "CondicionFiscalId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RazonSocial",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cuit",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Dni",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cuit",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Apellido",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_VentaId",
                table: "Facturas",
                column: "VentaId",
                unique: true);
        }
    }
}
