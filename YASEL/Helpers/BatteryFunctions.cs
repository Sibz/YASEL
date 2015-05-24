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


namespace YASEL
{
    partial class Program
    {
        #region BatteryFunctions
        //# Requires GenericFunctions
        //# Requires BlockFunctions

        public static double GetBatteryCharge(IMyBatteryBlock b)
        {
            double maxPower = GetPower(GetDetail(b, "Max Stored Power"));
            double curPower = GetPower(GetDetail(b, "Stored Power"));
            return curPower / maxPower;
        }

        public static double GetBatteryCharge(List<IMyTerminalBlock> blocks)
        {
            double totalCharge = 0;
            blocks.ForEach(b => { totalCharge += GetBatteryCharge((IMyBatteryBlock)b); });
            return totalCharge / blocks.Count;

        }
        public static double GetPower(string strPower)
        {
            string[] vals = strPower.Split();
            if (!(vals.Length > 1))
            {
                return 0;
            }
            if (InStrI(strPower, "mw"))
                return Convert.ToDouble(vals[0]) * 1000;
            else if (InStrI(strPower, "kw"))
                return Convert.ToDouble(vals[0]);
            else
                return Convert.ToDouble(vals[0]) / 1000;

        }

        public static bool IsCharging(IMyBatteryBlock b)
        {
            return InStrI(b.DetailedInfo, "Fully recharged in");
        }

        public static bool IsCharging(List<IMyTerminalBlock> blocks)
        {
            bool rval = true;
            blocks.ForEach(b => { if (rval) rval = IsCharging((IMyBatteryBlock)b); });
            return rval;
        }
        #endregion
    }
}
