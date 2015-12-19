using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace YaNavThrusterControlTestProgram
{
    using ProgramExtensions;
    using YaNavThrusterControl;

    class YaNavThrusterControlTestProgram : MyGridProgram
    {

        YaNavThrusterControl thrusterControl;
        int ticks = 0;
        int ticksPerRun = 15;

        void Main(string argument)
        {
            if (thrusterControl == null)
            {
                thrusterControl = new YaNavThrusterControl(this, new YaNavThrusterSettings()
                {
                    Remote = this.GetBlock("Remote") as IMyRemoteControl, // Must be inline with ship
                    MassCoEff = 0.8f, // Lower for light/manuverable ships, increase for heavy ships. (Default 1f, good for heavy ships)
                    TickCount = ticksPerRun, // Set to the number of ticks that pass between each run (Default=15)
                    InNatrualGravityOnly = true, // Set to true (default) to only handle movements within gravity, allows the absence of down thursters. If false, down thrusters are required.
                    Debug = new List<string>() { "tick", "moveAngle"}
                });
            }
            if (ticks % ticksPerRun == 0)
            {
                thrusterControl.Tick();

                //thrusterControl.MoveAngle(new Vector3D(0.9, 0.1, 0));
                
                //thrusterControl.MoveAngle(new Vector3D(1, 0, 0)); // Right
                //thrusterControl.MoveAngle(new Vector3D(0, 1, 0)); // Up
                //thrusterControl.MoveAngle(new Vector3D(0, 0, 1)); // Backward
                thrusterControl.MoveForward(15f);
                //thrusterControl.MoveUp(0f); 
                //thrusterControl.MoveRight(0f); */

            }
            ticks++;

        }

    }

}