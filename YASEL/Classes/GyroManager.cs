using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace GyroManager
{
    using GyroExtensions;
    class GyroManager
    {
        public List<IMyTerminalBlock> Gyros;

        public const string GYRO_LEFT = "IncreaseYaw";
        public const string GYRO_RIGHT = "DecreaseYaw";
        public const string GYRO_UP = "DecreasePitch";
        public const string GYRO_DOWN = "IncreasePitch";
        public const string GYRO_ROLL_L = "IncreaseRoll";
        public const string GYRO_ROLL_R = "DecreaseRoll";
        public const string GYRO_YAW = "Yaw";
        public const string GYRO_PITCH = "Pitch";
        public const string GYRO_ROLL = "Roll";

        public GyroManager(List<IMyTerminalBlock> gyros)
        {
            this.Gyros = gyros;
          
        }

        /// <summary>
        /// Yaw to the left
        /// </summary>
        /// <param name="gyros"></param>
        /// <param name="gSpeed"></param>
        public void YawL(int gSpeed)
        {
            Override(GYRO_LEFT, gSpeed);
        }
        public void YawR(int gSpeed)
        {
            Override(GYRO_RIGHT, gSpeed);
        }
        public void PitchU( int gSpeed) { Override(GYRO_UP, gSpeed); }
        public void PitchD( int gSpeed) { Override(GYRO_DOWN, gSpeed); }
        public void RollL( int gSpeed) { Override(GYRO_ROLL_L, gSpeed); }
        public void RollR( int gSpeed) { Override(GYRO_ROLL_R, gSpeed); }
        public void YawStop() { Stop(GYRO_YAW); }
        public void PitchStop() { Stop(GYRO_PITCH); }
        public void RollStop() { Stop(GYRO_ROLL); }

        public bool Moving(List<IMyTerminalBlock> gyros)
        {
            bool moving = false;
            gyros.ForEach(gyro =>
            {
                if (g is IMyGyro)
                {
                    var g = gyro as IMyGyro;
                    if (g.Yaw != 0 || g.Pitch != 0 || g.Roll != 0) moving = true;
                }
            });
            return moving;
        }

        public void Override(string action)
        {
            Gyros.ForEach(g =>
            {
                if (g is IMyGyro) (g as IMyGyro).Override(action);
            });
        }
        private void Override(string action, int gSpeed)
        {
            Gyros.ForEach(g => { if (g is IMyGyro)(g as IMyGyro).Override(action, gSpeed); });
        }

        public void Stop(string movement)
        {
            Gyros.ForEach(g => { if (g is IMyGyro) (g as IMyGyro).Stop(movement); });
        }

        public void SetOveride(bool on = true)
        {
            Gyros.ForEach(gy => { if (g is IMyGyro) (g as IMyGyro).SwitchOveride(on);});
        }
    }
}