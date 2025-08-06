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
                v => string.Join(",", v),                     // double[] → string
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(double.Parse)
                      .ToArray()                             // string → double[]
            );
    }
}
