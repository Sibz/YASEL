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
    static class YaNavBlockExtensions
    {
        // 
        public static bool IsAligned(this IMyTerminalBlock block, Vector3D rotationVector, Vector3D refVec)
        {
            //Vector3D refVec = block.GetDirectionalVector(direction);
            var rotVec = Vector3D.Transform(rotationVector, MatrixD.Transpose(block.WorldMatrix.GetOrientation()));
            var rot = Vector3D.Cross(refVec, rotVec);
            double ang = rot.Length();
            ang = Math.Atan2(ang, Math.Sqrt(Math.Max(0.0, 1.0 - ang * ang))); //More numerically stable than: ang=Math.Asin(ang)

            if (ang < 0.01)
                return true;

            return false;
        }
        public static Vector3D GetDirectionalVector(this IMyTerminalBlock block, string direction = "forward")
        {
            Matrix localOrientation;
            block.Orientation.GetMatrix(out localOrientation);

            Vector3D refVec =
                direction == "left" ? localOrientation.Left :
                direction == "right" ? localOrientation.Right :
                direction == "up" ? localOrientation.Up :
                direction == "down" ? localOrientation.Down :
                direction == "backward" ? localOrientation.Backward :
                localOrientation.Forward;
            return refVec;
        }
    }
}