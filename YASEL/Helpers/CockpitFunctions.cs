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
        #region CockpitFunctions

        public static void TurnOnOffDampeners(IMyTerminalBlock b, bool on = true)
        {
            var c = b as IMyShipController;
            if (on && !c.DampenersOverride)
                c.GetActionWithName("DampenersOverride").Apply(c);
            else if (!on && c.DampenersOverride)
                c.GetActionWithName("DampenersOverride").Apply(c);
        }

        #endregion
    }
}
