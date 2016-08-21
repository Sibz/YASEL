using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AutoLevelProgram
{
    using ProgramExtensions;
    using GyroExtensions;

    class AutoLevelProgram : MyGridProgram
    {
        /*
          * To Use:  
          *
          * Install 1 Remote, 1 Timer, 1 Programmable block and at least 1 Gyroscope (may need more for bigger ships)
          * Install Remote inline with ship
          * Install Gyro inline with ship
          * Install Programmable block, paste and modify this code
          * Install Timer, Add Actions: Programmable block with no argument, and Timer Trigger
          * Add shortcut to bar to Programmable block with argument "toggle"
          *  This will toggle autolevel on/off
          *  Alternatively you can have two shortcuts, arguments on / off to turn on and off respectivly.
          * Trigger timer to start program loop - you may need to do this upon load or after a power cut.
          *  It may be a good idea to have a shortcut on bar to trigger time

          * Notes:
          * Gyroscope is inline with ship when:
          *   flat face down and the circle pointing up/forward
          * Remote is inline when 't' part of the remote is facing forward with the slanted edge up
          *
          * Also use at least one gyro for this program and atleast one for your control
          *  so you can maintain some control when leveling.
          */

        /*
         * Settings  
         * 
         * coEff - Make this higher if responding too slowly, or lower if yoyo-ing
         * 
         * accuracy - Set higher if having trouble stopping yoyo-ing with above 
         *          - or set lower if its stopping when not very level
         *          
         * rollOnly - set to true to not adjust pitch to be level
         * 
         * remoteName - Name of the remote to use
         * 
         * *use only one of these*
         * gyroName - Name of the gyroscope to use (Should be "" if only usin group of gyroscopes)
         * 
         * gyroGroup - Name of group of gyroscopes to use (Should be "" if only usin single gyro)
         * 
         */

        float coEff = 0.2f; // This is good for small light ship 

        float accuracy = 0.2f;

        bool rollOnly = false;

        string remoteName = ("Remote");

        string gyroName = "Gyroscope 3";

        string gyroGroup = "";

        /*
         * Leave these as is
         */
        int ticks = 0;
        int ticksPerRun = 15;
        bool autoLevelOn = false;

        void Main(string argument)
        {

            IMyTerminalBlock remote = this.GetBlock(remoteName);
            List<IMyTerminalBlock> gyros;
            if (gyroName != "")
                gyros = new List<IMyTerminalBlock>() { (this.GetBlock(gyroName) as IMyGyro) };
            else
                gyros = this.GetBlockGroup(gyroGroup);

            if (argument.IndexOf("on", StringComparison.InvariantCultureIgnoreCase) != -1)
                autoLevelOn = true;
            else if (argument.IndexOf("off", StringComparison.InvariantCultureIgnoreCase) != -1)
                autoLevelOn = false;
            else if (argument.IndexOf("toggle", StringComparison.InvariantCultureIgnoreCase) != -1)
                autoLevelOn = !autoLevelOn;

            if (!autoLevelOn)
            {
                gyros.Stop();
                gyros.OverrideOff();
            }
            else if (ticks % ticksPerRun == 0)
            {
                gyros.Rotate(
                    (remote as IMyRemoteControl).GetNaturalGravity(),
                    remote.GetDirectionalVector("down"),
                    coEff, accuracy);
                if (rollOnly) gyros.SetPitch(0f);
                if (!gyros.IsRolling(accuracy / 2) && (rollOnly || !gyros.IsPitching(accuracy / 2)))
                    gyros.OverrideOff();
            }
            ticks++;
        }

    }
}

