using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace DisassemblerProgram
{

    using Grid;
    using Disassembler;

    class DisassemblerProgram : MyGridProgram
    {

        void Main(string argument)
        {
            Grid.Set(this);
            var dSettings = new DisassemblerSettings() { DisassemblerGroup = "Disassemblers", ScrapComponentsCargoGroup = "Scrap Components" };
            var d = new Disassembler(dSettings);
            d.FillDisassemblers();
        }

    }
}