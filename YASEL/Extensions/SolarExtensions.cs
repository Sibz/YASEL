using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace SolarExtensions
{
    public static class SolarExtensions
    {
        public static float GetCurrentPowerOutput(this IMySolarPanel solarPanel)
        {
            string[] lines = solarPanel.DetailedInfo.Split('\n');
            if (lines.Length < 3)
                throw new Exception ("SolarExtensions.GetCurrentPowerOutput\n - Unable to get lines from detailed info");
            string[] words = lines[2].Split(' ');
            if (words.Length < 3)
                throw new Exception ("SolarExtensions.GetCurrentPowerOutput\n - Unable to get words from detailed info");

            float data = Convert.ToSingle(words[2]);
            string unit = words[3];
            data = unit == "kW" ? data / 1000 : data;
            return data;
        }
    }
}