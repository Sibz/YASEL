using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace StatusDisplayProgram
{
    using StatusDisplay;
    using BatteryInfoModule;
    using ProgramExtensions;
    class StatusDisplayProgram : MyGridProgram
    {
        StatusDisplay statusDisplay;
        BatteryInfoModule bModule;
        void Main(string argument)
        {
            ProgramExtensions.Debug = true;
            statusDisplay = new StatusDisplay(this);
            if (bModule == null) bModule = new BatteryInfoModule(statusDisplay);
            statusDisplay.AddModule(bModule);
            statusDisplay.UpdateDisplays();
        }

    }

}