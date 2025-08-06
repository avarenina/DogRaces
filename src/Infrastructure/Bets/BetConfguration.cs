using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Bets;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Bets;

internal sealed class BetConfiguration : IEntityTypeConfiguration<Bet>
{
    public void Configure(EntityTypeBuilder<Bet> builder)
    {
        builder.HasKey(t => t.Id);

        var converter = new ValueConverter<List<int>, string>(
            v => SerializeList(v),
            v => DeserializeList(v)
        );


        builder.Property(b => b.Runners)
            .HasConversion(converter)
            .IsRequired();

        builder.Ignore(b => b.Type);

        builder.HasDiscriminator<BetType>("Type")
        .HasValue<WinnerBet>(BetType.Winner);
    }

    // TODO: Move to separate helper
    private static string SerializeList(List<int> list)
    {
        return string.Join(",", list);
    }

    private static List<int> DeserializeList(string csv)
    {
        if (string.IsNullOrEmpty(csv))
        {
            return [];
        }

        return [.. csv.Split(',').Select(x => int.Parse(x, CultureInfo.InvariantCulture))];
    }
}
