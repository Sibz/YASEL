using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace DisassemblerProgram
{

    using Disassembler;

    class DisassemblerProgram : MyGridProgram
    {
        void Main(string argument)
        {
            var dSettings = new DisassemblerSettings() {
                DisassemblerGroup = "Disassemblers", ComponentsCargoGroup = "Cargo Components"
            };
            var d = new Disassembler(this,dSettings);
            d.FillDisassemblers();
        }

    }
}