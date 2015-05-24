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
        #region SetupStatics
        public static IMyGridTerminalSystem gts;
        public static IMyTerminalBlock sMe;
        public static Action<string> sEcho;
        
        public static void SetStatics(IMyGridTerminalSystem l_gts, IMyProgrammableBlock l_me, Action<string> l_echo)
        {
            gts = l_gts;
            sMe = l_me;
            sEcho = l_echo;
        }
        public static bool StaticsAreSet {
            get { return gts != null && sMe != null && sEcho != null; }
        }
        public static void CheckStatics()
        {
            if (!StaticsAreSet)
                throw new Exception("Static variables not set. Call 'SetStatics(GridTerminalSystem, Me, Echo);' from Main.");
        }
        #endregion
    }
}
