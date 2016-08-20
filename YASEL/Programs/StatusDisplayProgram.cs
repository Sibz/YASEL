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
    using ReactorInfoModule;
    using ProgramExtensions;
    class StatusDisplayProgram : MyGridProgram
    {
        StatusDisplay statusDisplay;
        StatusDisplaySettings settings = new StatusDisplaySettings();
        void Main(string argument)
        {
            ProgramExtensions.Debug = true;
            settings.Modules.Add(new BatteryInfoModule(this));
            settings.Modules.Add(new ReactorInfoModule(this));
            statusDisplay = new StatusDisplay(this, settings);
            statusDisplay.UpdateDisplays();
        }

    }

}