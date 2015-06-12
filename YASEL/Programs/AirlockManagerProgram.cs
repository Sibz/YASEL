using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AirlockManagerProgram
{
    using AirlockManager;
    using Grid;
    class AirlockManagerProgram : MyGridProgram
    {
        /*
         * Airlock Manager
         * 
         * To use you need at least 4 components:
         * A Sensor - This covers the exterior door and triggers the opening of airlock
         * An Airvent - This pressurises the airlock
         * An Interior door
         * An Exterior door
         * 
         * The sensor name defines the airlock: Airlock Sensor MyAirlock
         *  It must follow that format
         * 
         * The Airvent must have the airlock name: Air Vent MyAirlock
         * 
         * The Interior door also must have name, plus 'In': Door MyAirlock In
         * 
         * The Exterior door muust have name plus 'Ex': Door MyAirlock Ex
         * 
         * You may have multple of each component, just follow above rules
         *    You can add numbers after if you want. 
         *    I.e. Door MyAirlock In 1, Door MyAirlock In 2
         * 
         * With Sensor name it is important to keep format.
         *    'My Sensor Airlock MyAirlock 1' wont work
         *    
         * With doors, just make sure airlock name is apart of door name with 'In'/'Ex'
         *   'Big Hangar Door MyAirlock 1' wont work
         *   'Big Hangar Door MyAirlock In 1' will
         *   'MyAirlock In' will also work
         *   
         * With air vents, anything goes as long as airlock name is there
         * 
         *
         * Simple!
         * 
         */

        // Global to store airlock manager object
        AirlockManager myAirlockManager;
        // Global to keep track of time since we ran airlock script
        TimeSpan TimeSinceRun;

        void Main(string argument)
        {
            // Just some helper code
            Grid.Set(this);

            // If first run set up the AirlockManager
            // Note that if you change the airlock, i.e. add doors, you need to re-compile
            if (myAirlockManager == null)
                myAirlockManager = new AirlockManager(
                    new AirlockManagerSettings() { 
                        // Here you can set the update function, for the example it uses function below, can be ommited
                        OnAirlockUpdate = AirlockUpdate 
                    });

            // This code is to make airlock script only run every 250ms
            // Thus you can use timer with self trigger to run this programming block
            if (TimeSinceRun.TotalMilliseconds < 250)
            {
                TimeSinceRun += ElapsedTime;
                return;
            }
            TimeSinceRun = new TimeSpan();

            // This is the call to manage the aurlocks
            myAirlockManager.ManageAirlocks();
        }

        /// <summary>
        /// Callback function for doing stuff when airlock updates.
        /// I.e. you could search for lights with airlockName in them and adjust the color.
        /// </summary>
        /// <param name="airlockName">Name of airlock being updated</param>
        /// <param name="airlockState">State if the airlock either init, idle, opening, depressurise, open, closing, pressurise</param>
        /// <param name="oxygenLevel">Current level of oxygen in the airlock</param>
        void AirlockUpdate(string airlockName, string airlockState, float oxygenLevel)
        {
            // Here you can do custom stuff, like write to an LCD, or adjust lights etc
        }
    }
}