using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace RefineryManagerProgram
{

    using GridHelper;
    using RefineryManager;

    class RefineryManagerProgram : MyGridProgram
    {
        GridHelper gh;

        void Main(string argument)
        {
            if (gh == null) gh = new GridHelper(this);
            var rm = new RefineryManager(gh);
            rm.LoadRefineries("Ore For Productivity","Productive Refineries", 1000);
            rm.LoadRefineries("Ore For Effectiveness", "Effective Refineries", 100);
        }

    }
}