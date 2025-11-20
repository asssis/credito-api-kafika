using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CreditApi.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "credito",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    numero_credito = table.Column<string>(type: "text", nullable: false),
                    numero_nfse = table.Column<string>(type: "text", nullable: false),
                    data_constituicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor_issqn = table.Column<decimal>(type: "numeric", nullable: false),
                    tipo_credito = table.Column<string>(type: "text", nullable: false),
                    simples_nacional = table.Column<bool>(type: "boolean", nullable: false),
                    aliquota = table.Column<decimal>(type: "numeric", nullable: false),
                    valor_faturado = table.Column<decimal>(type: "numeric", nullable: false),
                    valor_deducao = table.Column<decimal>(type: "numeric", nullable: false),
                    base_calculo = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credito", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credito");
        }
    }
}
