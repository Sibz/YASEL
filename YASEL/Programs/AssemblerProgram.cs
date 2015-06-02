using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AssemblerProgram
{

    using Grid;
    using AssemblerManager;

    class AssemblerProgram : Program.Program
    {
        /*
         * AssemblerProgram
         * 
         * Uses an LCD for an input list of items/stock levels and turns on
         * assembler with said item name if the count of item is below stock level
         * 
         * Note: If you add or remove assemblers/or cargo containers, you will have to recompile script
         * 
         * 
         */

        AssemblerManager myAM;
        void Main(string argument)
        {
            Grid.Set(GridTerminalSystem, Me, Echo);

            if (myAM == null)
            {
                // settings
                AssemblerManagerSettings myAM_Settings = new AssemblerManagerSettings
                {
                    // if you want to only use some assemblers in checks uncomment line below and provide group name
                    // AssemblerGroupName = "", 

                    // RECOMMENDED - provide a group of cargo containers to count items from, otherwise it counts from all inventories
                    // CargoGroupName = "", 

                    // REQUIRED, LCD with list of items and stock levels:
                    LCDStockLevelsName = ""
                    /*
                     Example List:
                      SteelPlate:1000
                      MetalGrid:500
                      Construction:1500
                     */
                };
                myAM = new AssemblerManager(myAM_Settings);
            }
            myAM.Tick();
        }
    }
}