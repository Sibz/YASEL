using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game;


namespace Battery
{
    using Str;
    /// <summary>
    /// Common battery block functions
    /// </summary>
    static class Battery
    {
        /// <summary>
        /// Charge of battery as % (0-1)
        /// </summary>
        /// <param name="b"></param>
        /// <returns>Percent charge</returns>
        public static float ChargePercent(IMyBatteryBlock b)
        {
            return b.CurrentStoredPower / b.MaxStoredPower;
        }

        /// <summary>
        /// Charge of batteries as %
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns>Percent charge</returns>
        public static float ChargePercent(List<IMyTerminalBlock> blocks)
        {
            float totalCharge = 0;
            blocks.ForEach(b => { totalCharge += ChargePercent((IMyBatteryBlock)b); });
            return totalCharge / blocks.Count;
        }

        /// <summary>
        /// Determines if battery is charging from DetailedInfo
        /// </summary>
        /// <param name="b"></param>
        /// <returns>True if charging</returns>
        public static bool IsCharging(IMyBatteryBlock b)
        {
            return Str.Contains(b.DetailedInfo, "Fully recharged in");
        }

        /// <summary>
        /// Determines if batteries are charging
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns>True if all blocks are charging, false if one is not</returns>
        public static bool IsCharging(List<IMyTerminalBlock> blocks)
        {
            bool rval = true;
            blocks.ForEach(b => { if (rval) rval = IsCharging((IMyBatteryBlock)b); });
            return rval;
        }
    }
}
