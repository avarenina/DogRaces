using Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Tickets;
internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder
            .HasIndex(t => new { t.Status, t.CompletedAt })
            .HasDatabaseName("IX_Tickets_Status_CompletedAt");

        // For UI pagination: ORDER BY CreatedAt DESC
        builder
            .HasIndex(t => t.CreatedAt)
            .HasDatabaseName("IX_Tickets_CreatedAt");

        // For processor: status = Success AND completed_at IS NULL ordered by CreatedAt
        builder
            .HasIndex(t => t.CreatedAt)
            .HasFilter("status = 2 AND completed_at IS NULL")
            .HasDatabaseName("IX_Tickets_ToProcess");
    }
}
