using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace RefineryManagerProgram
{

    using RefineryManager;

    class RefineryManagerProgram : MyGridProgram
    {
        RefineryManager refineryManager;
        void Main(string argument)
        {
            if (refineryManager == null)
            {
                refineryManager = new RefineryManager(this);
                refineryManager.AddRefineryGroup("Ore For Productivity", "Productive Refineries");
                refineryManager.AddRefineryGroup("Ore For Effectiveness", "Effective Refineries");
            }
            refineryManager.ManageRefineries();
        }

    }
}