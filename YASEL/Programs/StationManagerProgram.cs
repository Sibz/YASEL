using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRageMath;


namespace StationManagerProgram
{
    using StationManager;
    class StationManagerProgram : Program.Program
    {
        StationManager myStationManager;
        void Main(string argument)
        {
            if (myStationManager==null)
                myStationManager = new StationManager(new StationManagerSettings());
            myStationManager.ManageAutoDoors();
        }
    }
}