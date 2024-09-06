using Uniswap.Sdk.Core.Entities;
using Uniswap.Sdk.Core.Entities.Fractions;
using Uniswap.Sdk.Core.Utils;

namespace Uniswap.Sdk.Testing.Core.Utils;

public class ComputePriceImpactTests(ITestOutputHelper output)
{
    private const string ADDRESS_ZERO = "0x0000000000000000000000000000000000000000";
    private const string ADDRESS_ONE = "0x0000000000000000000000000000000000000001";

    private readonly ITestOutputHelper output = output;

    private readonly Token t0 = new(1, ADDRESS_ZERO, 18);
    private readonly Token t1 = new(1, ADDRESS_ONE, 18);

    [Fact]
    public void ComputePriceImpact_IsCorrectForZero()
    {
        var result = PriceImpact.Compute(
            new Price<Ether, Token>(Ether.OnChain(1), t0, 10, 100),
            CurrencyAmount<Ether>.FromRawAmount(Ether.OnChain(1), 10),
            CurrencyAmount<Token>.FromRawAmount(t0, 100)
        );
        var expect = new Percent(0, 10000);
        Assert.Equal(expect, result);
    }

    [Fact]
    public void ComputePriceImpact_IsCorrectForHalfOutput()
    {
        var result = PriceImpact.Compute(
            new Price<Token, Token>(t0, t1, 10, 100),
            CurrencyAmount<Token>.FromRawAmount(t0, 10),
            CurrencyAmount<Token>.FromRawAmount(t1, 50)
        );

        var expect = new Percent(5000, 10000);
        Assert.Equal(expect, result);
    }

    [Fact]
    public void ComputePriceImpact_IsNegativeForMoreOutput()
    {
        var result = PriceImpact.Compute(
            new Price<Token, Token>(t0, t1, 10, 100),
            CurrencyAmount<Token>.FromRawAmount(t0, 10),
            CurrencyAmount<Token>.FromRawAmount(t1, 200)
        );
        var expect = new Percent(-10000, 10000);
        Assert.Equal(expect, result);
    }
}