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
    using Grid;
    using TextPanel;
    class StationManagerProgram : Program.Program
    {
        StationManager myStationManager;
        TimeSpan TimeSinceRun;
        
        void Main(string argument)
        {
            Grid.Set(GridTerminalSystem, Me, Echo);

            if (myStationManager==null)
                myStationManager = new StationManager(new StationManagerSettings() { TextPanelTimeName = "TP Time", OnAirlockUpdate = AirlockLog });

            if (TimeSinceRun.TotalMilliseconds < 250)
            {
                TimeSinceRun += ElapsedTime;
                return;
            }
            TimeSinceRun = TimeSpan.Zero;
            myStationManager.ManageAutoDoors();
            myStationManager.DisplayTime();
            myStationManager.ManageAirlocks();
            Me.GetActionWithName("Run").Apply(Me);
        }

        void AirlockLog(string name, string state, float percent)
        {
            TextPanel.Write("Airlock "+name+" TP", "Airlock: " + name + "\nState: " + state + "\nOxygen Level: " + percent*100 + "%", false);
        }
    }
    
}