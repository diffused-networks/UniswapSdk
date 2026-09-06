<h1 align="center">UniswapSharp</h1>

<p align="center">
  <strong>The Uniswap SDK for .NET.</strong><br>
  A faithful C# port of the official <a href="https://github.com/Uniswap/sdks"><code>Uniswap/sdks</code></a> monorepo —
  verified to the digit against upstream's own test vectors.
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/UniswapSharp"><img alt="NuGet" src="https://img.shields.io/nuget/v/UniswapSharp.svg?logo=nuget"></a>
  <a href="https://www.nuget.org/packages/UniswapSharp"><img alt="Downloads" src="https://img.shields.io/nuget/dt/UniswapSharp.svg?logo=nuget"></a>
  <a href="https://github.com/grinidx/UniswapSharp/actions/workflows/ci.yml"><img alt="CI" src="https://github.com/grinidx/UniswapSharp/actions/workflows/ci.yml/badge.svg"></a>
  <a href="https://github.com/grinidx/UniswapSharp/actions/workflows/codeql.yml"><img alt="CodeQL" src="https://github.com/grinidx/UniswapSharp/actions/workflows/codeql.yml/badge.svg"></a>
  <img alt=".NET" src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white">
  <a href="LICENSE"><img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-blue.svg"></a>
</p>

<p align="center">
  <a href="#install">Install</a> ·
  <a href="#quickstart">Quickstart</a> ·
  <a href="#examples">Examples</a> ·
  <a href="#whats-in-the-box">What's in the box</a> ·
  <a href="#design-notes">Design notes</a> ·
  <a href="docs/PORTING.md">Porting guide</a> ·
  <a href="CHANGELOG.md">Changelog</a>
</p>

---

Model currencies, tokens, pools, positions, routes and trades. Run the tick, sqrt-price, swap and
liquidity math. Build calldata for the V3/V4 periphery and the Universal Router. Hash and sign
Permit2 permits and UniswapX orders.

All of it offline and deterministic — **no RPC required**, and **no floating point anywhere in
protocol math** (exact `BigInteger` / `BigRational` throughout).

Namespaces and file names deliberately mirror the upstream TypeScript, so the two read side by side.

> [!NOTE]
> **v2.0.0** · **1,920 tests, 0 failing** on Linux / Windows / macOS, zero compiler warnings ·
> at upstream parity with [`Uniswap/sdks@35c4e35`](https://github.com/Uniswap/sdks/commit/35c4e35aca9e22169ce17d7106e7fc5f27ccd03d) (2026-09-01).
> Upgrading from 1.x? See [Migrating from 1.0.0](CHANGELOG.md#migrating-from-100) — two call sites move.

## Install

```bash
dotnet add package UniswapSharp
```

Targets **.NET 10** (`net10.0`). One package, no plugins — every namespace below ships in it.

## Quickstart

```csharp
using System.Numerics;
using UniswapSharp.Core.Entities;
using UniswapSharp.V3.Entities;
using UniswapSharp.V3.Utils;
using FeeAmount = UniswapSharp.V3.Constants.FeeAmount;

var usdc = new Token(1, "0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48", 6, "USDC", "USD Coin");
var weth = new Token(1, "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2", 18, "WETH", "Wrapped Ether");

// Deterministic V3 pool address — CREATE2, no network call.
Console.WriteLine(Pool.GetAddress(usdc, weth, FeeAmount.MEDIUM));

// Construct a pool at ~2000 USDC per WETH and read the price back.
var sqrtPriceX96 = EncodeSqrtRatioX96.Encode(
    BigInteger.Parse("1000000000000000000000"),   // 1,000 WETH
    BigInteger.Parse("2000000000000"));           // 2,000,000 USDC
var pool = new Pool(usdc, weth, FeeAmount.MEDIUM, sqrtPriceX96,
    BigInteger.Zero, TickMath.GetTickAtSqrtRatio(sqrtPriceX96));

Console.WriteLine($"1 WETH = {pool.PriceOf(weth).ToSignificant(6)} USDC");
```

```
0x8ad599c3A0ff1De082011EFDDc58f1908eb6e6D8
1 WETH = 2000 USDC
```

## Examples

<details open>
<summary><strong>Build swap calldata for the V3 router</strong></summary>

Quote a trade and produce a signable transaction. A pool needs tick data to be swapped against
offline — here a single full-range position stands in for the real tick list you'd load from a
subgraph or `TickLens`.

```csharp
using System.Numerics;
using UniswapSharp.Core;
using UniswapSharp.Core.Entities;
using UniswapSharp.Core.Entities.Fractions;
using UniswapSharp.V3;
using UniswapSharp.V3.Entities;
using UniswapSharp.V3.Utils;
using FeeAmount = UniswapSharp.V3.Constants.FeeAmount;

var usdc = new Token(1, "0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48", 6, "USDC", "USD Coin");
var weth = new Token(1, "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2", 18, "WETH", "Wrapped Ether");

var sqrtPriceX96 = EncodeSqrtRatioX96.Encode(
    BigInteger.Parse("1000000000000000000000"), BigInteger.Parse("2000000000000"));
var liquidity = BigInteger.Parse("100000000000000000000");
int spacing = UniswapSharp.V3.Constants.TICK_SPACINGS[FeeAmount.MEDIUM];

var ticks = new List<Tick>
{
    new(NearestUsableTick.Find(TickMath.MIN_TICK, spacing), liquidity, liquidity),
    new(NearestUsableTick.Find(TickMath.MAX_TICK, spacing), -liquidity, liquidity),
};

var pool = new Pool(usdc, weth, FeeAmount.MEDIUM, sqrtPriceX96, liquidity,
    TickMath.GetTickAtSqrtRatio(sqrtPriceX96), ticks);

var trade = await Trade<Token, Token>.FromRoute(
    new Route<Token, Token>([pool], usdc, weth),
    CurrencyAmount<Token>.FromRawAmount(usdc, 1_000_000_000),   // exactly 1,000 USDC
    TradeType.EXACT_INPUT);

var slippage = new Percent(50, 10_000);   // 0.50%
var parameters = SwapRouter.SwapCallParameters(trade, new Staker.SwapOptions
{
    SlippageTolerance = slippage,
    Recipient = "0x0000000000000000000000000000000000000003",
    Deadline   = 2_000_000_000,
});

Console.WriteLine($"quoted out   : {trade.OutputAmount.ToSignificant(6)} WETH");
Console.WriteLine($"min out      : {trade.MinimumAmountOut(slippage).ToSignificant(6)} WETH");
Console.WriteLine($"price impact : {trade.PriceImpact.ToSignificant(3)}%");
Console.WriteLine($"selector     : {parameters.Calldata[..10]}");   // exactInputSingle
```

```
quoted out   : 0.498499 WETH
min out      : 0.496019 WETH
price impact : 0.3%
selector     : 0x414bf389
```

`parameters.Calldata` and `parameters.Value` go straight into an `eth_sendTransaction`.
</details>

<details>
<summary><strong>Sign a Permit2 permit</strong></summary>

```csharp
using UniswapSharp.Permit2;

var permit = new PermitTransferFrom(
    Permitted: new TokenPermissions("0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48", 1_000_000_000),
    Spender:   "0x0000000000000000000000000000000000000003",
    Nonce:     0,
    Deadline:  2_000_000_000);

const string permit2 = "0x000000000022D473030F116dDEE9F6B43aC78BA3";

// The EIP-712 digest a wallet signs.
Console.WriteLine(SignatureTransfer.Hash(permit, permit2, chainId: 1));

// Or the full typed-data payload, for eth_signTypedData_v4.
PermitData data = SignatureTransfer.GetPermitData(permit, permit2, chainId: 1);
```

```
0x4d55f96e4cee5574a9b358c7ec7378b3b21a27b94c5979fced600845ac5e8d12
```

The typed-data encoder is a byte-exact port of ethers' `_TypedDataEncoder`, pinned against
upstream's EIP-712 hash vectors.
</details>

<details>
<summary><strong>More</strong></summary>

The test suite under [`test/UniswapSharp.Testing/`](test/UniswapSharp.Testing) mirrors the source
tree and doubles as a worked example library — every case is a round-trip against an upstream
vector. Good entry points:

| I want to… | Look at |
|---|---|
| Build V3 swap calldata | `V3/SwapRouterTests.cs`, `V3/SwapRouterMultiTests.cs` |
| Mint / manage a V3 position | `V3/NonfungiblePositionManagerTests.cs` |
| Work with V4 pools and hooks | `V4/Entities/PoolTests.cs`, `V4/Utils/HookTests.cs` |
| Plan a V4 action sequence | `V4/Utils/V4PlannerTests.cs`, `V4/PositionManagerTests.cs` |
| Route across v2 + v3 + v4 | `Router/Entities/MixedRoute/MixedRouteTests.cs` |
| Drive the Universal Router | `UniversalRouter/EncodeSwapsTests.cs`, `UniversalRouter/RouterTradeAdapterTests.cs` |
| Build and hash a UniswapX order | `UniswapX/DutchOrderTests.cs`, `UniswapX/OrderUtilsTests.cs` |
| Sign Permit2 | `Permit2/SignatureTransferTests.cs`, `Permit2/AllowanceTransferTests.cs` |
| Launch a token | `LiquidityLauncher/QuickLaunchTests.cs`, `LiquidityLauncher/InstantLaunchTests.cs` |

</details>

## What's in the box

Every namespace maps one-to-one to an upstream package.

| Namespace | Upstream package | What's in it |
|---|---|---|
| `UniswapSharp.Core` | `@uniswap/sdk-core` | `Token`, `Ether`, `Fraction`/`Percent`/`Price`/`CurrencyAmount`, chain + address registries, WETH9 |
| `UniswapSharp.V2` | `@uniswap/v2-sdk` | `Pair` (CREATE2 + 997/1000 fee math), `Route`, `Trade`, `Router` |
| `UniswapSharp.V3` | `@uniswap/v3-sdk` | `Pool`, `Position`, `Route`, `Trade`, `Tick`; tick / sqrt-price / swap / liquidity math; periphery calldata |
| `UniswapSharp.V4` | `@uniswap/v4-sdk` | currency-based `Pool`/`Position`/`Route`/`Trade`, hook permissions, `V4Planner`, `PositionManager` |
| `UniswapSharp.Router` | `@uniswap/router-sdk` | mixed v2+v3+v4 routes, aggregated `Trade`, SwapRouter02 calldata |
| `UniswapSharp.UniversalRouter` | `@uniswap/universal-router-sdk` | `RoutePlanner`, `RouterTradeAdapter`, `SwapRouter`, direct transfers, signed-route EIP-712 |
| `UniswapSharp.UniswapX` | `@uniswap/uniswapx-sdk` | Dutch / Priority / Relay / V3 / Hybrid orders, decay math, builders, trades, Permit2-witness hashing |
| `UniswapSharp.Permit2` | `@uniswap/permit2-sdk` | `SignatureTransfer`, `AllowanceTransfer`, byte-exact EIP-712 typed-data encoder |
| `UniswapSharp.SmartWallet` | `@uniswap/smart-wallet-sdk` | ERC-7821 call planners and encoders |
| `UniswapSharp.LiquidityLauncher` | `@uniswap/liquidity-launcher-sdk` | launch config math, CREATE2 poolId / salts, quick-launch and instant-launch surfaces |
| `UniswapSharp.Flashtestations` | `@uniswap/flashtestations-sdk` | TEE workload ID (keccak) + block verification, injectable RPC |
| `UniswapSharp.Tamperproof` | `@uniswap/tamperproof-transactions` | EIP-7754 sign / verify (RSA, ECDSA, Ed25519), canonical JSON, injectable DNS-over-HTTPS |

## Design notes

**Exact arithmetic, always.** Protocol math runs on `System.Numerics.BigInteger` and
`BigRational`. There is no `double` or `decimal` anywhere a price, tick, or amount is computed —
rounding and tick boundaries agree with upstream to the last digit.

**Verified against upstream, not against ourselves.** Every ported module brings its upstream
`.test.ts` cases across as xUnit tests. Where upstream ships golden calldata, addresses, or
EIP-712 digests, those are pinned verbatim — including on-chain-observed values such as launcher
dispatcher selectors and real pool ids.

**Offline and deterministic.** The library builds calldata and computes math; it never opens a
socket. The few upstream modules that genuinely need a live node (quoters, validators, event
watchers) sit behind injectable interfaces or are deliberately out of scope —
[`docs/PORTING.md`](docs/PORTING.md) lists every one and why.

**Nullable-clean.** Nullable reference types are enabled and the solution builds with zero
warnings. Option and parameter types carry explicit null contracts derived from the upstream
TypeScript interfaces.

## Versioning

[Semantic Versioning](https://semver.org/). The public API surface, the calldata a given input
produces, and the numeric output of protocol math are all covered — a change to any of them that
would alter a caller's bytes or numbers is a major.

Releases are tagged `v*` and published to NuGet from CI via
[Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing) (OIDC,
keyless — no long-lived API key exists). Packages ship with SourceLink and symbols, so you can
step into the source from your debugger.

## Building from source

```bash
git clone https://github.com/grinidx/UniswapSharp.git
cd UniswapSharp
dotnet build -c Release
dotnet test  -c Release
```

Requires the **.NET 10 SDK**. Tests run on Microsoft.Testing.Platform (selected by `global.json`);
CI runs the full suite on ubuntu, windows and macOS, plus CodeQL.

## Contributing

Contributions are welcome — see [CONTRIBUTING.md](CONTRIBUTING.md). The short version: work
test-first, port the upstream `.test.ts` cases alongside the code, match upstream to the digit, and
land everything through a PR with green CI.

[`docs/PORTING.md`](docs/PORTING.md) is the map: the file-by-file correspondence to upstream, the
re-sync workflow, and every intentional divergence.

Please also read the [Code of Conduct](CODE_OF_CONDUCT.md) and the
[Security Policy](SECURITY.md).

## License and attribution

MIT — see [LICENSE](LICENSE).

UniswapSharp is a derivative work of the MIT-licensed
[`Uniswap/sdks`](https://github.com/Uniswap/sdks) (© Uniswap Labs). It is an independent port and
is **not affiliated with, endorsed by, or supported by Uniswap Labs**.

> [!WARNING]
> This software is provided as is, without warranty. It constructs transaction calldata that moves
> real funds. Verify calldata, amounts and slippage independently — ideally by simulating against a
> fork — before broadcasting anything on-chain.
