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
    using DoorExtensions;
    using ConnectorManager;
    using BatteryManager;


    class ShipManager
    {
        MyGridProgram gp;

        ConnectorManager cm;
        BatteryManager bm;

        List<IMyTerminalBlock> listThrusters;
        List<IMyTerminalBlock> listGyros;
        List<IMyTerminalBlock> listSpots;
        List<IMyTerminalBlock> listReactors;
        Dictionary<string, BreachDoorZone> breachDoorZones;

        IMyTextPanel tpDebug;

        public ShipManager(MyGridProgram gp)
        {
            this.gp = gp;
            cm = new ConnectorManager(gp);
            bm = new BatteryManager(gp);
            
            listThrusters       = new List<IMyTerminalBlock>();
            listGyros           = new List<IMyTerminalBlock>();
            listSpots           = new List<IMyTerminalBlock>();
            listReactors        = new List<IMyTerminalBlock>();

            gp.GridTerminalSystem.GetBlocksOfType<IMyThrust>        (listThrusters, b => { return b.CubeGrid == gp.Me.CubeGrid; });
            gp.GridTerminalSystem.GetBlocksOfType<IMyGyro>          (listGyros,     b => { return b.CubeGrid == gp.Me.CubeGrid; });
            gp.GridTerminalSystem.GetBlocksOfType<IMyLightingBlock> (listSpots,     b => { return b.CubeGrid == gp.Me.CubeGrid; });
            gp.GridTerminalSystem.GetBlocksOfType<IMyReactor>       (listReactors,  b => { return b.CubeGrid == gp.Me.CubeGrid; });
            
            tpDebug = gp.GetBlock("LCD Debug") as IMyTextPanel;
            
            debug("Initialised ShipManager.", false);
        }
        private void debug(string text, bool append = true)
        {
            if (tpDebug is IMyTextPanel)
                tpDebug.WritePublicText(text + "\n", append);
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
            if (cm.AnyConnected() && doTurnOff)
            {
                if (thrusters)      listThrusters.TurnOff();
                if (gyros)          listGyros.TurnOff();
                if (lights)         listSpots.TurnOff();
                if (reactors)       listReactors.TurnOff();
                if (batteries)      bm.Recharge(true);
            }
            else
            {
                if (thrusters)      listThrusters.TurnOn();
                if (gyros)          listGyros.TurnOn();
                if (lights)         listSpots.TurnOn();
                if (reactors)       listReactors.TurnOn();
                if (batteries) bm.Recharge(false);
            }
        }

        public void Dock()
        {
            if (cm.AnyConnected())
            {
                listReactors.TurnOn();
                bm.Recharge(false);
                cm.UnLock();
            }else 
                cm.Lock();
        }

        public void AddBreachDoorZone(string ventSideA, string ventSideB, string doorSideA, string doorSideB)
        {
            var bdz = new BreachDoorZone(gp, ventSideA, ventSideB, doorSideA, doorSideB);
            if (breachDoorZones.ContainsKey(ventSideA + ventSideB + doorSideA + doorSideB))
                breachDoorZones[ventSideA + ventSideB + doorSideA + doorSideB] = bdz;
            else
                breachDoorZones.Add(ventSideA + ventSideB + doorSideA + doorSideB,bdz);
        }
        public void ManageBreachDoors()
        {
            var bdzEnum = breachDoorZones.GetEnumerator();
            while (bdzEnum.MoveNext())
                bdzEnum.Current.Value.CheckBreach();
        }
        
    }

    class BreachDoorZone
    {
        public IMyAirVent VentA, VentB;
        public List<IMyTerminalBlock> DoorsA, DoorsB;
        public float VentACheckPressure;
        public float VentBCheckPressure;

        public BreachDoorZone(MyGridProgram gp, string ventA, string ventB, string doorA, string doorB)
        {
            VentA = gp.GetBlock(ventA) as IMyAirVent;
            VentB = gp.GetBlock(ventB) as IMyAirVent;
            DoorsA = gp.SearchBlocks(doorA);
            DoorsB = gp.SearchBlocks(doorB);
            if (VentA == null || VentB == null || DoorsA.Count == 0 || DoorsB.Count == 0)
                throw new Exception("ManageBreachDoors: Unable to access a vent or door provided.");
            UpdatePressures();
        }

        public void UpdatePressures()
        {
            VentACheckPressure = VentA.GetOxygenLevel();
            VentBCheckPressure = VentB.GetOxygenLevel();
        }

        public void CheckBreach()
        {
            int breachVal = checkSide(VentA, VentACheckPressure) + checkSide(VentB, VentBCheckPressure);
            if (breachVal > 0)
            {
                DoorsA.ForEach(d => { if (d is IMyDoor) (d as IMyDoor).Shut();});
                DoorsB.ForEach(d => { if (d is IMyDoor) (d as IMyDoor).Shut();});
            }
            else if (breachVal <= -1 && VentA.GetOxygenLevel() > 0.95 && VentB.GetOxygenLevel() > 0.95)
            {
                DoorsA.ForEach(d => { if (d is IMyDoor) (d as IMyDoor).Open();});
                DoorsB.ForEach(d => { if (d is IMyDoor) (d as IMyDoor).Open();});
            }
            UpdatePressures();
        }

        private int checkSide(IMyAirVent vent, float lastPressure)
        {
            if (lastPressure > vent.GetOxygenLevel() &&
                vent.GetOxygenLevel() < 0.95) // Pressure has dropped and is below breathable level, Breach!
                return 1;
            else if (lastPressure < vent.GetOxygenLevel() &&
                vent.GetOxygenLevel() > 0.95) // Pressure is rising and at breathable level, Breach sealed.
                return -1;
            else
                return 0; // Pressure neither rising or falling
        }
    }

}