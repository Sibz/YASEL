using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace ShipManager
{
    using Grid;
    using Block;
    using Connector;
    using Battery;
    using Inventory;


    class ShipManager
    {

        List<IMyTerminalBlock> listConnectors;
        List<IMyTerminalBlock> listThrusters;
        List<IMyTerminalBlock> listGyros;
        List<IMyTerminalBlock> listSpots;
        List<IMyTerminalBlock> listBats;
        List<IMyTerminalBlock> listReactors;
        ShipManagerSettings s;

        public ShipManager()
        {
            listConnectors      = new List<IMyTerminalBlock>();
            listThrusters       = new List<IMyTerminalBlock>();
            listGyros           = new List<IMyTerminalBlock>();
            listSpots           = new List<IMyTerminalBlock>();
            listBats            = new List<IMyTerminalBlock>();
            listReactors        = new List<IMyTerminalBlock>();

            Grid.ts.GetBlocksOfType<IMyBatteryBlock>    (listBats, Grid.BelongsToGrid);
            Grid.ts.GetBlocksOfType<IMyShipConnector>   (listConnectors, Grid.BelongsToGrid);
            Grid.ts.GetBlocksOfType<IMyThrust>          (listThrusters, Grid.BelongsToGrid);
            Grid.ts.GetBlocksOfType<IMyGyro>            (listGyros, Grid.BelongsToGrid);
            Grid.ts.GetBlocksOfType<IMyLightingBlock>   (listSpots, Grid.BelongsToGrid);
            Grid.ts.GetBlocksOfType<IMyReactor>         (listReactors, Grid.BelongsToGrid);
        }
        public ShipManager(ShipManagerSettings settings) : this()
        {
            s = settings;
        } 

        /// <summary>
        /// Turns Engines, reactors, lights and gyros off when connected, and back on when disconnected.
        /// Note: to kickstart the power back on use Dock function to dock or undock ship.
        /// </summary>
        /// <param name="thrusters"></param>
        /// <param name="gyros"></param>
        /// <param name="lights"></param>
        /// <param name="batteries"></param>
        /// <param name="reactors"></param>
        public void ManageDockingState(bool thrusters = true, bool gyros = true, bool lights = true, bool batteries = true, bool reactors = true)
        {

            if (Connector.IsDocked(listConnectors))
            {
                if (thrusters)      Block.TurnOnOff(listThrusters, false);
                if (gyros)          Block.TurnOnOff(listGyros, false);
                if (lights)         Block.TurnOnOff(listSpots, false);
                if (reactors)       Block.TurnOnOff(listReactors, false);
                if (batteries)      Battery.Recharge(listBats);
            }
            else
            {
                if (thrusters)      Block.TurnOnOff(listThrusters);
                if (gyros)          Block.TurnOnOff(listGyros);
                if (lights)         Block.TurnOnOff(listSpots);
                if (reactors)       Block.TurnOnOff(listReactors);
                if (batteries)      Battery.Recharge(listBats, false);
            }
        }

        public void Dock()
        {
            if (Connector.IsDocked(listConnectors))
            {
                Block.TurnOnOff(listReactors);
                Battery.Recharge(listBats, false);
            }
            Connector.SwitchLock(listConnectors);
        }

        public void LoadFromGroup(string checkConnector = "")
        {
            if (!Connector.IsDocked(listConnectors)) return;
            if (checkConnector != "")
            {
                var con = Grid.GetBlock(checkConnector, false) as IMyShipConnector;
                if ((con is IMyShipConnector) && !Connector.IsDocked(con)) return;
            }

            var cargoGroup = Grid.GetBlockGrp(s.LoadFromGroupName);
            if (Inventory.CountItems(cargoGroup) == 0)
                return;
            var invsFrom = Inventory.GetInventories(cargoGroup);
            var invsTo = Inventory.GetInventories(Grid.GetBlockGrp(s.LoadToGroupName));
            invsTo.ForEach(invTo =>
            {
                if ((float)invTo.CurrentVolume/(float)invTo.MaxVolume < 0.98)
                {
                    invsFrom.ForEach(invFrom =>
                    {
                        Inventory.MoveItems(invFrom, invTo); 
                    });
                }
            });
        }

    }

    public class ShipManagerSettings
    {
        public string LoadFromGroupName = "BaseCargoGroup";
        public string LoadToGroupName = "ShipCargoGroup";
    }
}