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

    class AirlockManagerProgram2 : MyGridProgram
    {
        
        static AirlockManager am;

        void Main(string argument)
        {
            Grid.Set(this);


            if (am == null)
            {
                am = new AirlockManager();
                am.AddAirlock("Hangar Outer Door", "Hangar Inner Door", "Hangar Vent");
            }

            if (argument.Contains("AirlockP"))
            {

            }
        }

    }
}