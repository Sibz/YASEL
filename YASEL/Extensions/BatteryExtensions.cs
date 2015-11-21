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
        /// Determines if battery is charging from DetailedInfo
        /// </summary>
        /// <param name="b"></param>
        /// <returns>True if charging</returns>
        public static bool IsCharging(this IMyBatteryBlock b)
        {
            return b.GetValueBool("Recharge");
        }

        

        public static void Recharge(this IMyBatteryBlock b, bool on = true)
        {
            b.SetValueBool("Recharge", on);
        }
        
        public static void Discharge(this IMyBatteryBlock b, bool on = true)
        {
            b.SetValueBool("Discharge", on);
        }

        public static float GetChargePercent(this IMyBatteryBlock b)
        {
            return b.CurrentStoredPower / b.MaxStoredPower;
        }
        
    }
}
