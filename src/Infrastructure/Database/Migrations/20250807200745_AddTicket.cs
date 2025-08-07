using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddTicket : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tickets",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                payin = table.Column<decimal>(type: "numeric", nullable: false),
                total_odds = table.Column<decimal>(type: "numeric", nullable: false),
                win_amount = table.Column<decimal>(type: "numeric", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tickets", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "ticket_bets",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                bet_id = table.Column<Guid>(type: "uuid", nullable: false),
                odds = table.Column<decimal>(type: "numeric", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ticket_bets", x => x.id);
                table.ForeignKey(
                    name: "fk_ticket_bets_bets_bet_id",
                    column: x => x.bet_id,
                    principalSchema: "public",
                    principalTable: "bets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_ticket_bets_tickets_ticket_id",
                    column: x => x.ticket_id,
                    principalSchema: "public",
                    principalTable: "tickets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_ticket_bets_bet_id",
            schema: "public",
            table: "ticket_bets",
            column: "bet_id");

        migrationBuilder.CreateIndex(
            name: "ix_ticket_bets_ticket_id",
            schema: "public",
            table: "ticket_bets",
            column: "ticket_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ticket_bets",
            schema: "public");

        migrationBuilder.DropTable(
            name: "tickets",
            schema: "public");
    }
}
