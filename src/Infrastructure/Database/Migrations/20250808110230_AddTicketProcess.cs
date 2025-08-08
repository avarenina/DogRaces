using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddTicketProcess : Migration
{
    private static readonly string[] ticketColumns = ["status", "completed_at"];
    private static readonly string[] ticketBetcolumns = ["ticket_id", "status"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_ticket_bets_ticket_id",
            schema: "public",
            table: "ticket_bets");

        migrationBuilder.CreateIndex(
            name: "IX_Tickets_Status_CompletedAt",
            schema: "public",
            table: "tickets",
            columns: ticketColumns);

        migrationBuilder.CreateIndex(
            name: "IX_TicketBets_TicketId_Status",
            schema: "public",
            table: "ticket_bets",
            columns: ticketBetcolumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Tickets_Status_CompletedAt",
            schema: "public",
            table: "tickets");

        migrationBuilder.DropIndex(
            name: "IX_TicketBets_TicketId_Status",
            schema: "public",
            table: "ticket_bets");

        migrationBuilder.CreateIndex(
            name: "ix_ticket_bets_ticket_id",
            schema: "public",
            table: "ticket_bets",
            column: "ticket_id");
    }
}
