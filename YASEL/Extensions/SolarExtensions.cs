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
                return 0f;
            string[] words = lines[2].Split(' ');
            if (words.Length < 3)
                return 0f;

            float data = Convert.ToSingle(words[2]);
            string unit = words[3];
            data = unit == "kW" ? data * 1000 : data;
            return data;
        }
        public static float GetMaxPowerOutput(this IMySolarPanel solarPanel)
        {
            string[] lines = solarPanel.DetailedInfo.Split('\n');
            if (lines.Length < 3)
                return 0f;
            string[] words = lines[1].Split(' ');
            if (words.Length < 3)
                return 0f;

            float data = Convert.ToSingle(words[2]);
            string unit = words[3];
            data = unit == "kW" ? data * 1000 : data;
            return data;
        }
    }
}