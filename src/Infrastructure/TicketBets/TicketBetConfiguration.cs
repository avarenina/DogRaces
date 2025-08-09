using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Tickets;

namespace Infrastructure.TicketBets;

internal sealed class TicketBetConfiguration : IEntityTypeConfiguration<TicketBet>
{
    public void Configure(EntityTypeBuilder<TicketBet> builder)
    {
        builder.HasKey(t => t.Id);

        builder.HasOne(tb => tb.Ticket)
            .WithMany(t => t.Bets)
            .HasForeignKey(tb => tb.TicketId)
            .IsRequired();

        builder
            .HasIndex(tb => new { tb.TicketId, tb.Status })
            .HasDatabaseName("IX_TicketBets_TicketId_Status");
    }
}
