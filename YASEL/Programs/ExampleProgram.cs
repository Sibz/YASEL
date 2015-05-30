// standard using statments
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRageMath;

// Wrap your program in a custom namespace, This has to be your file name
namespace ExampleProgram
{
    // Put using statements here
    using Str; // Examply of inclusion of other file, this one is for string functions
    using Grid; // This one is for Grid functions

    // Your programs class, must extend Program.Program, otherwise YASEL Exporter won't work.
    class ExampleProgram : Program.Program
    {

        // You can use variables here that are initialised first time the PB runs, and the keep their values between runs
        string myPersistantVariable;

        // Add a main function just like in game
        void Main(string argument)
        {
            // A Simple program that cylces doors print there names, and put Explaination marks after ones that have Auto in the name
            var doors = Grid.GetBlocks<IMyDoor>();
            doors.ForEach(door =>
            {
                Echo(door.CustomName + (Str.Contains(door.CustomName, "Auto") ? "!!!" : ""));
            });
        }
    }
}