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

            var smSettings = new ShipManagerSettings() { LoadFromGroupName = "Cargo Components", LoadToGroupName = "Welder Cargo" };

            ShipManager sm = new ShipManager(smSettings);

            if (argument == "")
            {
                sm.ManageDockingState();
                sm.LoadFromGroup("Connector - Components");
            }
            else if (argument == "undock" || argument == "dock")
                sm.Dock();
        }
    }
}