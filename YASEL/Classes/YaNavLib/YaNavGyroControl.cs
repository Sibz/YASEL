using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace YaNavGyroControl
{
    using YaNav;
    using GyroExtensions;
    using ProgramExtensions;

    public class YaNavGyroControl
    {
        MyGridProgram gp;
        List<IMyTerminalBlock> gyroscopes;
        Vector3D targetVector;
        string vectorDirection = "forward";

        public bool IsRotating = false;


        public YaNavGyroControl(MyGridProgram gp, List<IMyTerminalBlock> gyroscopes = null)
        {
            this.gp = gp;
            targetVector = Vector3D.Zero;
            if (gyroscopes == null)
            {
                this.gyroscopes = new List<IMyTerminalBlock>();
                gp.GridTerminalSystem.GetBlocksOfType<IMyGyro>(this.gyroscopes, b => { return b.CubeGrid == gp.Me.CubeGrid; });
            }
            else
            {
                this.gyroscopes = gyroscopes;
            }
            if (this.gyroscopes.Count == 0)
                throw new YaNavGyroControlException(lang.ErrorNoGyroBlocks);
        }

        public void SetVectorAndDirection(Vector3D v, string d = "forward")
        {
            targetVector = v;
            vectorDirection = d;
        }
        public void ClearVectorAndDirection()
        {
            targetVector = Vector3D.Zero;
            vectorDirection = "";
            gyroscopes.ForEach(gyroscope => { (gyroscope as IMyGyro).Stop(); });
        }
        public void Tick()
        {
            if (!Vector3D.IsZero(targetVector) && vectorDirection != "")
                gyroscopes.ForEach(gyroscope =>
                {
                    IsRotating |= (gyroscope as IMyGyro).Rotate(targetVector, gyroscope.GetDirectionalVector(vectorDirection));
                });
        }
    }
    public class YaNavGyroControlException : YaNavException
    {
        public YaNavGyroControlException(string message)
            : base("GyroControl: " + message)
        {

        }
    }
}