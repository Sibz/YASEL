using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace InventoryTestProgram
{

    using GridHelper;
    using Inventory;

    class InventoryTestProgram : MyGridProgram
    {
        GridHelper gh;

        void Main(string argument)
        {
            if (gh == null) gh = new GridHelper(this);

            var invBlocksFrom = gh.GetBlockGrp("Cargo Components");
            var invBlocksTo = gh.GetBlockGrp("Welder Cargo"); 
            Inventory.MoveItemAmount(invBlocksFrom, invBlocksTo, "SteelPlate",110);
        }

    }
}