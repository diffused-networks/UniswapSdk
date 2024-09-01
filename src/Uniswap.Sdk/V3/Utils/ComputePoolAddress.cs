using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using Uniswap.Sdk.Core;
using Uniswap.Sdk.Core.Entities;
using Uniswap.Sdk.Core.Utils;

namespace Uniswap.Sdk.V3.Utils;

public static class ComputePoolAddress
{
    public static string Compute(
        string factoryAddress,
        Token tokenA,
        Token tokenB,
        Constants.FeeAmount fee,
        string? initCodeHashManualOverride = null,
        ChainId? chainId = null)
    {
        var (token0, token1) = tokenA.SortsBefore(tokenB) ? (tokenA, tokenB) : (tokenB, tokenA);

        var abiEncoder = new ABIEncode();
        var encodedData = abiEncoder.GetABIEncoded(
            new ABIValue("address", token0.Address),
            new ABIValue("address", token1.Address),
            new ABIValue("uint24", (int)fee)
        );

        var salt = Sha3Keccack.Current.CalculateHash(encodedData);
        var initCodeHash = initCodeHashManualOverride ?? Constants.PoolInitCodeHash(chainId);

        switch (chainId)
        {
            //case ChainId.ZKSYNC:
            //    return ComputeZksyncCreate2Address(factoryAddress, initCodeHash, salt);
            default:
                return AddressValidator.GetCreate2Address(factoryAddress, salt, initCodeHash.HexToByteArray());
        }
    }



}