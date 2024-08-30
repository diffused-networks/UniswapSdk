﻿using Uniswap.Sdk.Core.Entities;
using Uniswap.Sdk.Core.Entities.Fractions;

namespace Uniswap.Sdk.Core.Utils;

/// <summary>
/// Returns the percent difference between the mid price and the execution price, i.e. price impact.
/// </summary>
/// <typeparam name="TBase">The base currency type.</typeparam>
/// <typeparam name="TQuote">The quote currency type.</typeparam>
/// <param name="midPrice">Mid price before the trade.</param>
/// <param name="inputAmount">The input amount of the trade.</param>
/// <param name="outputAmount">The output amount of the trade.</param>
/// <returns>The price impact as a Percent.</returns>
/// 
public static class PriceImpact
{
        public static Percent Compute<TBase, TQuote>(
        Price<TBase, TQuote> midPrice,
        CurrencyAmount<TBase> inputAmount,
        CurrencyAmount<TQuote> outputAmount)
        where TBase : BaseCurrency
        where TQuote : BaseCurrency
    {
        var quotedOutputAmount = midPrice.Quote(inputAmount);
        // calculate price impact := (exactQuote - outputAmount) / exactQuote
        var priceImpact = quotedOutputAmount.Subtract(outputAmount).Divide(quotedOutputAmount);
        return new Percent(priceImpact.Numerator, priceImpact.Denominator);
    }

}