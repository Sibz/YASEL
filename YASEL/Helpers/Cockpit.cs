using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRageMath;


namespace Cockpit
{
    /// <summary>
    /// Static class for Cockpit/IMyShipController functions
    /// </summary>
    static class Cockpit
    {
        /// <summary>
        /// Switchs the state on ship dampeners
        /// </summary>
        /// <param name="b">cockpit/controller block to switch</param>
        /// <param name="on">true (deafault) to turn on<br />false to turn off</param>
        public static void TurnOnOffDampeners(IMyTerminalBlock b, bool on = true)
        {
            var c = b as IMyShipController;
            if (c == null)
                throw new Exception("Cockpit.TurnOnOffDampeners: block passed is not an IMyShipController");
            if (on && !c.DampenersOverride)
                c.GetActionWithName("DampenersOverride").Apply(c);
            else if (!on && c.DampenersOverride)
                c.GetActionWithName("DampenersOverride").Apply(c);
        }
    }
}