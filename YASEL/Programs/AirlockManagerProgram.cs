using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AirlockManagerProgram
{
    using AirlockManager;
    using Grid;
    using TextPanel;
    class AirlockManagerProgram : Program.Program
    {
        AirlockManager myAirlockManager;
        TimeSpan TimeSinceRun;

        void Main(string argument)
        {
            Grid.Set(GridTerminalSystem, Me, Echo);

            if (myAirlockManager == null)
                myAirlockManager = new AirlockManager(new AirlockManagerSettings() { OnAirlockUpdate = AirlockLog });

            if (TimeSinceRun.TotalMilliseconds < 250)
            {
                TimeSinceRun += ElapsedTime;
                return;
            }
            TimeSinceRun = new TimeSpan();

            myAirlockManager.ManageAirlocks();
        }

        void AirlockLog(string name, string state, float percent)
        {
            TextPanel.Write("Airlock " + name + " TP", "Airlock: " + name + "\nState: " + state + "\nOxygen Level: " + percent * 100 + "%", false);
        }
    }
}