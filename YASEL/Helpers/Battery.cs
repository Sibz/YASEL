using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Battery
{
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
            return b.GetValueBool("Recharge");
        }

        /// <summary>
        /// Determines if batteries are charging
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns>True if all blocks are charging, false if one is not</returns>
        public static bool IsCharging(List<IMyTerminalBlock> blocks)
        {
            bool rval = true;
            blocks.ForEach(b => { if (b is IMyBatteryBlock && rval) rval = IsCharging(b as IMyBatteryBlock); });
            return rval;
        }

        public static void Recharge(IMyBatteryBlock b, bool on = true)
        {
            b.SetValueBool("Recharge", on);
        }
        public static void Recharge(List<IMyBatteryBlock> blocks, bool on = true)
        {
            blocks.ForEach(b => Recharge(b, on));
        }
    }
}
