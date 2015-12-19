using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace GyroExtensions
{
    using BlockExtensions;
    using YaNavBlockExtensions;
    /// <summary>
    /// Common Gyroscope functions
    /// </summary>
    /// 
    static class GyroExtensions
    {

        public static void OverrideOn(this IMyGyro gyroscope)
        {
            gyroscope.SetValueBool("Override", true);
        }
        public static void OverrideOff(this IMyGyro gyroscope)
        {
            gyroscope.SetValueBool("Override", false);
        }
        public static void Stop(this IMyGyro gyroscope)
        {
            gyroscope.SetPitch(0f);
            gyroscope.SetYaw(0f);
            gyroscope.SetRoll(0f);
        }
        

       
        public static void SetPitch(this IMyGyro gyroscope, float pitch)
        {
            gyroscope.SetValueFloat("Pitch", pitch);
        }
        public static void SetYaw(this IMyGyro gyroscope, float yaw)
        {
            gyroscope.SetValueFloat("Yaw", yaw);
        }
        public static void SetRoll(this IMyGyro gyroscope, float roll)
        {
            gyroscope.SetValueFloat("Roll", roll);
        }
    }
}
