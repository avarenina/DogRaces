using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Shouldly;

namespace UnitTests.Races;
public class RaceFactoryTests
{
    [Fact]
    public void Create_ShouldReturnRaceWithCorrectPropertiesAndBets()
    {
        // Arrange
        var mockDateTimeProvider = new Mock<SharedKernel.IDateTimeProvider>();
        var mockProbabilityCalculator = new Mock<Application.Abstractions.IProbabilityCalculator>();
        var mockBetFactory = new Mock<Application.Abstractions.IBetFactory>();

        var now = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        mockDateTimeProvider.Setup(x => x.UtcNow).Returns(now);

        int numberOfRunners = 3;
        double bookmakerMargin = 0.1;
        double[] probabilities = [0.5, 0.3, 0.2];
        mockProbabilityCalculator.Setup(x => x.CalculateWinnerProbabilities(numberOfRunners, bookmakerMargin)).Returns(probabilities);

        //var race = new Domain.Races.Race(Guid.NewGuid(), probabilities, now.AddHours(1), now, Domain.Races.RaceStatus.Open);
        var winnerBets = new List<Domain.Bets.Bet> { new Domain.Bets.WinnerBet { Id = Guid.NewGuid(), Odds = 2.0m, Runners = [1], Status = Domain.Bets.BetStatus.InProgress, Type = Domain.Bets.BetType.Winner } };
        var withinFirstThreeBets = new List<Domain.Bets.Bet> { new Domain.Bets.WithinFirstThreeBet { Id = Guid.NewGuid(), Odds = 1.5m, Runners = [2], Status = Domain.Bets.BetStatus.InProgress, Type = Domain.Bets.BetType.WithinFirstThree } };
        mockBetFactory.Setup(x => x.CreateWinnerBets(It.IsAny<Domain.Races.Race>())).Returns(winnerBets);
        mockBetFactory.Setup(x => x.CreateWithinFirstThreeBets(It.IsAny<Domain.Races.Race>())).Returns(withinFirstThreeBets);

        var factory = new Application.Races.Create.RaceFactory(
            mockDateTimeProvider.Object,
            mockProbabilityCalculator.Object,
            mockBetFactory.Object
        );

        DateTime startTime = now.AddHours(1);

        // Act
        Domain.Races.Race result = factory.Create(startTime, numberOfRunners, bookmakerMargin);

        // Assert
        result.ShouldNotBeNull();
        result.Probabilities.ShouldBe(probabilities);
        result.StartTime.ShouldBe(startTime);
        result.CreatedAt.ShouldBe(now);
        result.Status.ShouldBe(Domain.Races.RaceStatus.Open);
        result.Bets.ShouldContain(winnerBets[0]);
        result.Bets.ShouldContain(withinFirstThreeBets[0]);
        result.Bets.Count.ShouldBe(2);
    }
}
