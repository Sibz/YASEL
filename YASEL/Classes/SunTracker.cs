using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace SunTracker
{
    using ProgramExtensions;
    using SolarExtensions;
    using RotorExtensions;

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

        float lastAngle = 0f, lastPowerReading = 0f, minPower;

        PowerReadings powerReadings = new PowerReadings(3);
        int movementRestCount = 0;

        public SolarArray(MyGridProgram gp, string panelName, string rotorName, float rotorVelocity = 0.1f, float minPower = 0.04f)
        {
            this.gp = gp;
            this.rotorVelocity = rotorVelocity;
            this.minPower = minPower;
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
                if (panel.MaxOutput > 0f)
                {
                    lastPowerReading = panel.MaxOutput;
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
            
            
            var currentPower = panel.MaxOutput;
            powerReadings.addReading(currentPower);
            float diff = (float)Math.Round(currentPower - powerReadings.avg(), 7);
            
            gp.Echo("Track:\n Curent Power:" + currentPower + "\n powerAvg: " + powerReadings.avg() + "\n Moving: " + rotor.GetValueFloat("Velocity"));
            gp.Echo("Diff:" + Math.Round(diff * 1000*1000,4) + "W");
            
            if (movementRestCount>0)
            {
                gp.Echo("Recent movement made, resting");
                movementRestCount--;
                return;
            }
            
           
            if (diff < 0 && rotor.GetValueFloat("Velocity")==0f)
            {
                gp.Echo("Track: power is less and rotor is not moving, so moving rotor.");
                rotor.SetValueFloat("Velocity", rotorVelocity);
                movementRestCount = 2;
            }
            else if (diff <= 0 && rotor.GetValueFloat("Velocity") != 0f)
            {
                    gp.Echo("Track: power is less or equal and rotor IS moving, so stopping rotor.");
                    rotor.SetValueFloat("Velocity", 0f);
                movementRestCount = 6;
            } 
            
            if (currentPower < 0.02f && lastPowerReading < 0.02f)
            {
                gp.Echo("Track: Two <0.02 power readouts, resseting.");

                state = resetting;

            }
            lastPowerReading = powerReadings.avg();

        }

        class PowerReadings
        {
            int qSize;
            public PowerReadings(int queueSize = 5)
            {
                qSize = queueSize;
            }
            Queue<float> readings = new Queue<float>();
            public void addReading(float r)
            {
                readings.Enqueue(r);
                if (readings.Count>qSize)
                    readings.Dequeue();
            }
            public float avg()
            {
                float total = 0f;
                foreach (var r in readings)
                    total += r;
                return total / readings.Count;
            }
        }
    }
}