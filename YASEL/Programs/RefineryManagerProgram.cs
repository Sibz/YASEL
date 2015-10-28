using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace RefineryManagerProgram
{

    using Grid;
    using RefineryManager;

    class RefineryManagerProgram : MyGridProgram
    {

        void Main(string argument)
        {
            Grid.Set(this);
            var rm = new RefineryManager();
            rm.LoadRefineries("Ore For Productivity","Productive Refineries", 1000);
            rm.LoadRefineries("Ore For Effectiveness", "Effective Refineries", 100);
        }

    }
}