using Domain.Ticket;
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
    }
}
