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
        YaNavGyroControlSettings settings;
        Vector3D? targetDirection;
        Vector3D? indicatorDirection;

        public bool IsRotating = false;


        public YaNavGyroControl(MyGridProgram gp, YaNavGyroControlSettings s)
        {
            this.gp = gp;
            this.settings = s;
            targetDirection = null;
            
            if (settings.Gyroscopes.Count == 0)
                gp.GridTerminalSystem.GetBlocksOfType<IMyGyro>(settings.Gyroscopes, b => { return b.CubeGrid == gp.Me.CubeGrid; });
            
            if (settings.Gyroscopes.Count == 0)
                throw new YaNavGyroControlException(lang.ErrorNoGyroBlocks);
        }

        public void SetTargetAndIndicator(Vector3D target, Vector3D? indicator = null)
        {
            targetDirection = target;
            indicatorDirection = indicator ?? settings.OrientationReferenceBlock.GetDirectionalVector("forward", true);
        }
        public void ClearVectorAndDirection()
        {
            targetDirection = null;
            indicatorDirection = null;
            settings.Gyroscopes.ForEach(gyroscope => { (gyroscope as IMyGyro).Stop(); (gyroscope as IMyGyro).OverrideOff(); });
            IsRotating = false;
        }
        public void Tick()
        {
            if ((targetDirection.HasValue) && (indicatorDirection.HasValue))
                settings.Gyroscopes.ForEach(gyroscope =>
                {
                    IsRotating =
                        settings.UseGravityVector ?
                        !(gyroscope as IMyGyro).Rotate(
                         indicatorDirection.Value,
                         settings.OrientationReferenceBlock.GetDirectionalVector("down",true),
                         targetDirection.Value,
                         settings.Remote.GetNaturalGravity(), 
                         settings.GyroCoEff,
                         settings.GyroAccuracy
                        )
                        :
                        !(gyroscope as IMyGyro).Rotate(targetDirection.Value,
                        indicatorDirection.Value,
                        settings.GyroCoEff,
                        settings.GyroAccuracy
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
        public float GyroAccuracy = 0.01f;
        public IMyTerminalBlock OrientationReferenceBlock;
        public List<IMyTerminalBlock> Gyroscopes;
        public YaNavGyroControlSettings()
        {
            Gyroscopes = new List<IMyTerminalBlock>();
        }
    }
}