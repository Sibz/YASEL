using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace InventoryTestProgram
{

    using Grid;
    using Inventory;

    class InventoryTestProgram : MyGridProgram
    {

        void Main(string argument)
        {
            Grid.Set(this);

            var invBlocksFrom = Grid.GetBlockGrp("Cargo Components");
            var invBlocksTo = Grid.GetBlockGrp("Welder Cargo"); 
            Inventory.MoveItemAmount(invBlocksFrom, invBlocksTo, "SteelPlate",110);
        }

    }
}