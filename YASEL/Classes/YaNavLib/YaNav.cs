using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace YaNav
{

    public class YaNavException : Exception
    {
        public YaNavException(string message)
            : base("Yanav: " + message)
        {

        }
    }
    

    public static class lang
    {
        //Repeated Phrases
        public const string NoAccess = "Unable to access ";
        //Error strings
        public const string ErrorNoGyroBlocks = NoAccess + "gyroscope blocks.";
        public const string ErrorNoThrusterBlocks = NoAccess + "some or all thruster blocks.";
        public const string ErrorNoRemote = NoAccess + "remote";
        public const string ErrorNullRemote = "Remote is null.";
        public const string ErrorNoDownThrusters = NoAccess + "down thrusters or none specified and settings.InNatrualGravityOnly is not set to true.";

        //Warning Strings


    }
    
}