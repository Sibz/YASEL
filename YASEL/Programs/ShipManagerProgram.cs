using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace ShipManagerProgram
{

    using Grid;
    using Block;
    using Connector;
    using Battery;
    using ShipManager;

    class ShipManagerProgram : MyGridProgram
    {

        void Main(string argument)
        {
            Grid.Set(this);


            ShipManager sm = new ShipManager();

            if (argument == "")
            {
                sm.ManageDockingState();
            }
            else if (argument == "undock" || argument == "dock")
                sm.Dock();
        }
    }
}