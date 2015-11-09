using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace ThrustExtensions
{
    public static class ThrustExtensions
    {
        public static List<IMyThrust> ToThrusts(this List<IMyTerminalBlock> bs)
        {
            var ths = new List<IMyThrust>();
            bs.ForEach(b => { if (b is IMyThrust) ths.Add(b as IMyThrust); });
            return ths;
        }
        //# Requires GenericFunctions
        public static void SetThrustOverride(this IMyThrust t, double ovrd)
        {

            var bDef = t.BlockDefinition.ToString();
            double percentOvrd = ((double)ovrd) / 100;
            double ovrdVal = 0;

            if (bDef.Contains("SmallBlockLargeThrust"))
                ovrdVal = (((144000 * percentOvrd) < 1500) && (ovrd > 0)) ? 1500 : (144000 * percentOvrd);
            else if (bDef.Contains("SmallBlockSmallThrust"))
                ovrdVal = (((12000 * percentOvrd) < 150) && (ovrd > 0)) ? 150 : (12000 * percentOvrd);
            else
                throw new Exception("Unsupported Thruster TYPE:" + t.BlockDefinition.ToString());

            t.SetValue("Override", (float)(ovrdVal));

        }

        public static void SetThrustOverride(this List<IMyThrust> thrusts, double ovrd)
        { thrusts.ForEach(t => { t.SetThrustOverride(ovrd); }); }
    }
}
