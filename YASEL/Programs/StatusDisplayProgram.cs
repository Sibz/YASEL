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
    using SolarInfoModule;
    using OreInfoModule;
    using ProgramExtensions;
    class StatusDisplayProgram : MyGridProgram
    {
        StatusDisplay statusDisplay;
        StatusDisplaySettings settings = new StatusDisplaySettings();
        void Main(string argument)
        {
            ProgramExtensions.Debug = true;

            if (statusDisplay == null)
            {
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("pad", "  ");

                settings.Modules.Add(new BatteryInfoModule(this, args));
                settings.Modules.Add(new ReactorInfoModule(this, args));
                settings.Modules.Add(new SolarInfoModule(this, args));
                settings.Modules.Add(new OreInfoModule(this, args));

                statusDisplay = new StatusDisplay(this, settings);
            }
            statusDisplay.UpdateDisplays();
        }

    }

}
