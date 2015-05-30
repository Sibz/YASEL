using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Nav.NavSettings
{  
    public class NavSettings
    {

        public string LCDVariableStoreName,
                                LCDDebugScreenName,
                                LeftBlockName,
                                RightBlockName,
                                TopBlockName,
                                BottomBlockName,
                                FwdBlockName,
                                RearBlockName,
                                CentreBlock,
                                GyroGroupName,
                                FwdThrustGroupName,
                                RvsThrustGroupName,
                                LeftThrustGroupName,
                                RightThrustGroupName,
                                UpThrustGroupName,
                                DownThrustGroupName,
                                CockpitName;

        public double AlignMargin, MaxSpeed, StoppingDistance, DefaultThrust, ConnectorSpeed, BreakingDistMultiplier;

        public int DebugLevel, TicksPerSec, ActionTick, MinThrust;

        public NavSettings()
        {
            LCDVariableStoreName = "LCD Variable Store";
            LCDDebugScreenName = "LCD Debug";
            DebugLevel = 1;
            TicksPerSec = 60;
            ActionTick = 15;
            AlignMargin = 0.005;
            MaxSpeed = 100;
            StoppingDistance = 10;
            MinThrust = 5;
            DefaultThrust = 100;
            ConnectorSpeed = 20; // Speed at which to approach connector WP
            BreakingDistMultiplier = 20; // Speed * this is when you'll start breaking.
            LeftBlockName = "GPS Left";
            RightBlockName = "GPS Right";
            TopBlockName = "GPS Top";
            BottomBlockName = "GPS Bottom";
            RearBlockName = "GPS Rear";
            FwdBlockName = "GPS Fwd";
            GyroGroupName = "Ship Gyros";
            FwdThrustGroupName = "Forward Thrusters";
            RvsThrustGroupName = "Reverse Thrusters";
            LeftThrustGroupName = "Left Thrusters";
            RightThrustGroupName = "Right Thrusters";
            UpThrustGroupName = "Up Thrusters";
            DownThrustGroupName = "Down Thrusters";
            CentreBlock = "rear";
        }
    }
}
