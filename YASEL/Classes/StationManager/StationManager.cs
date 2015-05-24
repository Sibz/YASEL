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
        #region StationManager
        // Version 0.1.0
        //# Requires StationManagerSettings

        public class StationManager
        {
	        StationManagerSettings settings;
	
	        public StationManager(StationManagerSettings s = null)
	        {
		        settings = s!=null?s:new StationManagerSettings();
	        }
	
	        // Shows percent full of storage groups on LCD.
	        public void DisplayCargoStatus()
	        {
	        }
	
	        // Turns Oxygen gens off at a high threshold, and back on at a low threshold
	        public void ManageOxygenGens()
	        {
	        }
	
	        // Displays Time
	        public void DisplayTime()
	        {
	        }
	
	        // Manages Airlocks
            public void ManageAirlocks()
	        {
	        }
        }
        #endregion
    }
}
