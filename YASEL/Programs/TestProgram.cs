using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace TestProgram
{
    using ProgramExtensions;
    using YaNavGyroControl;

    class TestProgram : MyGridProgram
    {

        YaNavGyroControl gyroControl;
        int ticks = 0;
        int ticksPerRun = 15;

        void Main(string argument)
        {
            if (gyroControl == null)
            {
                gyroControl = new YaNavGyroControl(this, new YaNavGyroControlSettings() { OrientationReferenceBlock = this.GetBlock("Cockpit"), GyroCoEff = 0.2f } );
                gyroControl.SetVectorAndDirection((this.GetBlock("Remote") as IMyRemoteControl).GetNaturalGravity(), "down");
            }
            if (ticks % ticksPerRun == 0)
            {
                gyroControl.Tick();

            }
            ticks++;

        }

    }

}