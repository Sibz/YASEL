using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace YASEL_Program1
{

    using Grid;
    using Block;
    using Connector;
    using Battery;

    class YASEL_Program1 : MyGridProgram
    {
        

        void Main(string argument)
        {
            Grid.Set(this);

            if (argument=="")
                ManageDockingState();
            else if (argument=="undock")
            {
                List<IMyTerminalBlock> listBats = new List<IMyTerminalBlock>();
                Grid.ts.GetBlocksOfType<IMyBatteryBlock>(listBats, Grid.BelongsToGrid);
                // TODO set battery recharge to off.

            }
        }

        /// <summary>
        /// Turns Engines and gyros off when connected, and back on when disconnected.
        /// </summary>
        void ManageDockingState()
        {
            List<IMyTerminalBlock> listConnectors = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> listThrusters = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> listGyros = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> listSpots = new List<IMyTerminalBlock>();

            Grid.ts.GetBlocksOfType<IMyShipConnector>(listConnectors, Grid.BelongsToGrid);
            Grid.ts.GetBlocksOfType<IMyThrust>(listThrusters, Grid.BelongsToGrid);
            Grid.ts.GetBlocksOfType<IMyGyro>(listGyros, Grid.BelongsToGrid);
            Grid.ts.GetBlocksOfType<IMyLightingBlock>(listSpots, Grid.BelongsToGrid);

            if (Connector.IsDocked(listConnectors))
            {
                Block.TurnOnOff(listThrusters, false);
                Block.TurnOnOff(listGyros, false);
                Block.TurnOnOff(listSpots, false);
                // TODO set battery recharge to on.
            }
            else
            {
                Block.TurnOnOff(listThrusters);
                Block.TurnOnOff(listGyros);
                Block.TurnOnOff(listSpots);
            }
        }

    }
}