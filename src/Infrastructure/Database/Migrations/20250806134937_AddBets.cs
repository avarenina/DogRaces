using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddBets : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "bets",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                odds = table.Column<decimal>(type: "numeric", nullable: false),
                race_id = table.Column<Guid>(type: "uuid", nullable: false),
                runners = table.Column<string>(type: "text", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                type = table.Column<int>(type: "integer", nullable: false),
                bet_type = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_bets", x => x.id);
                table.ForeignKey(
                    name: "fk_bets_races_race_id",
                    column: x => x.race_id,
                    principalSchema: "public",
                    principalTable: "races",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_bets_race_id",
            schema: "public",
            table: "bets",
            column: "race_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "bets",
            schema: "public");
    }
}
