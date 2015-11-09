using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace BatteryExtensions
{
    /// <summary>
    /// Common battery block functions
    /// </summary>
    public static class BatteryExtensions
    {
        public static List<IMyBatteryBlock> ToBatteryBlocks(this List<IMyTerminalBlock> bs)
        {
            var l = new List<IMyBatteryBlock>();
            bs.ForEach(b => { if (b is IMyBatteryBlock) l.Add(b as IMyBatteryBlock); });
            return l;
        }
        /// <summary>
        /// Charge of battery as % (0-1)
        /// </summary>
        /// <param name="b"></param>
        /// <returns>Percent charge</returns>
        public static float ChargePercent(this IMyBatteryBlock b)
        {
            return b.CurrentStoredPower / b.MaxStoredPower;
        }

        /// <summary>
        /// Charge of batteries as %
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns>Percent charge</returns>
        public static float ChargePercent(this List<IMyBatteryBlock> blocks)
        {
            float totalCharge = 0;
            blocks.ForEach(b => { totalCharge += ChargePercent(b); });
            return totalCharge / blocks.Count;
        }

        /// <summary>
        /// Determines if battery is charging from DetailedInfo
        /// </summary>
        /// <param name="b"></param>
        /// <returns>True if charging</returns>
        public static bool IsCharging(this IMyBatteryBlock b)
        {
            return b.GetValueBool("Recharge");
        }

        /// <summary>
        /// Determines if batteries are charging
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns>True if all blocks are charging, false if one is not</returns>
        public static bool IsCharging(this  List<IMyBatteryBlock> blocks)
        {
            bool rval = true;
            blocks.ForEach(b => { if (rval) rval = IsCharging(b); });
            return rval;
        }

        public static void Recharge(this IMyBatteryBlock b, bool on = true)
        {
            b.SetValueBool("Recharge", on);
        }
        public static void Recharge(this List<IMyBatteryBlock> blocks, bool on = true)
        {
            blocks.ForEach(b => { Recharge(b, on); });
        }
        public static void Discharge(this IMyBatteryBlock b, bool on = true)
        {
            b.SetValueBool("Discharge", on);
        }
        public static void Discharge(this List<IMyBatteryBlock> blocks, bool on = true)
        {
            blocks.ForEach(b => { Discharge(b, on); });
        }
    }
}
