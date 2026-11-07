using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrevisaoFaturamento.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CriarFaturamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FaturamentosDiarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UnidadeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Data = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    StatusCaixa = table.Column<int>(type: "INTEGER", nullable: false),
                    FundoDeCaixa = table.Column<long>(type: "INTEGER", nullable: false),
                    ValorConferido = table.Column<long>(type: "INTEGER", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FechadoPorUsuarioId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DataFechamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaturamentosDiarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaturamentosParciais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FaturamentoDiarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Valor = table.Column<long>(type: "INTEGER", nullable: false),
                    HoraInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoraFim = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Origem = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsAtivo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaturamentosParciais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaturamentosParciais_FaturamentosDiarios_FaturamentoDiarioId",
                        column: x => x.FaturamentoDiarioId,
                        principalTable: "FaturamentosDiarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaturamentosDiarios_UnidadeId_Data",
                table: "FaturamentosDiarios",
                columns: new[] { "UnidadeId", "Data" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaturamentosParciais_FaturamentoDiarioId",
                table: "FaturamentosParciais",
                column: "FaturamentoDiarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaturamentosParciais");

            migrationBuilder.DropTable(
                name: "FaturamentosDiarios");
        }
    }
}
