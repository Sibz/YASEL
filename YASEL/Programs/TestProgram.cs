using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace TestProgram
{
    using DoorExtensions;
    
    class TestProgram : MyGridProgram
    {

        void Main(string argument)
        {
            var l = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName("Door", l);
            l.ForEach(i => { Echo(i.CustomName); });
            var doors = new List<IMyDoor>();
            doors = l.ToDoors();
            doors.DoClose();

        }

    }
   
}