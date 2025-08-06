using Domain.Bets;
using System.Security.Cryptography;
using Domain.Races;
using SharedKernel;

namespace Application.Races.Create;

internal class RaceFactory(
    IDateTimeProvider dateTimeProvider
) 
{
    public Race Create(DateTime startTime)
    {
        var race = new Race
        {
            Id = Guid.NewGuid(),
            StartTime = startTime,
            CreatedAt = dateTimeProvider.UtcNow,
            Status = RaceStatus.Open
        };

        race.Bets = WinnerBetGenerator.GenerateWinnerBets(6, race);

        race.Raise(new RaceCreatedDomainEvent(race.Id));

        return race;
    }
}

public static class WinnerBetGenerator
{
    public static List<Bet> GenerateWinnerBets(int numDogs, Race race, double bookmakerMargin = 0.05)
    {
        double[] winnerProbabilities = new double[numDogs];

        // Generate secure random doubles between 0 and 1
        for (int i = 0; i < numDogs; i++)
        {
            winnerProbabilities[i] = GetSecureRandomDouble();
        }

        // Normalize so sum = 1
        double sum = winnerProbabilities.Sum();
        for (int i = 0; i < numDogs; i++)
        {
            winnerProbabilities[i] /= sum;
        }

        // Apply bookmaker margin: scale to sum > 1
        double scale = (1 + bookmakerMargin) / winnerProbabilities.Sum();
        for (int i = 0; i < numDogs; i++)
        {
            winnerProbabilities[i] *= scale;
        }

        var bets = new List<Bet>();

        for (int i = 0; i < numDogs; i++)
        {
            decimal odds = Math.Round((decimal)(1.0 / winnerProbabilities[i]), 2);

            bets.Add(new WinnerBet
            {
                Race = race,
                Runners = [i + 1],
                Odds = odds,
                Status = BetStatus.InProgress,
                Id = Guid.NewGuid(),
                Type = BetType.Winner
            });
        }

        return bets;
    }

    // Generate a cryptographically secure random double between 0.0 and 1.0
    private static double GetSecureRandomDouble()
    {
        // 8 bytes = 64 bits for a double precision float
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);

        ulong ulongRand = BitConverter.ToUInt64(bytes);

        // Scale ulong to double between 0 and 1 (exclusive)
        return ulongRand / (double)ulong.MaxValue;
    }
}
