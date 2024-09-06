using Nethereum.Contracts;
using Newtonsoft.Json.Linq;

namespace Uniswap.Sdk.V3;

public abstract class Multicall
{
    public static Contract INTERFACE;

    static Multicall()
    {
        var jsonContent = File.ReadAllText("IMulticall.json");
        var jsonObject = JObject.Parse(jsonContent);
        var abiArray = (JArray)jsonObject["abi"];
        var abiString = abiArray.ToString();

        // INTERFACE = new Contract(null, abiString);
    }

    private Multicall()
    {
    }

    public static string EncodeMulticall(string calldata)
    {
        return EncodeMulticall(new[] { calldata });
    }

    public static string EncodeMulticall(IEnumerable<string> calldatas)
    {
        var calldataList = new List<string>(calldatas);
        if (calldataList.Count == 1)
        {
            return calldataList[0];
        }

        var function = INTERFACE.GetFunction("multicall");
        return function.GetData(calldataList);
    }
}