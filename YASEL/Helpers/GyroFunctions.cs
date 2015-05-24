using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game;


namespace YASEL
{
    partial class Program
    {
        #region GyroFunctions
        //# Requires GenericFunctions
        public const string GYRO_LEFT = "IncreaseYaw";
        public const string GYRO_RIGHT = "DecreaseYaw";
        public const string GYRO_UP = "DecreasePitch";
        public const string GYRO_DOWN = "IncreasePitch";
        public const string GYRO_ROLL_L = "IncreaseRoll";
        public const string GYRO_ROLL_R = "DecreaseRoll";
        public const string GYRO_YAW = "Yaw";
        public const string GYRO_PITCH = "Pitch";
        public const string GYRO_ROLL = "Roll";

        public static void YawL(List<IMyTerminalBlock> gyros, int gSpeed) { SetGyro(gyros, GYRO_LEFT, gSpeed); }
        public static void YawR(List<IMyTerminalBlock> gyros, int gSpeed) { SetGyro(gyros, GYRO_RIGHT, gSpeed); }
        public static void PitchU(List<IMyTerminalBlock> gyros, int gSpeed) { SetGyro(gyros, GYRO_UP, gSpeed); }
        public static void PitchD(List<IMyTerminalBlock> gyros, int gSpeed) { SetGyro(gyros, GYRO_DOWN, gSpeed); }
        public static void RollL(List<IMyTerminalBlock> gyros, int gSpeed) { SetGyro(gyros, GYRO_ROLL_L, gSpeed); }
        public static void RollR(List<IMyTerminalBlock> gyros, int gSpeed) { SetGyro(gyros, GYRO_ROLL_R, gSpeed); }
        public static void YawStop(List<IMyTerminalBlock> gyros) { StopGyro(gyros, GYRO_YAW); }
        public static void PitchStop(List<IMyTerminalBlock> gyros) { StopGyro(gyros, GYRO_PITCH); }
        public static void RollStop(List<IMyTerminalBlock> gyros) { StopGyro(gyros, GYRO_ROLL); }

        public static bool GyroMoving(List<IMyTerminalBlock> gyros)
        {
            bool moving = false;
            gyros.ForEach(gyro =>
            {
                var g = gyro as IMyGyro;
                if (g.Yaw != 0 || g.Pitch != 0 || g.Roll != 0) moving = true;
            });
            return moving;
        }

        public static void GyroOveride(IMyTerminalBlock gyro, string action)
        {
            (gyro as IMyGyro).GetActionWithName(action).Apply(gyro);
        }
        public static void GyroOveride(List<IMyTerminalBlock> gyros, string action)
        {
            gyros.ForEach(g =>
            {
                GyroOveride(g, action);
            });
        }
        public static void SetGyro(IMyTerminalBlock gyro, string action, int gSpeed)
        {
            var g = gyro as IMyGyro;
            if (InStrI(action, GYRO_YAW)) StopGyro(g, GYRO_YAW);
            if (InStrI(action, GYRO_PITCH)) StopGyro(g, GYRO_PITCH);
            if (InStrI(action, GYRO_ROLL)) StopGyro(g, GYRO_ROLL);
            for (int i = 1; i <= gSpeed && i <= 10; i++)
            { GyroOveride(g, action); }
        }
        public static void SetGyro(List<IMyTerminalBlock> gyros, string action, int gSpeed)
        {
            gyros.ForEach(g => { SetGyro(g, action, gSpeed); });
        }
        public static void StopGyro(IMyTerminalBlock gyro, string movement)
        {
            (gyro as IMyGyro).SetValueFloat(movement, 0);
        }

        public static void StopGyro(List<IMyTerminalBlock> gyros, string movement)
        {
            gyros.ForEach(g => { StopGyro(g, movement); });
        }

        public static void GyroOverride(List<IMyTerminalBlock> gyros, bool on = true)
        {
            gyros.ForEach(gy =>
            {
                var g = gy as IMyGyro;
                if (on && !g.GyroOverride) g.GetActionWithName("Override").Apply(g);
                if (!on && g.GyroOverride) g.GetActionWithName("Override").Apply(g);
            });
        }
        #endregion
    }
}
