using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace YaNavGyroControl
{
    using YaNav;
    using YaNavBlockExtensions;
    using GyroExtensions;
    using ProgramExtensions;

    public class YaNavGyroControl
    {
        MyGridProgram gp;
        YaNavGyroControlSettings settings;
        Vector3D? targetVector;
        string vectorDirection = "forward";

        public bool IsRotating = false;


        public YaNavGyroControl(MyGridProgram gp, YaNavGyroControlSettings s)
        {
            this.gp = gp;
            this.settings = s;
            targetVector = null;
            
            if (settings.Gyroscopes.Count == 0)
                gp.GridTerminalSystem.GetBlocksOfType<IMyGyro>(settings.Gyroscopes, b => { return b.CubeGrid == gp.Me.CubeGrid; });
            
            if (settings.Gyroscopes.Count == 0)
                throw new YaNavGyroControlException(lang.ErrorNoGyroBlocks);
        }

        public void SetVectorAndDirection(Vector3D v, string d = "forward")
        {
            targetVector = v;
            vectorDirection = d;
        }
        public void ClearVectorAndDirection()
        {
            targetVector = null;
            vectorDirection = "";
            settings.Gyroscopes.ForEach(gyroscope => { (gyroscope as IMyGyro).Stop(); (gyroscope as IMyGyro).OverrideOff(); });
            IsRotating = false;
        }
        public void Tick()
        {
            if ((targetVector.HasValue) && vectorDirection != "")
                settings.Gyroscopes.ForEach(gyroscope =>
                {
                    IsRotating =
                        settings.UseGravityVector ?
                        !(gyroscope as IMyGyro).Rotate(
                         settings.OrientationReferenceBlock == null ? gyroscope.GetDirectionalVector(vectorDirection, true) : settings.OrientationReferenceBlock.GetDirectionalVector(vectorDirection, true),
                         settings.OrientationReferenceBlock.GetDirectionalVector("down",true),
                         targetVector.Value,
                         settings.Remote.GetNaturalGravity(), 
                         settings.GyroCoEff
                        )
                        :
                        !(gyroscope as IMyGyro).Rotate(targetVector.Value, 
                        settings.OrientationReferenceBlock == null ? gyroscope.GetDirectionalVector(vectorDirection,true) : settings.OrientationReferenceBlock.GetDirectionalVector(vectorDirection, true),
                        settings.GyroCoEff
                        );
                    
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
    public class YaNavGyroControlSettings
    {
        public IMyRemoteControl Remote;
        public bool UseGravityVector = true;
        public float GyroCoEff = 1f;
        public IMyTerminalBlock OrientationReferenceBlock;
        public List<IMyTerminalBlock> Gyroscopes;
        public YaNavGyroControlSettings()
        {
            Gyroscopes = new List<IMyTerminalBlock>();
        }
    }
}