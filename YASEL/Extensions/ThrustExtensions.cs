using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace ThrustExtensions
{
    public static class ThrustExtensions
    {
       
        public static void SetThrustOverride(this IMyThrust t, double overridePercent)
        {
            double ovrdVal = Math.Max(t.GetMaxThrust() * 0.1, t.GetMaxThrust() * overridePercent);
            t.SetValue("Override", (float)(ovrdVal));

        }
        public static float GetMaxThrust(this IMyThrust thruster)
        {
            return thruster.GetMaximum<float>("Override");
        }

    }
}
