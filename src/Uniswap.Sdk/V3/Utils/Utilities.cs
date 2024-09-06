using System.Numerics;

namespace Uniswap.Sdk.V3.Utils;

public static class Utilities
{
    /// <summary>
    ///     Converts a big int to a hex string
    /// </summary>
    /// <param name="bigintIsh">The BigInteger to convert</param>
    /// <returns>The hex encoded calldata</returns>
    public static string ToHex(BigInteger bigintIsh)
    {
        var hex = bigintIsh.ToString("X");
        if (hex.Length % 2 != 0)
        {
            hex = $"0{hex}";
        }

        return $"0x{hex}";
    }
}