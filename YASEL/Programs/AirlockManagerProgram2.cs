using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AirlockManagerProgram2
{

    using Grid;
    using AirlockManager2;
    //using TextPanel;

    class AirlockManagerProgram2 : MyGridProgram
    {
        
        static AirlockManager am;
        //IMyTextPanel tp;

        void Main(string argument)
        {
            Grid.Set(this);


            if (am == null)
            {
                am = new AirlockManager();
                am.AddAirlock("HangarAirlock", "Airtight Hangar Door", "JS Air Vent - Hangar", "JS Door - BridgeToHangar");
                am.AddAirlockNP("BridgeAirlock", "Airlock Door - Bridge Out", "Airlock Door - Bridge In");
                //tp = Grid.GetBlock("LCD Debug") as IMyTextPanel;
                //TextPanel.Write(tp, "Init Airlock Manager\n",false);
            }

            if (argument.Contains("Airlock") || argument.Contains("airlock"))
            {
                string[] args = argument.Split(' ');
                if (args.Length < 3)
                    throw new Exception("AirlockManager: Unable to open/close airlock - invalid arguments - usage: [open/close] Airlock [airlock name]");
                if (args[0] == "Open" || args[0] == "open")
                    am.OpenAirlock(args[2]);
                else
                    am.CloseAirlock(args[2]);
            }
            else
                am.Tick();
        }

        public void debugAirlock(string name, string state, float oxylevel)
        {
            //TextPanel.Write(tp, "Airlock:"+name+ " State:" + state + " oxygen:" + oxylevel + "\n");
        }

    }
}