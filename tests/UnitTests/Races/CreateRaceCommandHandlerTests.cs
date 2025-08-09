using Application.Races.Create;
using Application.Abstractions.Data;
using Domain.Races;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using SharedKernel;
using Application.Abstractions.Messaging;
using Domain.Abstractions;

namespace UnitTests.Races;

public class CreateRaceCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateSingleRaceAndReturnSuccess()
    {
        // Arrange
        var mockContext = new Mock<IApplicationDbContext>();
        var mockCache = new Mock<IDistributedCache>();
        var mockBatchFactory = new Mock<IRaceBatchFactory>();
        var mockMessagePublisher = new Mock<IMessagePublisher>();

        var mockRaceSet = new Mock<DbSet<Race>>();
        var addedRaces = new List<Race>();
        mockRaceSet.Setup(s => s.AddRange(It.IsAny<IEnumerable<Race>>()))
            .Callback<IEnumerable<Race>>(rs => addedRaces.AddRange(rs));
        mockRaceSet.Setup(s => s.AddRange(It.IsAny<Race[]>()))
            .Callback<Race[]>(rs => addedRaces.AddRange(rs));
        mockContext.Setup(c => c.Races).Returns(mockRaceSet.Object);

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        mockCache.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var now = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var race = new Race(Guid.NewGuid(), [0.5, 0.3, 0.2], now.AddHours(1), now, RaceStatus.Open);
        var batch = new List<Race> { race };
        mockBatchFactory.Setup(f => f.CreateBatch(null, 1, 3, 0.1, 60))
            .Returns(batch);

        var handler = new CreateRaceCommandHandler(
            mockContext.Object,
            mockCache.Object,
            mockBatchFactory.Object,
            mockMessagePublisher.Object);

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
        addedRaces.Count.ShouldBe(1);
        addedRaces[0].ShouldBe(race);
        mockRaceSet.Verify(s => s.AddRange(It.IsAny<IEnumerable<Race>>()), Times.Once);
        mockCache.Verify(c => c.RemoveAsync(CacheKeys.UpcomingRaces, It.IsAny<CancellationToken>()), Times.Once);
        mockBatchFactory.Verify(f => f.CreateBatch(null, 1, 3, 0.1, 60), Times.Once);
        mockMessagePublisher.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<RaceCreatedMessage>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateMultipleRaces_AndPassCorrectParametersToFactory()
    {
        // Arrange
        var mockContext = new Mock<IApplicationDbContext>();
        var mockCache = new Mock<IDistributedCache>();
        var mockBatchFactory = new Mock<IRaceBatchFactory>();
        var mockMessagePublisher = new Mock<IMessagePublisher>();

        var mockRaceSet = new Mock<DbSet<Race>>();
        var addedRaces = new List<Race>();
        mockRaceSet.Setup(s => s.AddRange(It.IsAny<IEnumerable<Race>>()))
            .Callback<IEnumerable<Race>>(rs => addedRaces.AddRange(rs));
        mockRaceSet.Setup(s => s.AddRange(It.IsAny<Race[]>()))
            .Callback<Race[]>(rs => addedRaces.AddRange(rs));
        mockContext.Setup(c => c.Races).Returns(mockRaceSet.Object);

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        mockCache.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var now = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var batch = new List<Race>
        {
            new Race(Guid.NewGuid(), [0.5, 0.3, 0.2], now.AddSeconds(60), now, RaceStatus.Open),
            new Race(Guid.NewGuid(), [0.5, 0.3, 0.2], now.AddSeconds(120), now, RaceStatus.Open),
            new Race(Guid.NewGuid(), [0.5, 0.3, 0.2], now.AddSeconds(180), now, RaceStatus.Open)
        };
        mockBatchFactory.Setup(f => f.CreateBatch(now, 3, 3, 0.1, 60))
            .Returns(batch);

        var handler = new CreateRaceCommandHandler(
            mockContext.Object,
            mockCache.Object,
            mockBatchFactory.Object,
            mockMessagePublisher.Object);

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
        addedRaces.Count.ShouldBe(3);
        addedRaces.ShouldBe(batch);
        mockBatchFactory.Verify(f => f.CreateBatch(now, 3, 3, 0.1, 60), Times.Once);
    }

    [Fact]
    public async Task Handle_WithZeroAmountOfRaces_ShouldNotAddAnyRace()
    {
        // Arrange
        var mockContext = new Mock<IApplicationDbContext>();
        var mockCache = new Mock<IDistributedCache>();
        var mockBatchFactory = new Mock<IRaceBatchFactory>();
        var mockMessagePublisher = new Mock<IMessagePublisher>();

        var mockRaceSet = new Mock<DbSet<Race>>();
        var addedRaces = new List<Race>();
        mockRaceSet.Setup(s => s.AddRange(It.IsAny<IEnumerable<Race>>()))
            .Callback<IEnumerable<Race>>(rs => addedRaces.AddRange(rs));
        mockContext.Setup(c => c.Races).Returns(mockRaceSet.Object);

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        mockBatchFactory.Setup(f => f.CreateBatch(null, 0, 3, 0.1, 60))
            .Returns(new List<Race>());

        var handler = new CreateRaceCommandHandler(
            mockContext.Object,
            mockCache.Object,
            mockBatchFactory.Object,
            mockMessagePublisher.Object);

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
        addedRaces.Count.ShouldBe(0);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockBatchFactory.Verify(f => f.CreateBatch(null, 0, 3, 0.1, 60), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseLastRaceStartTimeIfProvided()
    {
        // Arrange
        var mockContext = new Mock<IApplicationDbContext>();
        var mockCache = new Mock<IDistributedCache>();
        var mockBatchFactory = new Mock<IRaceBatchFactory>();
        var mockMessagePublisher = new Mock<IMessagePublisher>();

        var mockRaceSet = new Mock<DbSet<Race>>();
        mockContext.Setup(c => c.Races).Returns(mockRaceSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var now = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        mockBatchFactory.Setup(f => f.CreateBatch(now, 2, 3, 0.1, 30))
            .Returns(new List<Race>());

        var handler = new CreateRaceCommandHandler(
            mockContext.Object,
            mockCache.Object,
            mockBatchFactory.Object,
            mockMessagePublisher.Object);

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
        mockBatchFactory.Verify(f => f.CreateBatch(now, 2, 3, 0.1, 30), Times.Once);
    }
}
