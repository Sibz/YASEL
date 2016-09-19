using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AssemblerProgram
{

    using AssemblerManager;

    class AssemblerProgram : MyGridProgram
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

        Dictionary<string, double> targets = new Dictionary<string, double>();

        AssemblerManager myAM;
        void Main(string argument)
        {

            if (myAM== null)
            {
                targets.Add("SteelPlate", 5000);
                targets.Add("InteriorPlate", 2500);
                targets.Add("Construction", 5000);
                targets.Add("LargeTube", 500);
                targets.Add("SmallTube", 1500);
                targets.Add("Motor", 1500);
                targets.Add("Computer", 2000);
                targets.Add("MetalGrid", 1000);
                targets.Add("BulletproofGlass", 2000);
                targets.Add("Display", 1000);
                targets.Add("Girder", 1000);
                //targets.Add("", 0);
                //targets.Add("", 0);


                // settings
                AssemblerManagerSettings myAM_Settings = new AssemblerManagerSettings
                {
                    // if you want to only use some assemblers in checks uncomment line below and provide group name
                    // AssemblerGroupName = "", 

                    // REQUIRED - provide a group of cargo containers to count items from, otherwise it counts from all inventories
                    CargoGroupName = "Component Storage",
                    IngotStorageName = "Ingot Storage", 

                    // REQUIRED, LCD with list of items and stock levels 
                    // OR - use Targets Dictionary of string/double:
                    //LCDStockLevelsName = ""
                    /*
                     Example List:
                      SteelPlate:1000
                      MetalGrid:500
                      Construction:1500
                     */
                     Targets = targets
                };
                myAM = new AssemblerManager(this,myAM_Settings);
            }
            myAM.Tick();
        }
    }
}