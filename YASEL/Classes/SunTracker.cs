using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace SunTracker
{
    using ProgramExtensions;
    using SolarExtensions;
    using RotorExtensions;
    using BlockExtensions;

    public class SunTracker
    {
        MyGridProgram gp;
        Dictionary<string, SolarArray> solarArrays;

        public SunTracker(MyGridProgram gp)
        {
            this.gp = gp;
            solarArrays = new Dictionary<string, SolarArray>();
        }
        public void AddSolarArray(string panelName, string rotorName, float rotorVelocity = 0.1f)
        {
            solarArrays.Add(panelName + rotorName, new SolarArray(this.gp, panelName, rotorName, rotorVelocity));
        }
        public void TrackSun()
        {
            var solarArrayEnum = solarArrays.GetEnumerator();
            while(solarArrayEnum.MoveNext())
                solarArrayEnum.Current.Value.TrackSun();
        }
    }

    public class SolarArray
    {
        IMySolarPanel panel;
        IMyMotorStator rotor;
        float rotorVelocity;
        MyGridProgram gp;

        string state = waitingForSun;
        const string waitingForSun = "Waiting for sun";
        const string resetting = "Resetting";
        const string tracking = "Tracking";
        
        float lastAngle = 0f, lastPowerReading = 0f;
        int logPosition = 0;

        public SolarArray(MyGridProgram gp, string panelName, string rotorName, float rotorVelocity = 0.1f)
        {
            this.gp = gp;
            this.rotorVelocity = rotorVelocity;

            panel = gp.GetBlock(panelName, false) as IMySolarPanel;
            if (panel == null) throw new Exception("SunTracker: Unable to creat solar array, can not access panel: " + panelName);
            
            rotor = gp.GetBlock(rotorName, false) as IMyMotorStator;
            if (rotor == null) throw new Exception("SunTracker: Unable to creat solar array, can not access rotor: " + rotorName);
            
            state = resetting;
     
        }
        public void TrackSun()
        {
            gp.Echo("TrackSun State = " + state);
            if (state == resetting)
            {
                gp.Echo("resetting: velocity: " + rotor.GetValueFloat("Velocity") + "  -  angle diff:" + Math.Abs(lastAngle - (float)Math.Round(rotor.Angle,2)));
                if (rotor.GetValueFloat("Velocity") != -rotorVelocity*3)
                {
                    rotor.SetValueFloat("Velocity", -rotorVelocity*3);
                }
                else if (Math.Abs(lastAngle - (float)Math.Round(rotor.Angle, 2)) == 0f)
                {
                    rotor.SetValueFloat("Velocity", 0);
                    state = waitingForSun;
                }
                lastAngle = (float)Math.Round(rotor.Angle,2);
            } else if (state == waitingForSun)
            {
                if (panel.GetCurrentPowerOutput() > 0f)
                {
                    lastPowerReading = panel.GetCurrentPowerOutput();
                    state = tracking; // logging;
                }
            }
            else if (state == tracking)
            {
                track();
            }
        }
    
        private void track()
        {
            var currentPower = panel.GetCurrentPowerOutput();

            gp.Echo("Track:\n Curent Power:" + currentPower + "\n LastPower: " + lastPowerReading + "\n Moving: " + rotor.GetValueFloat("Velocity"));

            if (currentPower<lastPowerReading && rotor.GetValueFloat("Velocity")==0f)
            {
                gp.Echo("Track: power is less and rotor is not moving, so moving rotor.");
                rotor.SetValueFloat("Velocity", rotorVelocity);
            }
            else if (currentPower <= lastPowerReading && rotor.GetValueFloat("Velocity") != 0f)
            {
                gp.Echo("Track: power is less or equal and rotor IS moving, so stopping rotor.");
                rotor.SetValueFloat("Velocity", 0f);
            } 
            
            if (currentPower ==0f && lastPowerReading == 0f)
            {
                gp.Echo("Track: Two 0 power readouts, resseting.");

                state = resetting;
            }
            lastPowerReading = currentPower;

        }

    }
}