using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace GyroExtensions
{
    /// <summary>
    /// Common Gyroscope functions
    /// </summary>
    static class GyroExtensions
    {

        const string GYRO_YAW = "Yaw";
        const string GYRO_PITCH = "Pitch";
        const string GYRO_ROLL = "Roll";

        public static void SwitchOveride(this IMyGyro gyro, bool on = true)
        {
            if (on && !gyro.GyroOverride) gyro.GetActionWithName("Override").Apply(gyro);
            if (!on && gyro.GyroOverride) gyro.GetActionWithName("Override").Apply(gyro);
        }
        public static void Override(this IMyGyro gyro, string action)
        {
            gyro.GetActionWithName(action).Apply(gyro);
        }
        public static void Override(this IMyGyro gyro, string action, int gSpeed)
        {
            if (action.Contains(GYRO_YAW)) gyro.Stop(GYRO_YAW);
            if (action.Contains(GYRO_PITCH)) gyro.Stop(GYRO_PITCH);
            if (action.Contains(GYRO_ROLL)) gyro.Stop(GYRO_ROLL);
            for (int i = 1; i <= gSpeed && i <= 10; i++)
            { gyro.Override(action); }
        }
        public static void Stop(this IMyGyro gyro, string movement)
        {
            gyro.SetValueFloat(movement, 0);
        }
        
    }
}
