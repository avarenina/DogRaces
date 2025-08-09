using Application.Tickets.Purchase;
using Domain.Tickets;

namespace UnitTests.Tickets;

public sealed class PurchaseTicketCommandValidatorTests
{
    private static TicketValidationOptions Options() => new()
    {
        MinPayin = 1m,
        MaxPayin = 100m,
        MaxBets = 3,
        MinTotalOdds = 1.01m,
        MaxTotalOdds = 1000m,
        MaxWin = 1000m,
    };

    [Fact]
    public void Should_Fail_When_Payin_Out_Of_Range()
    {
        PurchaseTicketCommandValidator validator = new(Options());
        PurchaseTicketCommand cmd = new() { Id = Guid.NewGuid(), Payin = 0.5m, Bets = new List<Guid> { Guid.NewGuid() } };
        FluentValidation.Results.ValidationResult result = validator.Validate(cmd);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Fail_When_Duplicate_Bets()
    {
        var id = Guid.NewGuid();
        var validator = new PurchaseTicketCommandValidator(Options());
        var cmd = new PurchaseTicketCommand { Id = Guid.NewGuid(), Payin = 10m, Bets = new List<Guid> { id, id } };
        FluentValidation.Results.ValidationResult result = validator.Validate(cmd);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Pass_When_Shape_Is_Valid()
    {
        PurchaseTicketCommandValidator validator = new(Options());
        PurchaseTicketCommand cmd = new() { Id = Guid.NewGuid(), Payin = 10m, Bets = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };
        FluentValidation.Results.ValidationResult result = validator.Validate(cmd);
        Assert.True(result.IsValid);
    }
}


