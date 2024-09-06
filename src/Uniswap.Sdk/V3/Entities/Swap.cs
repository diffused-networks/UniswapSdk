using Uniswap.Sdk.Core.Entities;
using Uniswap.Sdk.Core.Entities.Fractions;

namespace Uniswap.Sdk.V3.Entities;

public class Swap<TInput, TOutput> where TInput : BaseCurrency where TOutput : BaseCurrency
{
    public Swap(Route<TInput, TOutput> route, CurrencyAmount<TInput> inputAmount, CurrencyAmount<TOutput> outputAmount)
    {
        Route = route;
        InputAmount = inputAmount;
        OutputAmount = outputAmount;
    }

    public Route<TInput, TOutput> Route { get; }
    public CurrencyAmount<TInput> InputAmount { get; }
    public CurrencyAmount<TOutput> OutputAmount { get; }
}