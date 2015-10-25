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

    class Disassembler
    {
        DisassemblerSettings s;

        public Disassembler(DisassemblerSettings settings)
        {
            s = settings;
        }

        void UpdateDisassemblers()
        {
            var cargos = Grid.GetBlockGrp(s.ScrapComponentsCargoGroup);
            var assemblers = Grid.GetBlockGrp(s.DisassemblerGroup);
            
        }
    }

    public class DisassemblerSettings
    {
        public string ScrapComponentsCargoGroup;
        public string DisassemblerGroup;
    }
}