using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRageMath;


namespace StationManagerProgram
{
    using StationManager;
    using GridHelper;
    using Block;

    class StationManagerProgram : MyGridProgram
    {
        StationManager myStationManager;
        TimeSpan TimeSinceRun;
        GridHelper gh;
        
        void Main(string argument)
        {
            if (gh == null) gh = new GridHelper(this);

            if (myStationManager==null)
                myStationManager = new StationManager(gh,new StationManagerSettings() { TextPanelTimeName = "TP Time"});

            if (TimeSinceRun.TotalMilliseconds < 250)
            {
                TimeSinceRun += ElapsedTime;
                return;
            }
            TimeSinceRun = new TimeSpan();
            myStationManager.ManageAutoDoors();
            myStationManager.DisplayTime();
            myStationManager.ManageOxygen();
            var b = gh.GetBlock("test");
            var l = new List<IMyTerminalBlock>();

        }

    }
    
}