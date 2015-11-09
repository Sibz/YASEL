using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Connector
{
    using GridHelper;
    /// <summary>
    /// Common connector functions
    /// </summary>
    static class Connector
    {
        /// <summary>
        /// Checks if connector is connected to something
        /// </summary>
        /// <param name="connector"></param>
        /// <returns>True if connected</returns>
        /// <exception cref="System.Exception">Thrown when <i>connector</i> is null</exception>
        public static bool IsDocked(IMyShipConnector connector)
        {
            if (connector == null)
                throw new Exception("Connector.IsDocked: argument is null.");
            var builder = new StringBuilder();
            connector.GetActionWithName("SwitchLock").WriteValue(connector, builder);
            return (builder.ToString() == "Locked");
        }

        /// <summary>
        /// Checks if connector is connected to something
        /// </summary>
        /// <param name="connectorName">Name of connector</param>
        /// <returns>True if connected</returns>
        /// <exception cref="System.Exception">Thrown when <i>connectorName</i> is not accessible (doesn't exist, incorrect owner, etc)</exception>
        public static bool IsDocked(GridHelper gh, string connectorName)
        {
            IMyShipConnector con = (IMyShipConnector)gh.GetBlock(connectorName);
            if (con==null)
                throw new Exception("Connector.IsDocked: could not access connector:" + connectorName);
            return IsDocked(con);
        }

        /// <summary>
        /// Checks if a connector from list of connectors is connected to something
        /// </summary>
        /// <param name="connectors"></param>
        /// <returns>True if a connector is connected<br />
        /// False if none are connected, or empty list</returns>
        /// <exception cref="System.Exception">Thrown when an item in the list is not a IMyShipConnector</exception>
        public static bool IsDocked(List<IMyTerminalBlock> connectors)
        {
            bool connected = false;
            connectors.ForEach(c =>
            {
                if ((c as IMyShipConnector) == null)
                    throw new Exception("Connector.IsDocked: A IMyTerminalBlock provided in list is not a IMyShipConnector");
                if (IsDocked((IMyShipConnector)c))
                    connected = true;
            });
            return connected;
        }

        /// <summary>
        /// Checks if ship has a connected connector
        /// </summary>
        /// <returns>True if any connector on current grid is connected<br />
        /// False if none are connected, or no connectors exist.</returns>
        public static bool IsDocked(GridHelper gh)
        {
            List<IMyTerminalBlock> cons = new List<IMyTerminalBlock>();
            gh.Gts.GetBlocksOfType<IMyShipConnector>(cons);
            return IsDocked(cons);
        }

        /// <summary>
        /// Checks connector is in ready to lock state
        /// </summary>
        /// <param name="connector"></param>
        /// <returns>True if ready to lock</returns>
        /// <exception cref="System.Exception">Thrown if <i>connector</i> is null</exception>
        public static bool IsReadyToLock(IMyShipConnector connector)
        {
            if (connector == null)
                throw new Exception("Connector.IsReadyToLock: argument is null.");
            var builder = new StringBuilder();
            connector.GetActionWithName("SwitchLock").WriteValue(connector, builder);
            return (builder.ToString().Contains("Ready To Lock"));
        }

        /// <summary>
        /// Checks a connector in a list of connectors is in ready to lock state
        /// </summary>
        /// <param name="connectors"></param>
        /// <returns>True if a connector is ready to lock</returns>
        /// <exception cref="System.Exception">Thrown when an item in list is not a IMyShipConnector</exception>
        public static bool IsReadyToLock(List<IMyTerminalBlock> connectors)
        {
            bool rtl = false;
            connectors.ForEach(c =>
            {
                if (!(c is IMyShipConnector))
                    throw new Exception("Connector.IsReadyToLock: An item in list is not a IMyShipConnector");
                if (IsReadyToLock((IMyShipConnector)c))
                    rtl = true;
            });
            return rtl;
        }

        /// <summary>
        /// Switches the lock state
        /// </summary>
        /// <param name="b"></param>
        /// <exception cref="System.Exception">Thrown when <i>b</i> is not a IMyShipConnector</exception>
        public static void SwitchLock(IMyTerminalBlock b)
        {
            if (!(b is IMyShipConnector))
                throw new Exception("Block is not a IMyShipConnector");
            b.GetActionWithName("SwitchLock").Apply(b);
        }

        /// <summary>
        /// Switches the lock state of a list of connectors
        /// </summary>
        /// <param name="connectors"></param>
        /// <exception cref="System.Exception">Thrown when a item in <i>connectors</i> is not a IMyShipConnector</exception>
        public static void SwitchLock(List<IMyTerminalBlock> connectors)
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
        public static void ThrowOut(IMyShipConnector c, bool on = true)
        {
            if (c == null)
                throw new Exception("Conneector.ThrowOut: Argument is null");
            if (c.IsFunctional)
                if ((c.ThrowOut & !on) || !c.ThrowOut & on)
                    c.GetActionWithName("ThrowOut").Apply(c);
        }

        /// <summary>
        /// Switches the throw out setting for a list of connectors
        /// </summary>
        /// <param name="connectors"></param>
        /// <param name="on"></param>
        /// <exception cref="System.Exception">Thrown when item in <i>connectors</i> is null (or not a IMyShipConnector)</exception>
        public static void ThrowOut(List<IMyTerminalBlock> connectors, bool on = true)
        {
            connectors.ForEach(c =>
            {
                if (c is IMyShipConnector)
                    ThrowOut((IMyShipConnector)c, on);
                else
                    throw new Exception("Connector.ThrowOut: Block in list is not a IMyShipConnector");
            });
        }
    }
}
