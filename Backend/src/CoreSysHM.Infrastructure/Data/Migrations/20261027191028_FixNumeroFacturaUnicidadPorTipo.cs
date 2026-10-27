using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreSysHM.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixNumeroFacturaUnicidadPorTipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Facturas_NumeroFactura",
                table: "Facturas");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_PuntoVenta_Tipo_Numero",
                table: "Facturas",
                columns: new[] { "PuntoVentaId", "TipoComprobanteId", "NumeroFactura" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Facturas_PuntoVenta_Tipo_Numero",
                table: "Facturas");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_NumeroFactura",
                table: "Facturas",
                column: "NumeroFactura",
                unique: true);
        }
    }
}
