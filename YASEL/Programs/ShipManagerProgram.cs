using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace ShipManagerProgram
{

    using ShipManager;

    class ShipManagerProgram : MyGridProgram
    {

        void Main(string argument)
        {

            ShipManager sm = new ShipManager(this);

            if (argument == "")
            {
                sm.ManageDockingState();
            }
            else if (argument == "undock" || argument == "dock")
                sm.Dock();
        }
    }
}