using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Disassembler
{
    using GridHelper;
    using Inventory;

    public class Disassembler
    {
        GridHelper gh;
        DisassemblerSettings s;

        public Disassembler(GridHelper gh,DisassemblerSettings settings)
        {
            this.gh = gh;
            s = settings;
        }

        public void FillDisassemblers()
        {
            var cargos = gh.GetBlockGrp(s.ScrapComponentsCargoGroup);
            var assemblers = gh.GetBlockGrp(s.DisassemblerGroup);

            gh.Echo("Counting Items In Cargo : " + Inventory.CountItems(cargos));

            if (Inventory.CountItems(cargos) == 0)
                return;
            
            var invAssemblers = new List<IMyInventory>();

            assemblers.ForEach(assembler =>
            {
                invAssemblers.Add(assembler.GetInventory(1));
            });

            gh.Echo("Assembler Inventories:" + invAssemblers.Count);

            var invCargos = Inventory.GetInventories(cargos);

            gh.Echo("Cargo Inventories:" + invCargos.Count);

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