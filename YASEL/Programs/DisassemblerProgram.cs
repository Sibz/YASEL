using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace DisassemblerProgram
{

    using GridHelper;
    using Disassembler;

    class DisassemblerProgram : MyGridProgram
    {
        GridHelper gh;
        void Main(string argument)
        {
            if (gh == null) gh = new GridHelper(this);
            var dSettings = new DisassemblerSettings() { DisassemblerGroup = "Disassemblers", ScrapComponentsCargoGroup = "Scrap Components" };
            var d = new Disassembler(gh,dSettings);
            d.FillDisassemblers();
        }

    }
}