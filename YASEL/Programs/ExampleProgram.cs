// standard using statments
using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

// Wrap your program in a custom namespace, This has to be your file name
namespace ExampleProgram
{
    // Put using statements here
    using GridHelper; // This one is for Grid functions

    // Your programs class, must extend MyGridProgram, otherwise YASEL Exporter won't work.
    class ExampleProgram : MyGridProgram
    {
        GridHelper gh;
        // You can use variables here that are initialised first time the PB runs, and the keep their values between runs
        string myPersistantVariable;

        // Add a main function just like in game
        void Main(string argument)
        {
            // A Simple program that cylces doors print there names, and put Explanation marks after ones that have Auto in the name

            // First we need to set Grid up
            if (gh == null) gh = new GridHelper(this);
            
            // Then we can do out stuff
            var doors = new List<IMyTerminalBlock>();
            gh.Gts.GetBlocksOfType<IMyDoor>(doors);
            doors.ForEach(door =>
            {
                Echo(door.CustomName + (door.CustomName.Contains("Auto") ? "!!!" : ""));
            });
        }
    }
}