using System.Numerics;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Uniswap.Sdk.Core;
using Uniswap.Sdk.Core.Entities;
using Uniswap.Sdk.Core.Entities.Fractions;
using Uniswap.Sdk.V3.Entities;
using Uniswap.Sdk.V3.Utils;
using static Uniswap.Sdk.V3.Constants;
#pragma warning disable xUnit1048

// ReSharper disable UnusedMember.Local

namespace Uniswap.Sdk.Testing.V3.Entities;

public class TradeTests
{
    private static readonly Ether ETHER = Ether.OnChain(1);
    private static readonly Token token0 = new Token(1, "0x0000000000000000000000000000000000000001", 18, "t0", "token0");
    private static readonly Token token1 = new Token(1, "0x0000000000000000000000000000000000000002", 18, "t1", "token1");
    private static readonly Token token2 = new Token(1, "0x0000000000000000000000000000000000000003", 18, "t2", "token2");
    private static readonly Token token3 = new Token(1, "0x0000000000000000000000000000000000000004", 18, "t3", "token3");

    private static Pool V2StylePool(CurrencyAmount<Token> reserve0, CurrencyAmount<Token> reserve1, FeeAmount feeAmount = FeeAmount.MEDIUM)
    {
        var sqrtRatioX96 = EncodeSqrtRatioX96.Encode(reserve1.Quotient, reserve0.Quotient);
        var liquidity = BigIntegerSquareRoot.NewtonPlusSqrt(reserve0.Quotient * reserve1.Quotient);
        return new Pool(
            reserve0.Currency,
            reserve1.Currency,
            feeAmount,
            sqrtRatioX96,
            liquidity,
            TickMath.GetTickAtSqrtRatio(sqrtRatioX96),
            new List<Tick>
            {
                new Tick
                (
                    NearestUsableTick.Find(TickMath.MIN_TICK, TICK_SPACINGS[feeAmount]),
                    liquidity,
                    liquidity
                ),
                new Tick
                (
                    NearestUsableTick.Find(TickMath.MAX_TICK, TICK_SPACINGS[feeAmount]),
                    -liquidity,
                    liquidity
                )
            }
        );
    }

    private readonly Pool pool_0_1 = V2StylePool(
        CurrencyAmount<Token>.FromRawAmount(token0, 100000),
        CurrencyAmount<Token>.FromRawAmount(token1, 100000)
    );
    private readonly Pool pool_0_2 = V2StylePool(
        CurrencyAmount<Token>.FromRawAmount(token0, 100000),
        CurrencyAmount<Token>.FromRawAmount(token2, 110000)
    );
    private readonly Pool pool_0_3 = V2StylePool(
        CurrencyAmount<Token>.FromRawAmount(token0, 100000),
        CurrencyAmount<Token>.FromRawAmount(token3, 90000)
    );
    private readonly Pool pool_1_2 = V2StylePool(
        CurrencyAmount<Token>.FromRawAmount(token1, 120000),
        CurrencyAmount<Token>.FromRawAmount(token2, 100000)
    );
    private readonly Pool pool_1_3 = V2StylePool(
        CurrencyAmount<Token>.FromRawAmount(token1, 120000),
        CurrencyAmount<Token>.FromRawAmount(token3, 130000)
    );

    private readonly Pool pool_weth_0 = V2StylePool(
        CurrencyAmount<Token>.FromRawAmount(Weth9.Tokens[1], BigInteger.Parse("100000")),
        CurrencyAmount<Token>.FromRawAmount(token0, BigInteger.Parse("100000"))
    );

    private readonly Pool pool_weth_1 = V2StylePool(
        CurrencyAmount<Token>.FromRawAmount(Weth9.Tokens[1], BigInteger.Parse("100000")),
        CurrencyAmount<Token>.FromRawAmount(token1, BigInteger.Parse("100000"))
    );

    private readonly Pool pool_weth_2 = V2StylePool(
        CurrencyAmount<Token>.FromRawAmount(Weth9.Tokens[1], BigInteger.Parse("100000")),
        CurrencyAmount<Token>.FromRawAmount(token2, BigInteger.Parse("100000"))
    );

    [Fact]
    public async void FromRoute_CanBeConstructedWithEtherAsInput()
    {
        var trade = await Trade<BaseCurrency,Token>.FromRoute(
            new Route<BaseCurrency, Token>([pool_weth_0], ETHER, token0),
            CurrencyAmount < BaseCurrency>.FromRawAmount(Ether.OnChain(1), BigInteger.Parse("10000")),
            TradeType.EXACT_INPUT
        );
        Assert.Equal(ETHER, trade.InputAmount.Currency);
        Assert.Equal(token0, trade.OutputAmount.Currency);
    }

    [Fact]
    public async void FromRoute_CanBeConstructedWithEtherAsInputForExactOutput()
    {
        var trade = await Trade<BaseCurrency, Token>.FromRoute(
            new Route<BaseCurrency, Token>([pool_weth_0], ETHER, token0),
            CurrencyAmount<Token>.FromRawAmount(token0, BigInteger.Parse("10000")),
            TradeType.EXACT_OUTPUT
        );
        Assert.Equal(ETHER, trade.InputAmount.Currency);
        Assert.Equal(token0, trade.OutputAmount.Currency);
    }

    [Fact]
    public async void FromRoute_CanBeConstructedWithEtherAsOutput()
    {
        var trade = await Trade<Token, BaseCurrency>.FromRoute(
            new Route<Token, BaseCurrency>([pool_weth_0], token0, ETHER),
            CurrencyAmount<BaseCurrency>.FromRawAmount(Ether.OnChain(1), BigInteger.Parse("10000")),
            TradeType.EXACT_OUTPUT
        );
        Assert.Equal(token0, trade.InputAmount.Currency);
        Assert.Equal(ETHER, trade.OutputAmount.Currency);
    }

    [Fact]
    public async void FromRoute_CanBeConstructedWithEtherAsOutputForExactInput()
    {
        var trade = await Trade<Token, BaseCurrency>.FromRoute(
            new Route<Token, BaseCurrency>([pool_weth_0], token0, ETHER),
            CurrencyAmount<BaseCurrency>.FromRawAmount(token0, BigInteger.Parse("10000")),
            TradeType.EXACT_INPUT
        );
        Assert.Equal(token0, trade.InputAmount.Currency);
        Assert.Equal(ETHER, trade.OutputAmount.Currency);
    }

    [Fact]
    public async void FromRoutes_CanBeConstructedWithEtherAsInputWithMultipleRoutes()
    {
        var trade = await Trade<Ether, Token>.FromRoutes<Ether>(
            [
                new(CurrencyAmount<Ether>.FromRawAmount(Ether.OnChain(1), BigInteger.Parse("10000")), new Route<Ether, Token>([pool_weth_0], ETHER, token0))
            ]
            , TradeType.EXACT_INPUT);
            
        Assert.Equal(ETHER, trade.InputAmount.Currency);
        Assert.Equal(token0, trade.OutputAmount.Currency);
    }

    [Fact]
    public async void FromRoutes_CanBeConstructedWithEtherAsInputForExactOutputWithMultipleRoutes()
    {
        var trade = await Trade<Ether, Token>.FromRoutes<Token>(

            [
                new(CurrencyAmount<Token>.FromRawAmount(token0, BigInteger.Parse("3000")),new Route<Ether, Token>([pool_weth_0], ETHER, token0)),
                new(CurrencyAmount<Token>.FromRawAmount(token0, BigInteger.Parse("7000")),new Route<Ether, Token>([pool_weth_1, pool_0_1], ETHER, token0))
            ]
            , TradeType.EXACT_OUTPUT);


        Assert.Equal(ETHER, trade.InputAmount.Currency);
        Assert.Equal(token0, trade.OutputAmount.Currency);
    }

    //[Fact]
    //public async void FromRoutes_CanBeConstructedWithEtherAsOutputWithMultipleRoutes()
    //{
    //    var trade = await Trade.FromRoutes<Token, Ether, TradeType.EXACT_OUTPUT>(
    //        new List<RouteInput>
    //        {
    //            new RouteInput
    //            {
    //                Amount = CurrencyAmount.FromRawAmount(Ether.OnChain(1), BigInteger.Parse("4000")),
    //                Route = new Route(new List<Pool> { pool_weth_0 }, token0, ETHER),
    //            },
    //            new RouteInput
    //            {
    //                Amount = CurrencyAmount.FromRawAmount(Ether.OnChain(1), BigInteger.Parse("6000")),
    //                Route = new Route(new List<Pool> { pool_0_1, pool_weth_1 }, token0, ETHER),
    //            }
    //        },
    //        TradeType.EXACT_OUTPUT
    //    );
    //    Assert.Equal(token0, trade.InputAmount.Currency);
    //    Assert.Equal(ETHER, trade.OutputAmount.Currency);
    //}

    //[Fact]
    //public async void FromRoutes_CanBeConstructedWithEtherAsOutputForExactInputWithMultipleRoutes()
    //{
    //    var trade = await Trade.FromRoutes<Token, Ether, TradeType.EXACT_INPUT>(
    //        new List<RouteInput>
    //        {
    //            new RouteInput
    //            {
    //                Amount = CurrencyAmount.FromRawAmount(token0, BigInteger.Parse("3000")),
    //                Route = new Route(new List<Pool> { pool_weth_0 }, token0, ETHER),
    //            },
    //            new RouteInput
    //            {
    //                Amount = CurrencyAmount.FromRawAmount(token0, BigInteger.Parse("7000")),
    //                Route = new Route(new List<Pool> { pool_0_1, pool_weth_1 }, token0, ETHER),
    //            }
    //        },
    //        TradeType.EXACT_INPUT
    //    );
    //    Assert.Equal(token0, trade.InputAmount.Currency);
    //    Assert.Equal(ETHER, trade.OutputAmount.Currency);
    //}

    //[Fact]
    //public async void FromRoutes_ThrowsIfPoolsAreReUsedBetweenRoutes()
    //{
    //    await Assert.ThrowsAsync<Exception>(async () =>
    //    {
    //        await Trade.FromRoutes<Token, Ether, TradeType.EXACT_INPUT>(
    //            new List<RouteInput>
    //            {
    //                new RouteInput
    //                {
    //                    Amount = CurrencyAmount.FromRawAmount(token0, BigInteger.Parse("4500")),
    //                    Route = new Route(new List<Pool> { pool_0_1, pool_weth_1 }, token0, ETHER),
    //                },
    //                new RouteInput
    //                {
    //                    Amount = CurrencyAmount.FromRawAmount(token0, BigInteger.Parse("5500")),
    //                    Route = new Route(new List<Pool> { pool_0_1, pool_1_2, pool_weth_2 }, token0, ETHER),
    //                }
    //            },
    //            TradeType.EXACT_INPUT
    //        );
    //    });
    //}

    //[Fact]
    //public void CreateUncheckedTrade_ThrowsIfInputCurrencyDoesNotMatchRoute()
    //{
    //    Assert.Throws<Exception>(() =>
    //        Trade.CreateUncheckedTrade(new TradeInput
    //        {
    //            Route = new Route(new List<Pool> { pool_0_1 }, token0, token1),
    //            InputAmount = CurrencyAmount.FromRawAmount(token2, 10000),
    //            OutputAmount = CurrencyAmount.FromRawAmount(token1, 10000),
    //            TradeType = TradeType.EXACT_INPUT,
    //        })
    //    );
    //}

    //[Fact]
    //public void CreateUncheckedTrade_ThrowsIfOutputCurrencyDoesNotMatchRoute()
    //{
    //    Assert.Throws<Exception>(() =>
    //        Trade.CreateUncheckedTrade(new TradeInput
    //        {
    //            Route = new Route(new List<Pool> { pool_0_1 }, token0, token1),
    //            InputAmount = CurrencyAmount.FromRawAmount(token0, 10000),
    //            OutputAmount = CurrencyAmount.FromRawAmount(token2, 10000),
    //            TradeType = TradeType.EXACT_INPUT,
    //        })
    //    );
    //}

    //[Fact]
    //public void CreateUncheckedTrade_CanCreateAnExactInputTradeWithoutSimulating()
    //{
    //    Trade.CreateUncheckedTrade(new TradeInput
    //    {
    //        Route = new Route(new List<Pool> { pool_0_1 }, token0, token1),
    //        InputAmount = CurrencyAmount.FromRawAmount(token0, 10000),
    //        OutputAmount = CurrencyAmount.FromRawAmount(token1, 100000),
    //        TradeType = TradeType.EXACT_INPUT,
    //    });
    //}

    //[Fact]
    //public void CreateUncheckedTrade_CanCreateAnExactOutputTradeWithoutSimulating()
    //{
    //    Trade.CreateUncheckedTrade(new TradeInput
    //    {
    //        Route = new Route(new List<Pool> { pool_0_1 }, token0, token1),
    //        InputAmount = CurrencyAmount.FromRawAmount(token0, 10000),
    //        OutputAmount = CurrencyAmount.FromRawAmount(token1, 100000),
    //        TradeType = TradeType.EXACT_OUTPUT,
    //    });
    //}

    //[Fact]
    //public void CreateUncheckedTradeWithMultipleRoutes_ThrowsIfInputCurrencyDoesNotMatchRouteWithMultipleRoutes()
    //{
    //    Assert.Throws<Exception>(() =>
    //        Trade.CreateUncheckedTradeWithMultipleRoutes(new MultipleRoutesInput
    //        {
    //            Routes = new List<RouteInput>
    //            {
    //                new RouteInput
    //                {
    //                    Route = new Route(new List<Pool> { pool_1_2 }, token2, token1),
    //                    InputAmount = CurrencyAmount.FromRawAmount(token2, 2000),
    //                    OutputAmount = CurrencyAmount.FromRawAmount(token1, 2000),
    //                },
    //                new RouteInput
    //                {
    //                    Route = new Route(new List<Pool> { pool_0_1 }, token0, token1),
    //                    InputAmount = CurrencyAmount.FromRawAmount(token2, 8000),
    //                    OutputAmount = CurrencyAmount.FromRawAmount(token1, 8000),
    //                }
    //            },
    //            TradeType = TradeType.EXACT_INPUT,
    //        })
    //    );
    //}

    //[Fact]
    //public void CreateUncheckedTradeWithMultipleRoutes_ThrowsIfOutputCurrencyDoesNotMatchRouteWithMultipleRoutes()
    //{
    //    Assert.Throws<Exception>(() =>
    //        Trade.CreateUncheckedTradeWithMultipleRoutes(new MultipleRoutesInput
    //        {
    //            Routes = new List<RouteInput>
    //            {
    //                new RouteInput
    //                {
    //                    Route = new Route(new List<Pool> { pool_0_2 }, token0, token2),
    //                    InputAmount = CurrencyAmount.FromRawAmount(token0, 10000),
    //                    OutputAmount = CurrencyAmount.FromRawAmount(token2, 10000),
    //                },
    //                new RouteInput
    //                {
    //                    Route = new Route(new List<Pool> { pool_0_1 }, token0, token1),
    //                    InputAmount = CurrencyAmount.FromRawAmount(token0, 10000),
    //                    OutputAmount = CurrencyAmount.FromRawAmount(token2, 10000),
    //                }
    //            },
    //            TradeType = TradeType.EXACT_INPUT,
    //        })
    //    );
    //}

    //[Fact]
    //public void CreateUncheckedTradeWithMultipleRoutes_CanCreateAnExactInputTradeWithoutSimulatingWithMultipleRoutes()
    //{
    //    Trade.CreateUncheckedTradeWithMultipleRoutes(new MultipleRoutesInput
    //    {
    //        Routes = new List<RouteInput>
    //        {
    //            new RouteInput
    //            {
    //                Route = new Route(new List<Pool> { pool_0_1 }, token0, token1),
    //                InputAmount = CurrencyAmount.FromRawAmount(token0, 5000),
    //                OutputAmount = CurrencyAmount.FromRawAmount(token1, 50000),
    //            },
    //            new RouteInput
    //            {
    //                Route = new Route(new List<Pool> { pool_0_2, pool_1_2 }, token0, token1),
    //                InputAmount = CurrencyAmount.FromRawAmount(token0, 5000),
    //                OutputAmount = CurrencyAmount.FromRawAmount(token1, 50000),
    //            }
    //        },
    //        TradeType = TradeType.EXACT_INPUT,
    //    });
    //}

    //[Fact]
    //public void CreateUncheckedTradeWithMultipleRoutes_CanCreateAnExactOutputTradeWithoutSimulatingWithMultipleRoutes()
    //{
    //    Trade.CreateUncheckedTradeWithMultipleRoutes(new MultipleRoutesInput
    //    {
    //        Routes = new List<RouteInput>
    //        {
    //            new RouteInput
    //            {
    //                Route = new Route(new List<Pool> { pool_0_1 }, token0, token1),
    //                InputAmount = CurrencyAmount.FromRawAmount(token0, 5001),
    //                OutputAmount = CurrencyAmount.FromRawAmount(token1, 50000),
    //            },
    //            new RouteInput
    //            {
    //                Route = new Route(new List<Pool> { pool_0_2, pool_1_2 }, token0, token1),
    //                InputAmount = CurrencyAmount.FromRawAmount(token0, 4999),
    //                OutputAmount = CurrencyAmount.FromRawAmount(token1, 50000),
    //            }
    //        },
    //        TradeType = TradeType.EXACT_OUTPUT,
    //    });
    //}
}