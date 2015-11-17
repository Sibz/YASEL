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
    }
}