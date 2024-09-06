﻿using System.Diagnostics;
using Uniswap.Sdk.V3.Entities;

namespace Uniswap.Sdk.V3.Utils;

public static class NearestUsableTick
{
    /// <summary>
    ///     Returns the closest tick that is nearest a given tick and usable for the given tick spacing
    /// </summary>
    /// <param name="tick">The target tick</param>
    /// <param name="tickSpacing">The spacing of the pool</param>
    /// <returns>The nearest usable tick</returns>
    public static int Find(int tick, int tickSpacing)
    {
        Debug.Assert(tickSpacing > 0, "TICK_SPACING");
        Debug.Assert(tick >= Tick.MIN_TICK && tick <= Tick.MAX_TICK, "TICK_BOUND");

        var rounded = (int)Math.Round((double)tick / tickSpacing) * tickSpacing;
        if (rounded < Tick.MIN_TICK)
        {
            return rounded + tickSpacing;
        }

        if (rounded > Tick.MAX_TICK)
        {
            return rounded - tickSpacing;
        }

        return rounded;
    }
}