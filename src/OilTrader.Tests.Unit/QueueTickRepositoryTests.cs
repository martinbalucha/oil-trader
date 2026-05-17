using OilTrader.Contracts;
using OilTrader.Domain;

namespace OilTrader.Tests.Unit;

public class QueueTickRepositoryTests
{
    private readonly QueueTickRepository _repository = new();

    [Fact]
    public async Task AddAsync_AddsSingle_OneItemPresent()
    {
        // Arrange
        var tick = Tick.Create(Symbol.From("XBIUSD"), Price.From(100), Price.From(110), DateTimeOffset.Now, 100);

        // Act
        await _repository.AddAsync(tick);

        // Assert
        var items = await _repository.GetRecentAsync(100).ToListAsync();
        Assert.Single(items);

        var retrievedTick = items.Single();
        Assert.Equivalent(tick, retrievedTick);
    }

    [Fact]
    public async Task AddAsync_AddsExceedingSize_KeepsNewestUpToMaxSize()
    {
        // Act
        for (var i = 0; i < QueueTickRepository.MaxSize + 5; i++)
        {
            var tick = Tick.Create(
                Symbol.From("XBIUSD"),
                Price.From(100),
                Price.From(110),
                DateTimeOffset.UtcNow,
                i);
            await _repository.AddAsync(tick);
        }

        // Assert
        var items = await _repository.GetRecentAsync(int.MaxValue).ToListAsync();
        Assert.Equal(QueueTickRepository.MaxSize, items.Count);
        Assert.Equal(5L, items.First().Volume);
        Assert.Equal(QueueTickRepository.MaxSize + 4L, items.Last().Volume);
    }
}
