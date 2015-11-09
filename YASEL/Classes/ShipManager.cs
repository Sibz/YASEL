using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace ShipManager
{
    using ProgramExtensions;
    using BlockExtensions;
    using ThrustExtensions;
    using ConnectorExtensions;
    using BatteryExtensions;
    using Inventory;
    using DoorExtensions;


    class ShipManager
    {
        MyGridProgram gp;

        List<IMyShipConnector> listConnectors;
        List<IMyThrust> listThrusters;
        List<IMyTerminalBlock> listGyros;
        List<IMyTerminalBlock> listSpots;
        List<IMyBatteryBlock> listBats;
        List<IMyTerminalBlock> listReactors;
        Dictionary<string, float> VentCheckPressures;

        ShipManagerSettings s;

        IMyTextPanel tpDebug;

        public ShipManager(MyGridProgram gp)
        {
            this.gp = gp;
            listConnectors      = new List<IMyShipConnector>();
            listThrusters       = new List<IMyThrust>();
            listGyros           = new List<IMyTerminalBlock>();
            listSpots           = new List<IMyTerminalBlock>();
            listBats            = new List<IMyBatteryBlock>();
            listReactors        = new List<IMyTerminalBlock>();

            var tmpList = new List<IMyTerminalBlock>();
            gp.GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(tmpList, delegate(IMyTerminalBlock b) { return b.BelongsToGrid(gp); });
            listBats = tmpList.ToBatteryBlocks();
            gp.GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(tmpList, delegate(IMyTerminalBlock b) { return b.BelongsToGrid(gp); });
            listConnectors = tmpList.ToShipConnectors();
            gp.GridTerminalSystem.GetBlocksOfType<IMyThrust>(tmpList, delegate(IMyTerminalBlock b) { return b.BelongsToGrid(gp); });
            listThrusters = tmpList.ToThrusts();
            gp.GridTerminalSystem.GetBlocksOfType<IMyGyro>(listGyros, delegate(IMyTerminalBlock b) { return b.BelongsToGrid(gp); });
            gp.GridTerminalSystem.GetBlocksOfType<IMyLightingBlock>(listSpots, delegate(IMyTerminalBlock b) { return b.BelongsToGrid(gp); });
            gp.GridTerminalSystem.GetBlocksOfType<IMyReactor>(listReactors, delegate(IMyTerminalBlock b) { return b.BelongsToGrid(gp); });
            
            VentCheckPressures = new Dictionary<string, float>();

            tpDebug = gp.GetBlock("LCD Debug") as IMyTextPanel;
            
            debug("Initialised ShipManager.", false);
        }
        private void debug(string text, bool append = true)
        {
            if (tpDebug is IMyTextPanel)
                tpDebug.WritePublicText(text + "\n", append);
        }
        public ShipManager(MyGridProgram gp, ShipManagerSettings settings)
            : this(gp)
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
            bool doTurnOff = false;
            if (connectedConnector != "")
            {
                var cCon = gp.GetBlock(connectedConnector, false);
                if (cCon is IMyShipConnector && (cCon as IMyShipConnector).IsConnected)
                    doTurnOff = true;
            } else
                doTurnOff = true;
            if (listConnectors.IsConnected() && doTurnOff)
            {
                if (thrusters)      listThrusters.ForEach(t => { t.TurnOff(); });
                if (gyros)          listGyros.ForEach(t => { t.TurnOff(); });
                if (lights)         listSpots.TurnOff();
                if (reactors)       listReactors.TurnOff();
                if (batteries)      listBats.Recharge(true);
            }
            else
            {
                if (thrusters)      listThrusters.ForEach(t => { t.TurnOn(); });
                if (gyros)          listGyros.ForEach(t => { t.TurnOn(); });
                if (lights)         listSpots.TurnOn();
                if (reactors)       listReactors.TurnOn();
                if (batteries)      listBats.Recharge(false);
            }
        }

        public void Dock()
        {
            if (listConnectors.IsConnected())
            {
                listReactors.TurnOn();
                listBats.Recharge(false);
            }
            listConnectors.SwitchLock();
        }

        public void LoadFromGroup(string checkConnector = "")
        {
            if (!listConnectors.IsConnected()) return;
            if (checkConnector != "")
            {
                var con = gp.GetBlock(checkConnector, false) as IMyShipConnector;
                if ((con is IMyShipConnector) && !con.IsConnected) return;
            }

            var cargoGroup = gp.GetBlockGroup(s.LoadFromGroupName);
            if (Inventory.CountItems(cargoGroup) == 0)
                return;
            var invsFrom = Inventory.GetInventories(cargoGroup);
            var invsTo = Inventory.GetInventories(gp.GetBlockGroup(s.LoadToGroupName));
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
            var ventA = gp.GetBlock(ventSideA) as IMyAirVent;
            var ventB = gp.GetBlock(ventSideB) as IMyAirVent;
            var doorA = gp.GetBlock(doorSideA) as IMyDoor;
            var doorB = gp.GetBlock(doorSideB) as IMyDoor;
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
       
        private void switchBreachDoors(IMyDoor a, IMyDoor b, bool close = true)
        {
            if (close)
            {
                a.DoClose();
                b.DoClose();
            } else
            {
                a.DoOpen();
                b.DoOpen();
            }
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

        
        
    }

    public class ShipManagerSettings
    {
        public string LoadFromGroupName = "BaseCargoGroup";
        public string LoadToGroupName = "ShipCargoGroup";
    }

}