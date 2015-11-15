using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AirlockManagerProgram
{

    using AirlockManager;
    //using TextPanel;

    class AirlockManagerProgram : MyGridProgram
    {

        AirlockManager airlockManager;
        //IMyTextPanel tp;
        

        void Main(string argument)
        {

            if (airlockManager == null)
            {
                airlockManager = new AirlockManager(this);
                airlockManager.AddAirlock("HangarAirlock", "Airtight Hangar Door", "JS Air Vent - Hangar", "JS Door - BridgeToHangar");
                airlockManager.AddAirlockNP("BridgeAirlock", "Airlock Door - Bridge Out", "Airlock Door - Bridge In");
                //tp = Grid.GetBlock("LCD Debug") as IMyTextPanel;
                //TextPanel.Write(tp, "Init Airlock Manager\n",false);
            }

            if (argument.Contains("Airlock") || argument.Contains("airlock"))
            {
                string[] args = argument.Split(' ');
                if (args.Length < 3)
                    throw new Exception("AirlockManager: Unable to open/close airlock - invalid arguments - usage: [open/close] Airlock [airlock name]");
                if (args[0] == "Open" || args[0] == "open")
                    airlockManager.OpenAirlock(args[2]);
                else
                    airlockManager.CloseAirlock(args[2]);
            }
            else
                airlockManager.Tick();
        }

        public void debugAirlock(string name, string state, float oxylevel)
        {
            //TextPanel.Write(tp, "Airlock:"+name+ " State:" + state + " oxygen:" + oxylevel + "\n");
        }

    }
}