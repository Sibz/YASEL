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
    using YaNavThrusterControl
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
                    GyroSettings = new YaNavGyroControlSettings() { GyroCoEff = 0.7f }, 
                    StoppingDistance = 2000f,
                    ThrusterSettings = new YaNavThrusterSettings() { MassCoEff = 1.0f }
                    
                });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(53704.48,-26600.66,11872.15), Speed = 50f });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(53706.58,-26350.13,12503.77), Speed = 75 });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(52789.76,-27365.95,13875.54), Speed = 55f });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(53389.1,-27258.05,11960.78), Speed = 95f });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(53704.48, -26600.66, 11872.15), Speed = 95f });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(53706.58, -26350.13, 12503.77), Speed = 75 });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(53407.48, -26905.3, 12819.54), Speed = 55f });
                navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(53389.1, -27258.05, 11960.78), Speed = 95f, ResetThrusters = true });
            }
            if (ticks % ticksPerRun == 0)
            {
                navController.Tick();
            }
            ticks++;

        }

    }
}

