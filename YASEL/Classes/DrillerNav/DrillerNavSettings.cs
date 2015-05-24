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
        #region DrillerNavSettings
        public class DrillerNavSettings
        {

            public string LCDWaypointsName,
                                    ButtonSensorName,
                                    LCDVariableStoreName,
                                    OreCargoGroupName,
                                    AsteroidSensorName;

            public double ShaftHeight, ShaftWidth, ShaftDepth, MiningSpeed;
            public int ShaftMaxCount, StartingShaftNumber;

            public DrillerNavSettings()
            {
                ShaftHeight = 4;
                ShaftWidth = 7;
                ShaftDepth = 200;
                ShaftMaxCount = 10;
                MiningSpeed = 1;
                StartingShaftNumber = 0;
                LCDWaypointsName = "LCD Waypoints";
                LCDVariableStoreName = "LCD Var Store";
                OreCargoGroupName = "Ore Cargo";
                AsteroidSensorName = "SensorCloseAsteroid";
            }

        }
        #endregion
    }
}
