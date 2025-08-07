using System.Globalization;
using Domain.Races;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Races;

internal sealed class RaceConfiguration : IEntityTypeConfiguration<Race>
{
    public void Configure(EntityTypeBuilder<Race> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(r => r.Probabilities)
        .HasConversion(
            // Serialize using invariant culture and period as decimal separator
            v => string.Join(";", v.Select(d => d.ToString(CultureInfo.InvariantCulture))),

            // Deserialize safely with invariant culture
            v => string.IsNullOrWhiteSpace(v)
                ? Array.Empty<double>()
                : v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                   .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                   .ToArray()
        );
    }
}
