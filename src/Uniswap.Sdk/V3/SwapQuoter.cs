using System.Numerics;
using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Uniswap.Sdk.Core;
using Uniswap.Sdk.Core.Entities;
using Uniswap.Sdk.Core.Entities.Fractions;
using Uniswap.Sdk.V3.Entities;
using Uniswap.Sdk.V3.Utils;

namespace Uniswap.Sdk.V3;

public abstract class SwapQuoter
{
    public static ABIEncode V1INTERFACE = new ABIEncode(); // Initialize with IQuoter ABI
    public static ABIEncode V2INTERFACE = new ABIEncode(); // Initialize with IQuoterV2 ABI

    public static NonfungiblePositionManager.MethodParameters QuoteCallParameters<TInput, TOutput>(
        Route<TInput, TOutput> route,
        CurrencyAmount<BaseCurrency> amount,
        TradeType tradeType,
        QuoteOptions options = null) where TInput : BaseCurrency where TOutput : BaseCurrency
    {
        options = options ?? new QuoteOptions();
        bool singleHop = route.Pools.Count == 1;
        string quoteAmount = amount.Quotient.ToHex(false);
        string calldata;
        ABIEncode swapInterface = options.UseQuoterV2 ? V2INTERFACE : V1INTERFACE;

        if (singleHop)
        {
            var baseQuoteParams = new
            {
                tokenIn = ((dynamic)route.TokenPath[0]).Address,
                tokenOut = ((dynamic)route.TokenPath[1]).Address,
                fee = (int)route.Pools[0].Fee,
                sqrtPriceLimitX96 = (options.SqrtPriceLimitX96 ?? BigInteger.Zero).ToHex(false)

            };

            object quoteParams;
            if (tradeType == TradeType.EXACT_INPUT)
            {
                quoteParams = new
                {
                    baseQuoteParams.tokenIn,
                    baseQuoteParams.tokenOut,
                    baseQuoteParams.fee,
                    baseQuoteParams.sqrtPriceLimitX96,
                    amountIn = quoteAmount
                };
            }
            else
            {
                quoteParams = new
                {
                    baseQuoteParams.tokenIn,
                    baseQuoteParams.tokenOut,
                    baseQuoteParams.fee,
                    baseQuoteParams.sqrtPriceLimitX96,
                    amount = quoteAmount
                };
            }

            string tradeTypeFunctionName = tradeType == TradeType.EXACT_INPUT ? "quoteExactInputSingle" : "quoteExactOutputSingle";
            //calldata = swapInterface.GetFunctionEncoded(tradeTypeFunctionName, quoteParams);
        }
        else
        {
            if (options.SqrtPriceLimitX96.HasValue)
            {
                throw new InvalidOperationException("MULTIHOP_PRICE_LIMIT");
            }

            string path = EncodeRouteToPath(route, tradeType == TradeType.EXACT_OUTPUT);
            string tradeTypeFunctionName = tradeType == TradeType.EXACT_INPUT ? "quoteExactInput" : "quoteExactOutput";
            //calldata = swapInterface.GetFunctionEncoded(tradeTypeFunctionName, path, quoteAmount);
        }

        return new NonfungiblePositionManager.MethodParameters
        {
            //Calldata = calldata,
            Value = BigInteger.Zero.ToHex(false)
        };
    }

    private static string EncodeRouteToPath<TInput, TOutput>(Route<TInput, TOutput> route, bool exactOutput)
        where TInput : BaseCurrency where TOutput : BaseCurrency
    {
        // Implement the encoding logic here
        throw new NotImplementedException();
    }


    public class QuoteOptions
    {
        public BigInteger? SqrtPriceLimitX96 { get; set; }
        public bool UseQuoterV2 { get; set; }
    }
}