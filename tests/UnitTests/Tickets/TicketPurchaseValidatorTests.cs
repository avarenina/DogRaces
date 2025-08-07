using Application.Tickets.Purchase;
using Domain.Bets;

namespace UnitTests.Tickets;

public sealed class TicketPurchaseValidatorTests
{
    private static TicketValidationOptions DefaultOptions() => new()
    {
        MinPayin = 1m,
        MaxPayin = 100m,
        MaxBets = 10,
        MinTotalOdds = 1.01m,
        MaxTotalOdds = 1000m,
        MaxWin = 500m,
    };

    [Fact]
    public void Validate_ShouldFail_WhenMultipleBetsFromSameRace()
    {
        // Arrange
        TicketValidationOptions options = DefaultOptions();
        var betRaceId = Guid.NewGuid();
        WinnerBet bet1 = new() { Id = Guid.NewGuid(), Odds = 2.0m, Runners = new List<int> { 1 }, Status = BetStatus.InProgress, Type = BetType.Winner, Race = new Domain.Races.Race(betRaceId, [0.5], DateTime.UtcNow, DateTime.UtcNow, Domain.Races.RaceStatus.Open) };
        WinnerBet bet2 = new() { Id = Guid.NewGuid(), Odds = 1.5m, Runners = new List<int> { 2 }, Status = BetStatus.InProgress, Type = BetType.Winner, Race = bet1.Race };

        TicketPurchaseValidator sut = new();
        PurchaseTicketCommand cmd = new() { Id = Guid.NewGuid(), Payin = 10m, Bets = new List<Guid> { bet1.Id, bet2.Id } };

        // Act
        bool ok = sut.Validate(cmd, new List<Bet> { bet1, bet2 }, options, out SharedKernel.Error? error);

        // Assert
        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Equal("Bets.OneBetPerRaceAllowed", error!.Code);
    }

    [Fact]
    public void Validate_ShouldFail_WhenTotalOddsOutOfRange()
    {
        // Arrange
        TicketValidationOptions options = DefaultOptions();
        options = new TicketValidationOptions
        {
            MinPayin = options.MinPayin,
            MaxPayin = options.MaxPayin,
            MaxBets = options.MaxBets,
            MinTotalOdds = options.MinTotalOdds,
            MaxTotalOdds = 1.5m,
            MaxWin = options.MaxWin
        };
        WinnerBet bet = new() { Id = Guid.NewGuid(), Odds = 2.0m, Runners = new List<int> { 1 }, Status = BetStatus.InProgress, Type = BetType.Winner, Race = new Domain.Races.Race(Guid.NewGuid(), [0.5], DateTime.UtcNow, DateTime.UtcNow, Domain.Races.RaceStatus.Open) };
        TicketPurchaseValidator sut = new();
        PurchaseTicketCommand cmd = new() { Id = Guid.NewGuid(), Payin = 10m, Bets = new List<Guid> { bet.Id } };

        // Act
        bool ok = sut.Validate(cmd, new List<Bet> { bet }, options, out SharedKernel.Error? error);

        // Assert
        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Equal("Tickets.TotalOddsOutOfRange", error!.Code);
    }

    [Fact]
    public void Validate_ShouldFail_WhenMaxWinExceeded()
    {
        // Arrange
        TicketValidationOptions options = DefaultOptions();
        options = new TicketValidationOptions
        {
            MinPayin = options.MinPayin,
            MaxPayin = options.MaxPayin,
            MaxBets = options.MaxBets,
            MinTotalOdds = options.MinTotalOdds,
            MaxTotalOdds = options.MaxTotalOdds,
            MaxWin = 50m
        };
        WinnerBet bet = new() { Id = Guid.NewGuid(), Odds = 3.0m, Runners = new List<int> { 1 }, Status = BetStatus.InProgress, Type = BetType.Winner, Race = new Domain.Races.Race(Guid.NewGuid(), [0.5], DateTime.UtcNow, DateTime.UtcNow, Domain.Races.RaceStatus.Open) };
        TicketPurchaseValidator sut = new();
        PurchaseTicketCommand cmd = new() { Id = Guid.NewGuid(), Payin = 20m, Bets = new List<Guid> { bet.Id } };

        // Act
        bool ok = sut.Validate(cmd, new List<Bet> { bet }, options, out SharedKernel.Error? error);

        // Assert
        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Equal("Tickets.MaxWinExceeded", error!.Code);
    }

    [Fact]
    public void Validate_ShouldPass_ForValidSelection()
    {
        // Arrange
        TicketValidationOptions options = DefaultOptions();
        Domain.Races.Race race1 = new(Guid.NewGuid(), [0.5], DateTime.UtcNow, DateTime.UtcNow, Domain.Races.RaceStatus.Open);
        Domain.Races.Race race2 = new(Guid.NewGuid(), [0.5], DateTime.UtcNow, DateTime.UtcNow, Domain.Races.RaceStatus.Open);
        WinnerBet bet1 = new() { Id = Guid.NewGuid(), Odds = 2.0m, Runners = new List<int> { 1 }, Status = BetStatus.InProgress, Type = BetType.Winner, Race = race1 };
        WinnerBet bet2 = new() { Id = Guid.NewGuid(), Odds = 1.5m, Runners = new List<int> { 2 }, Status = BetStatus.InProgress, Type = BetType.Winner, Race = race2 };
        TicketPurchaseValidator sut = new();
        PurchaseTicketCommand cmd = new() { Id = Guid.NewGuid(), Payin = 10m, Bets = new List<Guid> { bet1.Id, bet2.Id } };

        // Act
        bool ok = sut.Validate(cmd, new List<Bet> { bet1, bet2 }, options, out SharedKernel.Error? error);

        // Assert
        Assert.True(ok);
        Assert.Null(error);
    }
}


