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
        #region ConnectorFunctions
        //# Requires SetupStatics
        //# Requires BlockFunctions
        //# Requires GenericFunctions

        public static bool IsDocked(IMyShipConnector connector)
        {
            var builder = new StringBuilder();
            connector.GetActionWithName("SwitchLock").WriteValue(connector, builder);
            return (builder.ToString() == "Locked");
        }
        public static bool IsDocked(string connectorName)
        {
            IMyShipConnector con = (IMyShipConnector)GetBlock(connectorName);
            return ((con is IMyShipConnector)) && IsDocked(con);
        }
        public static bool IsDocked(List<IMyTerminalBlock> connectors)
        {
            bool connected = false;
            connectors.ForEach(c =>
            {
                if ((c is IMyTerminalBlock) && IsDocked((IMyShipConnector)c))
                    connected = true;
            });
            return connected;
        }

        public static bool IsDocked()
        {
            CheckStatics();
            List<IMyTerminalBlock> cons = new List<IMyTerminalBlock>();
            gts.GetBlocksOfType<IMyShipConnector>(cons, BelongsToGrid);
            return IsDocked(cons);
        }
        public static bool IsReadyToLock(IMyShipConnector connector)
        {
            var builder = new StringBuilder();
            connector.GetActionWithName("SwitchLock").WriteValue(connector, builder);
            return (InStrI(builder.ToString(), "Ready To Lock"));
        }
        public static bool IsReadyToLock(List<IMyTerminalBlock> connectors)
        {
            bool rtl = false;
            connectors.ForEach(c =>
            {
                if ((c is IMyTerminalBlock) && IsReadyToLock((IMyShipConnector)c))
                    rtl = true;
            });
            return rtl;
        }
        public static void SwitchLock(IMyTerminalBlock b)
        {
            b.GetActionWithName("SwitchLock").Apply(b);
        }
        public static void SwitchLock(List<IMyTerminalBlock> connectors)
        {
            connectors.ForEach(c =>
            {
                SwitchLock(c);
            });
        }
        public static void ThrowOut(IMyShipConnector c, bool on = true)
        {
            if (c.IsFunctional)
                if ((c.ThrowOut & !on) || !c.ThrowOut & on)
                    c.GetActionWithName("ThrowOut").Apply(c);
        }
        public static void ThrowOut(List<IMyTerminalBlock> connectors, bool on = true)
        {
            connectors.ForEach(c =>
            {
                if (c is IMyShipConnector)
                    ThrowOut((IMyShipConnector)c, on);
            });
        }
        #endregion
    }
}
