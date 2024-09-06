using System.Numerics;

// ReSharper disable InconsistentNaming


namespace Uniswap.Sdk.V3.Utils;

public static class SqrtPriceMath
{
    private static readonly BigInteger MaxUint160 = BigInteger.Pow(2, 160) - BigInteger.One;

    private static BigInteger MultiplyIn256(BigInteger x, BigInteger y)
    {
        var product = BigInteger.Multiply(x, y);
        return product & Constants.MaxUint256;
    }

    private static BigInteger AddIn256(BigInteger x, BigInteger y)
    {
        var sum = BigInteger.Add(x, y);
        return sum & Constants.MaxUint256;
    }

    public static BigInteger GetAmount0Delta(BigInteger sqrtRatioAX96, BigInteger sqrtRatioBX96, BigInteger liquidity, bool roundUp)
    {
        if (sqrtRatioAX96 > sqrtRatioBX96)
        {
            (sqrtRatioAX96, sqrtRatioBX96) = (sqrtRatioBX96, sqrtRatioAX96);
        }

        var numerator1 = liquidity << 96;
        var numerator2 = sqrtRatioBX96 - sqrtRatioAX96;

        return roundUp
            ? FullMath.MulDivRoundingUp(FullMath.MulDivRoundingUp(numerator1, numerator2, sqrtRatioBX96), Constants.ONE, sqrtRatioAX96)
            : BigInteger.Divide(BigInteger.Divide(BigInteger.Multiply(numerator1, numerator2), sqrtRatioBX96), sqrtRatioAX96);
    }

    public static BigInteger GetAmount1Delta(BigInteger sqrtRatioAX96, BigInteger sqrtRatioBX96, BigInteger liquidity, bool roundUp)
    {
        if (sqrtRatioAX96 > sqrtRatioBX96)
        {
            (sqrtRatioAX96, sqrtRatioBX96) = (sqrtRatioBX96, sqrtRatioAX96);
        }

        return roundUp
            ? FullMath.MulDivRoundingUp(liquidity, sqrtRatioBX96 - sqrtRatioAX96, Constants.Q96)
            : BigInteger.Divide(BigInteger.Multiply(liquidity, BigInteger.Subtract(sqrtRatioBX96, sqrtRatioAX96)), Constants.Q96);
    }

    public static BigInteger GetNextSqrtPriceFromInput(BigInteger sqrtPX96, BigInteger liquidity, BigInteger amountIn, bool zeroForOne)
    {
        if (sqrtPX96 <= Constants.ZERO || liquidity <= Constants.ZERO)
        {
            throw new ArgumentException("Invalid input");
        }

        return zeroForOne
            ? GetNextSqrtPriceFromAmount0RoundingUp(sqrtPX96, liquidity, amountIn, true)
            : GetNextSqrtPriceFromAmount1RoundingDown(sqrtPX96, liquidity, amountIn, true);
    }

    public static BigInteger GetNextSqrtPriceFromOutput(BigInteger sqrtPX96, BigInteger liquidity, BigInteger amountOut, bool zeroForOne)
    {
        if (sqrtPX96 <= Constants.ZERO || liquidity <= Constants.ZERO)
        {
            throw new ArgumentException("Invalid input");
        }

        return zeroForOne
            ? GetNextSqrtPriceFromAmount1RoundingDown(sqrtPX96, liquidity, amountOut, false)
            : GetNextSqrtPriceFromAmount0RoundingUp(sqrtPX96, liquidity, amountOut, false);
    }

    private static BigInteger GetNextSqrtPriceFromAmount0RoundingUp(BigInteger sqrtPX96, BigInteger liquidity, BigInteger amount, bool add)
    {
        if (amount == Constants.ZERO)
        {
            return sqrtPX96;
        }

        var numerator1 = liquidity << 96;

        if (add)
        {
            var product = MultiplyIn256(amount, sqrtPX96);

            //Console.WriteLine(BigInteger.Divide(product, amount));

            if (BigInteger.Divide(product, amount) == sqrtPX96)
            {
                var denominator = AddIn256(numerator1, product);


                if (denominator >= numerator1)
                {
                    return FullMath.MulDivRoundingUp(numerator1, sqrtPX96, denominator);
                }
            }

            return FullMath.MulDivRoundingUp(numerator1, Constants.ONE, BigInteger.Add(BigInteger.Divide(numerator1, sqrtPX96), amount));
        }
        else
        {
            var product = MultiplyIn256(amount, sqrtPX96);

            if (BigInteger.Divide(product, amount) != sqrtPX96 || numerator1 <= product)
            {
                throw new ArgumentException("Invalid calculation");
            }

            var denominator = numerator1 - product;
            return FullMath.MulDivRoundingUp(numerator1, sqrtPX96, denominator);
        }
    }

    private static BigInteger GetNextSqrtPriceFromAmount1RoundingDown(BigInteger sqrtPX96, BigInteger liquidity, BigInteger amount, bool add)
    {
        if (add)
        {
            var quotient = amount <= MaxUint160
                ? BigInteger.Divide(amount << 96, liquidity)
                : BigInteger.Divide(BigInteger.Multiply(amount, Constants.Q96), liquidity);

            return sqrtPX96 + quotient;
        }
        else
        {
            var quotient = FullMath.MulDivRoundingUp(amount, Constants.Q96, liquidity);

            if (sqrtPX96 <= quotient)
            {
                throw new ArgumentException("Invalid calculation");
            }

            return sqrtPX96 - quotient;
        }
    }
}

// Note: The FullMath class is not provided in the original code, so you'll need to implement it separately.