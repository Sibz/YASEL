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
        #region Driller
        //# Requires DrillerNav
        //# Requires NavManager

        int tick = -1;

        //public static IMyGridTerminalSystem gts;

        public static NavManager nm;
        public static DrillerNav dn;

        //public static string LCDDebugScreenName;
        //public static int DebugLevel;

        /*
         Drill Ship Autopilot using NavManager Libiary
        */

        void Main()
        {
            SetStatics(GridTerminalSystem, Me, Echo);
            var timer = GetBlock("Timer Nav");

            // If 15 is changed NavSettings.ActionTick must also be changed
            int ActionTick = 15;
            if (!OnTick(ref tick, ActionTick)) { timer.GetActionWithName("TriggerNow").Apply(timer); return; }

            if (nm == null)
            {
                string ShipName = "DS1";
                NavSettings s = new NavSettings();
                // Override default options

                s.AlignMargin = 0.01;
                s.ActionTick = ActionTick;
                s.LeftBlockName = "Small Thruster 15";
                s.RightBlockName = s.FwdBlockName = "Small Thruster 86";
                s.TopBlockName = s.RearBlockName = "Remote Control";
                s.BottomBlockName = "Small Thruster 24";
                s.MaxSpeed = 100;
                s.GyroGroupName = "Gyros";

                DrillerNavSettings dnSettings = new DrillerNavSettings();

                dnSettings.LCDVariableStoreName = s.LCDVariableStoreName = "LCD Nav Var Store " + ShipName;
                dnSettings.OreCargoGroupName = "Ship Cargo (Ore) " + ShipName;
                dnSettings.StartingShaftNumber = 0;
                dnSettings.ShaftDepth = 100;
                dnSettings.ShaftMaxCount = 12;

                LCDDebugScreenName = "LCD Drillship Debug " + ShipName;
                DebugLevel = 1;

                nm = new NavManager(s);


                dn = new DrillerNav(dnSettings);
                //nm.Tick();
                //var wp = new WpInfo() { type="travel-center", stopDist=5, pos=new Vector3D(-7.88,-251.83,86.9) };
                //nm.Act(wp);

            }

            dn.Tick();

            // Reset gts as we may have undocked thus giving us a new GTS
            gts = GridTerminalSystem;
            timer.GetActionWithName("TriggerNow").Apply(timer);
        }

        public static bool OnTick(ref int tick, int actionTick) { tick++; if (tick >= actionTick) tick = 0; if (tick == 0)return true; return false; }
        #endregion Driller
    }
}
