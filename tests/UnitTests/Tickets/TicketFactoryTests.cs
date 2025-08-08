using Application.Abstractions;
using Application.Tickets.Purchase;
using Domain.Bets;
using Domain.Ticket;
using Moq;
using SharedKernel;

namespace UnitTests.Tickets;

public sealed class TicketFactoryTests
{
    [Fact]
    public void Create_ShouldSetPendingStatus_ComputeTotals_AndRaiseEvent()
    {
        // Arrange
        var now = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.Setup(x => x.UtcNow).Returns(now);

        var bets = new List<Bet>
        {
            new WinnerBet { Id = Guid.NewGuid(), Odds = 2.00m, Runners = new List<int>{1}, Status = BetStatus.InProgress, Type = BetType.Winner },
            new WinnerBet { Id = Guid.NewGuid(), Odds = 1.50m, Runners = new List<int>{2}, Status = BetStatus.InProgress, Type = BetType.Winner },
        };

        TicketFactory factory = new(dateTimeProvider.Object);
        var ticketId = Guid.NewGuid();
        decimal payin = 10m;

        // Act
        Ticket ticket = factory.Create(ticketId, bets, payin);

        // Assert
        Assert.Equal(ticketId, ticket.Id);
        Assert.Equal(TicketStatus.Pending, ticket.Status);
        Assert.Equal(payin, ticket.Payin);
        Assert.Equal(now, ticket.CreatedAt);

        decimal expectedTotalOdds = 2.00m * 1.50m;
        decimal expectedWin = Math.Round(payin * expectedTotalOdds, 2, MidpointRounding.AwayFromZero);
        Assert.Equal(expectedTotalOdds, ticket.TotalOdds);
        Assert.Equal(expectedWin, ticket.WinAmount);

        Assert.Equal(2, ticket.Bets.Count);
        Assert.All(ticket.Bets, tb => Assert.Equal(BetStatus.InProgress, tb.Status));
    }
}


