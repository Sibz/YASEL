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
        ShipManagerSettings settings;

        ConnectorManager cm;
        BatteryManager bm;

        List<IMyTerminalBlock> listThrusters;
        List<IMyTerminalBlock> listGyros;
        List<IMyTerminalBlock> listSpots;
        List<IMyTerminalBlock> listReactors;
        Dictionary<string, BreachDoorZone> breachDoorZones;

        IMyTextPanel tpDebug;

        public ShipManager(MyGridProgram gp, ShipManagerSettings settings = null)
        {
            this.gp = gp;
            this.settings = (settings == null) ? new ShipManagerSettings() : settings;
            
            cm = new ConnectorManager(gp);
            bm = new BatteryManager(gp);
            
            listThrusters       = new List<IMyTerminalBlock>();
            listGyros           = new List<IMyTerminalBlock>();
            listSpots           = new List<IMyTerminalBlock>();
            listReactors        = new List<IMyTerminalBlock>();

            gp.GridTerminalSystem.GetBlocksOfType<IMyThrust>        ( listThrusters, b => { return b.CubeGrid == gp.Me.CubeGrid; });
            gp.GridTerminalSystem.GetBlocksOfType<IMyGyro>          ( listGyros,     b => { return b.CubeGrid == gp.Me.CubeGrid; });
            gp.GridTerminalSystem.GetBlocksOfType<IMyLightingBlock> ( listSpots,     b => { return b.CubeGrid == gp.Me.CubeGrid; });
            gp.GridTerminalSystem.GetBlocksOfType<IMyReactor>       ( listReactors,  b => { return b.CubeGrid == gp.Me.CubeGrid; });
            
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
        public void ManageDockingState()
        {
            bool doTurnOff = false;
            if ( settings.BaseConnector != "")
            {
                var cCon = gp.GetBlock( settings.BaseConnector, false);
                if (cCon is IMyShipConnector && (cCon as IMyShipConnector).IsConnected)
                    doTurnOff = true;
            } else
                doTurnOff = true;
            if ( cm.AnyConnected() && doTurnOff)
            {
                if ( settings.SwitchThrusters)      listThrusters.TurnOff();
                if ( settings.SwitchGyros) listGyros.TurnOff();
                if ( settings.SwitchLights) listSpots.TurnOff();
                if ( settings.SwitchReactors) listReactors.TurnOff();
                if ( settings.DischargeBattteries) bm.Discharge(false);
                if ( settings.RechargeBatteries) bm.Recharge(true);
            }
            else
            {
                if ( settings.SwitchThrusters) listThrusters.TurnOn();
                if ( settings.SwitchGyros) listGyros.TurnOn();
                if ( settings.SwitchLights) listSpots.TurnOn();
                if ( settings.SwitchReactors) listReactors.TurnOn();
                if ( settings.RechargeBatteries) bm.Recharge(false);
                if ( settings.DischargeBattteries) bm.Discharge(true);
            }
        }

        public void Dock()
        {
            if ( cm.AnyConnected())
            {
                listReactors.TurnOn();
                bm.Recharge(false);
                cm.UnLock();
            }else 
                cm.Lock();
        }
        
        public void AddBreachDoorZone(string ventSideA, string ventSideB, string doorSideA, string doorSideB)
        {
            var bdz = new BreachDoorZone( gp, ventSideA, ventSideB, doorSideA, doorSideB);
            if ( breachDoorZones.ContainsKey(ventSideA + ventSideB + doorSideA + doorSideB))
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

    class ShipManagerSettings
    {
        public bool SwitchThrusters = true, SwitchGyros = true, SwitchReactors = true, SwitchLights = true, RechargeBatteries = true,
            DischargeBattteries = true;
        public string BaseConnector = "";
        
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
            var doors = new List<IMyTerminalBlock>();
            doors.AddList(DoorsA);
            doors.AddList(DoorsB);
            if (breachVal > 0)
            {
                doors.ForEach(d => { if (d is IMyDoor) (d as IMyDoor).Shut(); });
            }
            else if (breachVal <= -1 && VentA.GetOxygenLevel() > 0.95 && VentB.GetOxygenLevel() > 0.95)
            {
                doors.ForEach(d3 => { if (d3 is IMyDoor) (d3 as IMyDoor).Open(); });
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