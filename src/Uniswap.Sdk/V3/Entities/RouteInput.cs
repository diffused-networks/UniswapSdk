using Uniswap.Sdk.Core.Entities;
using Uniswap.Sdk.Core.Entities.Fractions;

namespace Uniswap.Sdk.V3.Entities;

public class RouteInput<TInput, TOutput> where TOutput : BaseCurrency where TInput : BaseCurrency
{
    public required Route<TInput, TOutput> Route { get; init; }
    public required CurrencyAmount<TInput> InputAmount { get; init; }
    public required CurrencyAmount<TOutput> OutputAmount { get; init; }
}