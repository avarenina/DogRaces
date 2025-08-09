using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "public");

        migrationBuilder.CreateTable(
            name: "races",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                result = table.Column<string>(type: "text", nullable: true),
                is_completed = table.Column<bool>(type: "boolean", nullable: false),
                probabilities = table.Column<string>(type: "text", nullable: false),
                start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                status = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_races", x => x.id);
            });

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
            name: "bets",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                odds = table.Column<decimal>(type: "numeric", nullable: false),
                race_id = table.Column<Guid>(type: "uuid", nullable: false),
                runners = table.Column<string>(type: "text", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                type = table.Column<int>(type: "integer", nullable: false)
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
            name: "ix_bets_race_id",
            schema: "public",
            table: "bets",
            column: "race_id");

        migrationBuilder.CreateIndex(
            name: "IX_Races_Upcoming",
            schema: "public",
            table: "races",
            column: "start_time",
            filter: "is_completed = FALSE");

        migrationBuilder.CreateIndex(
            name: "ix_ticket_bets_bet_id",
            schema: "public",
            table: "ticket_bets",
            column: "bet_id");

        migrationBuilder.CreateIndex(
            name: "IX_TicketBets_Status_InProgress",
            schema: "public",
            table: "ticket_bets",
            column: "status",
            filter: "status = 0");

        migrationBuilder.CreateIndex(
            name: "IX_TicketBets_TicketId_Status",
            schema: "public",
            table: "ticket_bets",
            columns: ["ticket_id", "status"]);

        migrationBuilder.CreateIndex(
            name: "IX_Tickets_Status_CompletedAt",
            schema: "public",
            table: "tickets",
            columns: ["status", "completed_at"]);

        migrationBuilder.CreateIndex(
            name: "IX_Tickets_ToProcess",
            schema: "public",
            table: "tickets",
            column: "created_at",
            filter: "status = 2 AND completed_at IS NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ticket_bets",
            schema: "public");

        migrationBuilder.DropTable(
            name: "bets",
            schema: "public");

        migrationBuilder.DropTable(
            name: "tickets",
            schema: "public");

        migrationBuilder.DropTable(
            name: "races",
            schema: "public");
    }
}
