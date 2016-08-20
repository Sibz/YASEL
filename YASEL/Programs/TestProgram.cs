using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace TestProgram
{
    using TextPanelExtensions;
    using ProgramExtensions;
    
    class TestProgram : MyGridProgram
    {

        

        void Main(string argument)
        {
            var inv = this.GetBlock("test").GetInventory(0);
            var items = inv.GetItems();
            

        }

    }

}