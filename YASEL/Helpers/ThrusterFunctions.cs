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
        #region ThrusterFunctions
        //# Requires GenericFunctions
        public static void SetThrustOverride(IMyTerminalBlock thrust, double ovrd)
        {
            var t = thrust as IMyThrust;
            var bDef = t.BlockDefinition.ToString();
            double percentOvrd = ((double)ovrd) / 100;
            double ovrdVal = 0;

            if (InStrI(bDef, "SmallBlockLargeThrust"))
                ovrdVal = (((144000 * percentOvrd) < 1500) && (ovrd > 0)) ? 1500 : (144000 * percentOvrd);
            else if (InStrI(bDef, "SmallBlockSmallThrust"))
                ovrdVal = (((12000 * percentOvrd) < 150) && (ovrd > 0)) ? 150 : (12000 * percentOvrd);
            else
                throw new Exception("Unsupported Thruster TYPE:" + t.BlockDefinition.ToString());

            t.SetValue("Override", (float)(ovrdVal));

        }

        public static void SetThrustOverride(List<IMyTerminalBlock> thrusts, double ovrd)
        { thrusts.ForEach(t => { SetThrustOverride(t, ovrd); }); }
        #endregion
    }
}
