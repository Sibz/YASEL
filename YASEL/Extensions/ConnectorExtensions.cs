using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace ConnectorExtensions
{
    /// <summary>
    /// Common connector functions
    /// </summary>
    static class ConnectorExtensions
    {

        /// <summary>
        /// Switches the lock state
        /// </summary>
        /// <param name="b"></param>
        public static void SwitchLock(this IMyShipConnector c)
        {
            c.GetActionWithName("SwitchLock").Apply(c);
        }

        public static void UnLock(this IMyShipConnector c)
        {
            if (c.IsConnected) c.SwitchLock();
        }
        public static void Lock(this IMyShipConnector c)
        {
            if (!c.IsConnected && c.IsLocked) c.SwitchLock();
        }
        /// <summary>
        /// Switches the throw out setting
        /// </summary>
        /// <param name="on"></param>
        public static void ThrowOut(this IMyShipConnector c, bool on = true)
        {
            if ((c.IsFunctional) && ((c.ThrowOut & !on) || !c.ThrowOut & on))
                c.GetActionWithName("ThrowOut").Apply(c);
        }
        
    }
}
