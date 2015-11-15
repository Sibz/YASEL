using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace ConnectorManager
{
    using ConnectorExtensions;
    class ConnectorManager
    {
        public List<IMyTerminalBlock> Connectors;

        public ConnectorManager()
        {
            Connectors = new List<IMyTerminalBlock>();
        }

        public ConnectorManager(MyGridProgram gp, bool ongrid = true)
        {
            Connectors = new List<IMyTerminalBlock>();
            if (ongrid)
                gp.GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(Connectors, b => { return b.CubeGrid == gp.Me.CubeGrid; });
            else
                gp.GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(Connectors);
        }

        public ConnectorManager(List<IMyTerminalBlock> cs)
        {
            Connectors = cs;
        }

        public void SwitchLock()
        {
            Connectors.ForEach(c => { if (c is IMyShipConnector) (c as IMyShipConnector).SwitchLock();});
        }

        public void UnLock()
        {
            Connectors.ForEach(c => { if (c is IMyShipConnector (c as IMyShipConnector).UnLock(); });
        }

        public void Lock()
        {
            Connectors.ForEach(c => { if (c is IMyShipConnector (c as IMyShipConnector).Lock(); });
        }

        public bool AnyConnected()
        {
            bool rval = false;
            Connectors.ForEach(c => { if (c is IMyShipConnector) rval&=(c as IMyShipConnector).IsConnected; });
            return rval;
        }

        public bool AnyReadyToLock()
        {
            bool rval = false;
            Connectors.ForEach(c => { if (c is IMyShipConnector) rval &= (c as IMyShipConnector).IsLocked; });
            return rval;
        }

        public void ThrowOut(bool on = true)
        {
            Connectors.ForEach(c => { if (c is IMyShipConnector) (c as IMyShipConnector).ThrowOut(on); });
        }
    }
}