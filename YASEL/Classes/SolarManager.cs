using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace SolarManager
{
    using SolarExtensions;
    class SolarManager
    {
        List<IMyTerminalBlock> SolarPanels;

        public SolarManager()
        {
            SolarPanels = new List<IMyTerminalBlock>();
        }

        public SolarManager(MyGridProgram gp, bool ongrid = true)
        {
            SolarPanels = new List<IMyTerminalBlock>();
            if (ongrid)
                gp.GridTerminalSystem.GetBlocksOfType<IMySolarPanel>(SolarPanels, b => { return b.CubeGrid == gp.Me.CubeGrid; });
            else
                gp.GridTerminalSystem.GetBlocksOfType<IMySolarPanel>(SolarPanels);
        }

        public SolarManager(List<IMyTerminalBlock> panels)
        {
            SolarPanels = panels;
        }

        public float GetCurrentPowerOutput()
        {
            float output = 0;
            SolarPanels.ForEach(b => { if (b is IMySolarPanel) output += (b as IMySolarPanel).GetCurrentPowerOutput(); });
            return output;
        }
    }
}