using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace CockpitExtensions
{
    /// <summary>
    /// Static class for Cockpit/IMyShipController functions
    /// </summary>
    static class CockpitExtensions
    {
        /// <summary>
        /// Turns on ship dampeners
        /// </summary>
        /// <param name="b">cockpit/controller block to switch</param>
        /// <exception cref="System.Exception">Thrown when <i>b</i> is not a IMyShipController</exception>
        public static void DampenersOn(this IMyShipController c)
        {
            if (c == null)
                throw new Exception("Cockpit.DampenersOn: Controller is null");
            if (!c.DampenersOverride)
                c.GetActionWithName("DampenersOverride").Apply(c);
        }
        /// <summary>
        /// Turns off ship dampeners
        /// </summary>
        /// <param name="b">cockpit/controller block to switch</param>
        /// <exception cref="System.Exception">Thrown when <i>b</i> is not a IMyShipController</exception>
        public static void DampenersOff(this IMyShipController c)
        {
            if (c == null)
                throw new Exception("Cockpit.DampenersOff: Controller is null");
            if (c.DampenersOverride)
                c.GetActionWithName("DampenersOverride").Apply(c);
        }
    }
}