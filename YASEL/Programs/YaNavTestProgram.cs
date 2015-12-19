using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace YaNavTestProgram
{
    using YaNav;
    using YaNavThrusterControl;
    using ProgramExtensions;

    class YaNavTestProgram : MyGridProgram
    {

        YaNav myYaNav;
        IMyTextPanel tp;

        void Main(string argument)
        {
            tp = this.GetBlock("debug") as IMyTextPanel;
            myYaNav = new YaNav(this, new YaNavSettings()
            {
                ThrusterGroupNames = new YaNavThrusterGroupNames() {
                    Forward = "ft",
                    Backward = "bt",
                    Left = "lt",
                    Right = "rt",
                    Up = "ut",
                    Down = "dt"
                },
                InNatrualGravityOnly = true,
            });
            //var v = myYaNav.GetShipPosition();
            //tp.WritePublicText("GPS:TEST:" + v.GetDim(0) + ":" + v.GetDim(1) + ":" + v.GetDim(2) + ":");
            //GPS:Sibz #1:53557.72:-26756.97:11968.78:

        }

    }
}

