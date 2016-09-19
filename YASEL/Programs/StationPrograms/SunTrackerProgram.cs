using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace SunTrackerProgram
{
    //using ProgramExtensions;
    using SunTracker;
    //using SolarExtensions;
    //using BlockExtensions;

    class SunTrackerProgram : MyGridProgram
    {

        SunTracker mySunTracker;
        //BatteryManager myBatteryManager;
        //IMySolarPanel refPanel;
        //List<IMyTerminalBlock> reactors;
        


        void Main(string argument)
        {
            if (mySunTracker==null)
            {
                //refPanel = this.GetBlock("Solar Panel Reference", false) as IMySolarPanel;
                mySunTracker = new SunTracker(this);
                mySunTracker.AddSolarArray("Solar Panel Reference", "Rotor Solar Array 2", 0.1f);
                //reactors = new List<IMyTerminalBlock>();
                //GridTerminalSystem.GetBlocksOfType<IMyReactor>(reactors, b => { return b.CubeGrid == Me.CubeGrid; });
            }
            mySunTracker.TrackSun();
            /*
            myBatteryManager = new BatteryManager(this, false);
            if (refPanel.GetMaxPowerOutput()==0)
            {
                // During the night, set station batteries to auto, and ship batteries to recharge
                myBatteryManager.Batteries.ForEach(b => { if (b.CubeGrid == Me.CubeGrid) (b as IMyBatteryBlock).Recharge(false); else (b as IMyBatteryBlock).Recharge(); });
            }
            else 
            {
                // During the day, turn on the reactor and set batteries to recharge. This will utilise 100% solar energy, and top up the rest will reactor power.
                reactors.TurnOn(); 
                myBatteryManager.Recharge();
            }*/
        }

    }
   
}