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
        public static List<IMyShipConnector> ToShipConnectors(this List<IMyTerminalBlock> bs)
        {
            var cns = new List<IMyShipConnector>();
            bs.ForEach(b => { if (b is IMyShipConnector) cns.Add(b as IMyShipConnector); });
            return cns;
        }
        /// <summary>
        /// Checks if a connector from list of connectors is connected to something
        /// </summary>
        /// <param name="connectors"></param>
        /// <returns>True if a connector is connected<br />
        /// False if none are connected, or empty list</returns>
        /// <exception cref="System.Exception">Thrown when an item in the list is not a IMyShipConnector</exception>
        public static bool IsConnected(this List<IMyShipConnector> connectors)
        {
            bool connected = false;
            connectors.ForEach(c =>
            {
                if (c.IsConnected)
                    connected = true;
            });
            return connected;
        }

        /// <summary>
        /// Checks connector is in ready to lock state
        /// </summary>
        /// <param name="connector"></param>
        /// <returns>True if ready to lock</returns>
        /// <exception cref="System.Exception">Thrown if <i>connector</i> is null</exception>
        public static bool IsReadyToLock(this IMyShipConnector c)
        {
            return c.IsLocked;
        }

        /// <summary>
        /// Checks a connector in a list of connectors is in ready to lock state
        /// </summary>
        /// <param name="connectors"></param>
        /// <returns>True if a connector is ready to lock</returns>
        /// <exception cref="System.Exception">Thrown when an item in list is not a IMyShipConnector</exception>
        public static bool IsReadyToLock(this List<IMyShipConnector> connectors)
        {
            bool rtl = false;
            connectors.ForEach(c =>
            {
                if (c.IsReadyToLock())
                    rtl = true;
            });
            return rtl;
        }

        /// <summary>
        /// Switches the lock state
        /// </summary>
        /// <param name="b"></param>
        /// <exception cref="System.Exception">Thrown when <i>b</i> is not a IMyShipConnector</exception>
        public static void SwitchLock(this IMyShipConnector c)
        {
            c.GetActionWithName("SwitchLock").Apply(c);
        }

        /// <summary>
        /// Switches the lock state of a list of connectors
        /// </summary>
        /// <param name="connectors"></param>
        /// <exception cref="System.Exception">Thrown when a item in <i>connectors</i> is not a IMyShipConnector</exception>
        public static void SwitchLock(this List<IMyShipConnector> connectors)
        {
            connectors.ForEach(c =>
            {
                SwitchLock(c);
            });
        }

        /// <summary>
        /// Switches the throw out setting
        /// </summary>
        /// <param name="c"></param>
        /// <param name="on"></param>
        /// <exception cref="System.Exception">Thrown when <i>c</i> is null</exception>
        public static void ThrowOut(this IMyShipConnector c, bool on = true)
        {
            if ((c.IsFunctional) && ((c.ThrowOut & !on) || !c.ThrowOut & on))
                c.GetActionWithName("ThrowOut").Apply(c);
        }

        /// <summary>
        /// Switches the throw out setting for a list of connectors
        /// </summary>
        /// <param name="connectors"></param>
        /// <param name="on"></param>
        /// <exception cref="System.Exception">Thrown when item in <i>connectors</i> is null (or not a IMyShipConnector)</exception>
        public static void ThrowOut(this List<IMyShipConnector> connectors, bool on = true)
        {
            connectors.ForEach(c =>
            {
                c.ThrowOut(on);
            });
        }
    }
}
