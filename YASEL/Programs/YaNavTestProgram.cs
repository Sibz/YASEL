using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace YaNavTestProgram
{
    using YaNavControl;
    using YaNavGyroControl;
    using ProgramExtensions;

    class YaNavTestProgram : MyGridProgram
    {

        YaNavControl navController;
        int ticks = 0;
        int ticksPerRun = 15;


        void Main(string argument)
        {
            if (navController == null)
            {

                navController = new YaNavControl(this, new YaNavSettings()
                {
                    Remote = this.GetBlock("Remote") as IMyRemoteControl,
                    TickCount = ticksPerRun,
                    Debug = new List<string>() { "tick", "initControl", "initThruster", "travelProcess", "move" },
                    GyroSettings = new YaNavGyroControlSettings() { GyroCoEff = 0.3f }
                    
                });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(54044.06, -30534.40, -1936.67), Speed = 95f });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(53279.77,-30374.92,-2806.39), Speed = 75 });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(53505.71, -29514.11, -2170.06), Speed = 25f });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(54044.06, -30534.40, -1936.67), Speed = 95f });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(53279.77, -30374.92, -2806.39), Speed = 75 });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(53505.71, -29514.11, -2170.06), Speed = 25f });
            }
            if (ticks % ticksPerRun == 0)
            {
                navController.Tick();
            }
            ticks++;

        }

    }
}

