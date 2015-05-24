using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game;

namespace YASEL
{
    partial class Program
    {
        public IMyGridTerminalSystem GridTerminalSystem;
        public IMyProgrammableBlock Me;
        public void Echo(string s) { }

    }
}
