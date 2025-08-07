using Application.Races.Create;
using Domain.Bets;
using Domain.Races;

namespace UnitTests.Races;

public class BetFactoryTests
{
    private static readonly int[] expected = new[] { 1, 2, 3 };

    [Fact]
    public void CreateWinnerBets_ShouldReturnBetsWithCorrectOddsAndProperties()
    {
        // Arrange
        var race = new Race(Guid.NewGuid(), [0.5, 0.3, 0.2], DateTime.UtcNow.AddHours(1), DateTime.UtcNow, RaceStatus.Open);
        var factory = new BetFactory();

        // Act
        List<Bet> bets = [.. factory.Create(race).Where(b => b is WinnerBet)];
       

        // Assert
        bets.Count.ShouldBe(3);
        bets[0].Odds.ShouldBe(Math.Round((decimal)(1.0 / 0.5), 2));
        bets[1].Odds.ShouldBe(Math.Round((decimal)(1.0 / 0.3), 2));
        bets[2].Odds.ShouldBe(Math.Round((decimal)(1.0 / 0.2), 2));
        bets.All(b => b.Status == BetStatus.InProgress).ShouldBeTrue();
        bets.All(b => b.Type == BetType.Winner).ShouldBeTrue();
        bets.SelectMany(b => b.Runners).ShouldBe(expected);
    }

    [Fact]
    public void CreateWithinFirstThreeBets_ShouldReturnBetsWithCorrectOddsAndProperties()
    {
        // Arrange
        var race = new Race(Guid.NewGuid(), [0.5, 0.3, 0.2], DateTime.UtcNow.AddHours(1), DateTime.UtcNow, RaceStatus.Open);
        var factory = new BetFactory();

        // Act
        List<Bet> bets = [.. factory.Create(race).Where(b => b is WithinFirstThreeBet)];

        // Assert
        bets.Count.ShouldBe(3);
        bets[0].Odds.ShouldBe(Math.Round((decimal)(1.0 / Math.Min(0.5 * 3, 0.99)), 2));
        bets[1].Odds.ShouldBe(Math.Round((decimal)(1.0 / Math.Min(0.3 * 3, 0.99)), 2));
        bets[2].Odds.ShouldBe(Math.Round((decimal)(1.0 / Math.Min(0.2 * 3, 0.99)), 2));
        bets.All(b => b.Status == BetStatus.InProgress).ShouldBeTrue();
        bets.All(b => b.Type == BetType.WithinFirstThree).ShouldBeTrue();
        bets.SelectMany(b => b.Runners).ShouldBe(expected);
    }

    [Fact]
    public void CreateBets_WithNoProbabilities_ShouldReturnEmptyList()
    {
        // Arrange
        var race = new Race(Guid.NewGuid(), [], DateTime.UtcNow.AddHours(1), DateTime.UtcNow, RaceStatus.Open);
        var factory = new BetFactory();

        // Act
        List<Bet> bets = factory.Create(race);

        // Assert
        bets.ShouldBeEmpty();
    }

    
}
