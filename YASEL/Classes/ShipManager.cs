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
    using Door;
    using TextPanel;


    class ShipManager
    {

        List<IMyTerminalBlock> listConnectors;
        List<IMyTerminalBlock> listThrusters;
        List<IMyTerminalBlock> listGyros;
        List<IMyTerminalBlock> listSpots;
        List<IMyTerminalBlock> listBats;
        List<IMyTerminalBlock> listReactors;
        Dictionary<string, float> VentCheckPressures;

        ShipManagerSettings s;

        IMyTextPanel tpDebug;

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

            VentCheckPressures = new Dictionary<string, float>();

            tpDebug = Grid.GetBlock("LCD Debug") as IMyTextPanel;
            
            debug("Initialised ShipManager.", false);
        }
        private void debug(string text, bool append = true)
        {
            if (tpDebug is IMyTextPanel)
                TextPanel.Write(tpDebug, text+"\n", append);
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
        public void ManageDockingState(string connectedConnector = "", bool thrusters = true, bool gyros = true, bool lights = true, bool batteries = true, bool reactors = true)
        {
            bool doTurnOff = true;
            if (connectedConnector != "")
            {
                var cCon = Grid.GetBlock(connectedConnector);
                if (!(cCon is IMyShipConnector) || (cCon is IMyShipConnector && !Connector.IsDocked(cCon as IMyShipConnector)))
                    doTurnOff = false;
            }
            if (Connector.IsDocked(listConnectors) && doTurnOff)
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

        public void ManageBreachDoors(string ventSideA, string doorSideA, string doorSideB, string ventSideB)
        {
            var ventA = Grid.GetBlock(ventSideA) as IMyAirVent;
            var ventB = Grid.GetBlock(ventSideB) as IMyAirVent;
            var doorA = Grid.GetBlock(doorSideA) as IMyDoor;
            var doorB = Grid.GetBlock(doorSideB) as IMyDoor;
            bool init = false;
            if (!VentCheckPressures.ContainsKey(ventSideA))
            {
                VentCheckPressures.Add(ventSideA, ventA.GetOxygenLevel());
                init = true;
            }
            if (!VentCheckPressures.ContainsKey(ventSideB))
            {
                VentCheckPressures.Add(ventSideB, ventB.GetOxygenLevel());
                init = true;
            }
            if (init) return;

            int breachVal = checkBreach(ventSideA, ventA) + checkBreach(ventSideB, ventB);
            if (breachVal>0)
                switchBreachDoors(doorA, doorB); // One area is breached, close doors
            else if (breachVal<=-1 && ventA.GetOxygenLevel()>0.95 && ventB.GetOxygenLevel()>0.95)
                switchBreachDoors(doorA, doorB, false); // Both areas are sealed with air, open doors

            VentCheckPressures[ventSideA] = ventA.GetOxygenLevel();
            VentCheckPressures[ventSideB] = ventB.GetOxygenLevel();

        }
        private int checkBreach(string ventName, IMyAirVent vent)
        {
            if (VentCheckPressures[ventName] > vent.GetOxygenLevel() &&
                vent.GetOxygenLevel() < 0.95) // Pressure has dropped below breathable level, Breach!
                return 1;
            else if (VentCheckPressures[ventName] < vent.GetOxygenLevel() &&
                vent.GetOxygenLevel() > 0.95) // Pressure is rising and at breathable level, Breach sealed.
                return -1;
            else
                return 0; // Pressure neither rising or falling
        }
        private void switchBreachDoors(IMyDoor a, IMyDoor b, bool close = true)
        {
            if (close)
            {
                Door.Close(a);
                Door.Close(b);
            } else
            {
                Door.Open(a);
                Door.Open(b);
            }
        }

        
        
    }

    public class ShipManagerSettings
    {
        public string LoadFromGroupName = "BaseCargoGroup";
        public string LoadToGroupName = "ShipCargoGroup";
    }

}