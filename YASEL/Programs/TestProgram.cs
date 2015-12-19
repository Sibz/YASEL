using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace TestProgram
{
    using ProgramExtensions;
    using YaNavThrusterControl;

    class TestProgram : MyGridProgram
    {

        YaNavThrusterControl tc;
        int ticks = 0;

        void Main(string argument)
        {
            if (tc == null)
            {
                tc = new YaNavThrusterControl(this, new YaNavThrusterSettings()
                {
                    positionReferenceBlock = this.GetBlock("Cockpit"),
                    remote = this.GetBlock("Remote") as IMyRemoteControl,
                    thrusterGroupNames = new YaNavThrusterGroupNames()
                    {
                        Forward = "ft",
                        Backward = "bt",
                        Left = "lt",
                        Right = "rt",
                        Up = "ut",
                        Down = "dt"
                    }
                });
            }
            if (ticks % 15 == 0)
            {
                tc.Tick();
                tc.MoveForward(0f); // 50 m/s works ok, 1-5 m/s not so well.
                tc.MoveUp(0f); // 50 m/s works ok, 1-5 m/s not so well.
                tc.MoveRight(0f); // 50 m/s works ok, 1-5 m/s not so well.
                IMyTextPanel tp = this.GetBlock("LCD test") as IMyTextPanel;
                tp.WritePublicText(Me.DetailedInfo);
            }
            ticks++;

        }

    }

}