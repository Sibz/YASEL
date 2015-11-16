using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace RotorExtensions
{
    public static class RotorExtensions
    {
        public static void Reverse(this IMyMotorStator rotor)
        {
            rotor.GetActionWithName("Reverse").Apply(rotor);
        }
        public static int GetAngle(this IMyMotorStator rotor)
        {
            string[] lines = rotor.DetailedInfo.Split('\n');
            if (lines.Length < 2)
                throw new Exception("RotorExtensions.GetAngle: Unable to get lines from detailed info (lines.Length=" + lines.Length + ")");
            string[] words = lines[1].Split(' ');
            if (words.Length < 3)
                throw new Exception("RotorExtensions.GetAngle: Unable to get words from detailed info (words.Length=" + words.Length + ")");
            
            int angle = Convert.ToInt32(words[2].Remove(words[2].Length - 1));

            return angle;
        }
    }
}