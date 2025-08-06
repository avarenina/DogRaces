using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions;
using Domain.Bets;
using Domain.Races;

namespace Application.Races.Create;
public sealed class BetFactory : IBetFactory
{
    public List<Bet> CreateWinnerBets(Race race)
    {
        var bets = new List<Bet>();

        for (int i = 0; i < race.Probabilities.Length; i++)
        {
            decimal odds = Math.Round((decimal)(1.0 / race.Probabilities[i]), 2);

            bets.Add(new WinnerBet
            {
                Id = Guid.NewGuid(),
                Runners = [i + 1],
                Odds = odds,
                Status = BetStatus.InProgress,
                Type = BetType.Winner
            });
        }

        return bets;
    }

    public List<Bet> CreateWithinFirstThreeBets(Race race)
    {
        var bets = new List<Bet>();

        // Assuming each runner's probability to be in top 3 is roughly 3 times their win probability
        // This is a simplification - a more accurate calculation would consider pairwise probabilities
        for (int i = 0; i < race.Probabilities.Length; i++)
        {
            // Multiply win probability by ~3 for top 3 probability
            double topThreeProbability = race.Probabilities[i] * 3;
            // Ensure probability doesn't exceed 1
            topThreeProbability = Math.Min(topThreeProbability, 0.99);
            decimal odds = Math.Round((decimal)(1.0 / topThreeProbability), 2);

            bets.Add(new WithinFirstThreeBet  // Assuming you have this bet type
            {
                Id = Guid.NewGuid(),
                Runners = [i + 1],
                Odds = odds,
                Status = BetStatus.InProgress,
                Type = BetType.WithinFirstThree
            });
        }

        return bets;
    }
}
