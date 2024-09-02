using Uniswap.Sdk.V3.Entities;
using Uniswap.Sdk.V3.Utils;

namespace Uniswap.Sdk.Testing.V3.Entities;

public class TickTests
{
    [Fact]
    public void Constructor_ThrowsIfTickIsBelowMinTick()
    {
        Assert.Throws<ArgumentException>(() => new Tick(TickMath.MIN_TICK - 1, 0, 0));
    }

    [Fact]
    public void Constructor_ThrowsIfTickIsAboveMaxTick()
    {
        Assert.Throws<ArgumentException>(() => new Tick(TickMath.MAX_TICK + 1, 0, 0));
    }
}