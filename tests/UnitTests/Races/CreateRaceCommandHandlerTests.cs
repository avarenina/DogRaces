using Application.Races.Create;
using Application.Abstractions;
using Application.Abstractions.Data;
using Domain.Races;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using SharedKernel;
using Application.Abstractions.Messaging;

namespace UnitTests.Races;

public class CreateRaceCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateSingleRaceAndReturnId()
    {
        // Arrange
        var mockContext = new Mock<IApplicationDbContext>();
        var mockDateTimeProvider = new Mock<IDateTimeProvider>();
        var mockCache = new Mock<IDistributedCache>();
        var mockRaceFactory = new Mock<IRaceFactory>();
        var mockMessagePublisher = new Mock<IMessagePublisher>();
        var now = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        mockDateTimeProvider.Setup(x => x.UtcNow).Returns(now);
        var race = new Race(Guid.NewGuid(), [0.5, 0.3, 0.2], now.AddHours(1), now, RaceStatus.Open);
        mockRaceFactory.Setup(f => f.Create(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<double>())).Returns(race);
        var races = new List<Race>();
        mockContext.Setup(c => c.Races.Add(It.IsAny<Race>())).Callback<Race>(r => races.Add(r));
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mockCache.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new CreateRaceCommandHandler(mockContext.Object, mockDateTimeProvider.Object, mockCache.Object, mockRaceFactory.Object, mockMessagePublisher.Object);
        var command = new CreateRaceCommand
        {
            LastRaceStartTime = null,
            AmountOfRacesToCreate = 1,
            TimeBetweenRaces = 60,
            NumberOfRunners = 3,
            BookmakerMargin = 0.1
        };
        // Act
        Result result = await handler.Handle(command, CancellationToken.None);
        // Assert
        result.IsSuccess.ShouldBeTrue();
        races.Count.ShouldBe(1);
        races[0].ShouldBe(race);
        mockCache.Verify(c => c.RemoveAsync(CacheKeys.UpcomingRaces, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateMultipleRacesWithCorrectStartTimes()
    {
        // Arrange
        var mockContext = new Mock<IApplicationDbContext>();
        var mockDateTimeProvider = new Mock<IDateTimeProvider>();
        var mockCache = new Mock<IDistributedCache>();
        var mockRaceFactory = new Mock<IRaceFactory>();
        var mockMessagePublisher = new Mock<IMessagePublisher>();
        var now = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        mockDateTimeProvider.Setup(x => x.UtcNow).Returns(now);
        var createdRaces = new List<Race>();
        mockRaceFactory.Setup(f => f.Create(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<double>()))
            .Returns<DateTime, int, double>((start, runners, margin) => new Race(Guid.NewGuid(), [0.5, 0.3, 0.2], start, now, RaceStatus.Open));
        mockContext.Setup(c => c.Races.Add(It.IsAny<Race>())).Callback<Race>(r => createdRaces.Add(r));
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mockCache.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new CreateRaceCommandHandler(mockContext.Object, mockDateTimeProvider.Object, mockCache.Object, mockRaceFactory.Object, mockMessagePublisher.Object);
        var command = new CreateRaceCommand
        {
            LastRaceStartTime = now,
            AmountOfRacesToCreate = 3,
            TimeBetweenRaces = 60,
            NumberOfRunners = 3,
            BookmakerMargin = 0.1
        };
        // Act
        Result result = await handler.Handle(command, CancellationToken.None);
        // Assert
        result.IsSuccess.ShouldBeTrue();
        createdRaces.Count.ShouldBe(3);
        createdRaces[0].StartTime.ShouldBe(now.AddSeconds(60));
        createdRaces[1].StartTime.ShouldBe(now.AddSeconds(120));
        createdRaces[2].StartTime.ShouldBe(now.AddSeconds(180));
    }

    [Fact]
    public async Task Handle_WithZeroAmountOfRaces_ShouldNotCreateAnyRace()
    {
        // Arrange
        var mockContext = new Mock<IApplicationDbContext>();
        var mockDateTimeProvider = new Mock<IDateTimeProvider>();
        var mockCache = new Mock<IDistributedCache>();
        var mockRaceFactory = new Mock<IRaceFactory>();
        var mockMessagePublisher = new Mock<IMessagePublisher>();
        var handler = new CreateRaceCommandHandler(mockContext.Object, mockDateTimeProvider.Object, mockCache.Object, mockRaceFactory.Object, mockMessagePublisher.Object);
        var command = new CreateRaceCommand
        {
            LastRaceStartTime = null,
            AmountOfRacesToCreate = 0,
            TimeBetweenRaces = 60,
            NumberOfRunners = 3,
            BookmakerMargin = 0.1
        };
        // Act
        Result result = await handler.Handle(command, CancellationToken.None);
        // Assert
        result.IsSuccess.ShouldBeTrue();
        mockContext.Verify(c => c.Races.Add(It.IsAny<Race>()), Times.Never);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseLastRaceStartTimeIfProvided()
    {
        // Arrange
        var mockContext = new Mock<IApplicationDbContext>();
        var mockDateTimeProvider = new Mock<IDateTimeProvider>();
        var mockCache = new Mock<IDistributedCache>();
        var mockRaceFactory = new Mock<IRaceFactory>();
        var mockMessagePublisher = new Mock<IMessagePublisher>();
        var now = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var createdRaces = new List<Race>();
        mockRaceFactory.Setup(f => f.Create(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<double>()))
            .Returns<DateTime, int, double>((start, runners, margin) => new Race(Guid.NewGuid(), [0.5, 0.3, 0.2], start, now, RaceStatus.Open));
        mockContext.Setup(c => c.Races.Add(It.IsAny<Race>())).Callback<Race>(r => createdRaces.Add(r));
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mockCache.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new CreateRaceCommandHandler(mockContext.Object, mockDateTimeProvider.Object, mockCache.Object, mockRaceFactory.Object, mockMessagePublisher.Object);
        var command = new CreateRaceCommand
        {
            LastRaceStartTime = now,
            AmountOfRacesToCreate = 2,
            TimeBetweenRaces = 30,
            NumberOfRunners = 3,
            BookmakerMargin = 0.1
        };
        // Act
        Result result = await handler.Handle(command, CancellationToken.None);
        // Assert
        result.IsSuccess.ShouldBeTrue();
        createdRaces[0].StartTime.ShouldBe(now.AddSeconds(30));
        createdRaces[1].StartTime.ShouldBe(now.AddSeconds(60));
    }
}
