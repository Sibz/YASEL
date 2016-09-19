using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace StatusDisplayProgram
{
    using StatusDisplay;
    using BatteryInfoModule;
    using ReactorInfoModule;
    using SolarInfoModule;
    using OreInfoModule;
    using ProgramExtensions;
    using TaskQueue;

    class Program : MyGridProgram
    {
        StatusDisplay statusDisplay;
        StatusDisplaySettings settings = new StatusDisplaySettings();
        TaskQueue queue;
        public Program()
        {
            queue = new TaskQueue(this, this.GetBlock("Queue Timer") as IMyTimerBlock);
            queue.Enqueue(initStatusDisplay);
        }

        void Main(string argument)
        {
            queue.Tick();
        }

        void initStatusDisplay()
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("pad", "  ");

            settings.Modules.Add(new BatteryInfoModule(this, args));
            settings.Modules.Add(new ReactorInfoModule(this, args));
            settings.Modules.Add(new SolarInfoModule(this, args));
            settings.Modules.Add(new OreInfoModule(this, args));

            statusDisplay = new StatusDisplay(this, ref queue, settings);
        }
    }

}
