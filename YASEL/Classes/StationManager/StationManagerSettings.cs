using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game;


namespace YASEL
{
    partial class Program
    {
        #region StationManagerSettings
        // Version 0.1.0
        public class StationManagerSettings
        {
            public string TimeLCDName,
                                CargoStatusLCDName;

            public double OxygenGenHighThreshold,
                                OxygenGenLowThreshold,
                                AirlockHangarDoorWaitTime;

            // key = type i.e. ore/ingot/component/ice, val = name
            public Dictionary<string, string> CargoGroups;

            public StationManagerSettings()
            {
                CargoGroups = new Dictionary<string, string>();
                OxygenGenHighThreshold = 90;
                OxygenGenLowThreshold = 10;
                AirlockHangarDoorWaitTime = 0;
            }

        }
        #endregion
    }
}
