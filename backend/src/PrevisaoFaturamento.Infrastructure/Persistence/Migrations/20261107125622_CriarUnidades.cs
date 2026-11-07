using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrevisaoFaturamento.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CriarUnidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Unidades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Endereco = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MetaMensal = table.Column<long>(type: "INTEGER", nullable: false),
                    ProjecaoFaturamento = table.Column<long>(type: "INTEGER", nullable: false),
                    DataInicio = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unidades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Unidades_UsuarioId",
                table: "Unidades",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Unidades");
        }
    }
}
