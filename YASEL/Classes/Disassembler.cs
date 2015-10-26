using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Disassembler
{
    using Grid;
    using Inventory;

    public class Disassembler
    {
        DisassemblerSettings s;

        public Disassembler(DisassemblerSettings settings)
        {
            s = settings;
        }

        public void FillDisassemblers()
        {
            var cargos = Grid.GetBlockGrp(s.ScrapComponentsCargoGroup);
            var assemblers = Grid.GetBlockGrp(s.DisassemblerGroup);

            Grid.Echo("Counting Items In Cargo : " + Inventory.CountItems(cargos));

            if (Inventory.CountItems(cargos) == 0)
                return;
            
            var invAssemblers = new List<IMyInventory>();

            assemblers.ForEach(assembler =>
            {
                invAssemblers.Add(assembler.GetInventory(1));
            });

            Grid.Echo("Assembler Inventories:" + invAssemblers.Count);

            var invCargos = Inventory.GetInventories(cargos);

            Grid.Echo("Cargo Inventories:" + invCargos.Count);

            invAssemblers.ForEach(invAssembler =>
            {
                if (((float)invAssembler.CurrentVolume / (float)invAssembler.MaxVolume) < 0.95)
                {
                    invCargos.ForEach(invCargo =>
                    {
                        if (invCargo.CurrentVolume>0)
                        {
                            Inventory.MoveItems(invCargo, invAssembler);
                            if (((float)invAssembler.CurrentVolume / (float)invAssembler.MaxVolume) > 0.95)
                                return;
                        }
                    });
                }
            });
        }
    }

    public class DisassemblerSettings
    {
        public string ScrapComponentsCargoGroup;
        public string DisassemblerGroup;
    }
}