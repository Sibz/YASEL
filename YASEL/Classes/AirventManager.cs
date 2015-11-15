using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AirventManager
{
    using AirventExtensions;
    class AirventManager
    {
        public List<IMyTerminalBlock> Airvents;

        public AirventManager()
        {
            Airvents = new List<IMyTerminalBlock>();
        }
        public AirventManager(MyGridProgram gp, bool ongrid = true)
        {
            Airvents = new List<IMyTerminalBlock>();
            if (ongrid)
                gp.GridTerminalSystem.GetBlocksOfType<IMyAirVent>(Airvents, b => { return b.CubeGrid == gp.Me.CubeGrid; });
            else
                gp.GridTerminalSystem.GetBlocksOfType<IMyAirVent>(Airvents);
        }

        public AirventManager(List<IMyTerminalBlock> airvents)
        {
            Airvents = airvents;
        }
        public void Pressurise()
        {
            Airvents.ForEach(vent => { if (vent is IMyAirVent) (vent as IMyAirVent).Pressurise(); });
        }
        public void Depressurise()
        {
            Airvents.ForEach(vent => { if (vent is IMyAirVent) (vent as IMyAirVent).Depressurise(); });
        }
    }
}