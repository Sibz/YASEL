using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace ShipManagerProgram
{

    using GridHelper;
    using Block;
    using Connector;
    using Battery;
    using ShipManager;

    class ShipManagerProgram : MyGridProgram
    {
        GridHelper gh;

        void Main(string argument)
        {
            if (gh == null) gh = new GridHelper(this);


            ShipManager sm = new ShipManager(gh);

            if (argument == "")
            {
                sm.ManageDockingState();
            }
            else if (argument == "undock" || argument == "dock")
                sm.Dock();
        }
    }
}