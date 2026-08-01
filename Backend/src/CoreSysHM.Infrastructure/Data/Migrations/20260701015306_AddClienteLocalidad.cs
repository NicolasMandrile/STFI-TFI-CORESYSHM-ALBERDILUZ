using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreSysHM.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClienteLocalidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Localidad",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Localidad",
                table: "Clientes");
        }
    }
}
