using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace TextPanelExtensions
{
    /// <summary>
    /// Static class for common TextPanel functions
    /// </summary>
    using ProgramExtensions;
    static class TextPanelExtensions
    {
        static public Dictionary<double, int> LinePerFontSize = linePerFontSize();

        public static void WriteToScreens(this List<IMyTerminalBlock> screens, string text, MyGridProgram gp = null)
        {
            if (gp != null)
            {
                var e = LinePerFontSize.GetEnumerator();
                while (e.MoveNext())
                    gp.dbout("k:" + e.Current.Key + " / " + e.Current.Value, "TP:WriteToScreens");
            }
            screens.Sort((x, y) => { return x.CustomName.CompareTo(y.CustomName); });
            var lines = text.Split('\n');
            int currentLine = 0;
            foreach (var screen in screens)
            {
                if (screen is IMyTextPanel)
                {
                    int linesWrittenToThisScreen = 0;
                    var fontSize = Math.Round(screen.GetValueFloat("FontSize"), 1);
                    (screen as IMyTextPanel).WritePublicText("");
                    while (currentLine < lines.Length && linesWrittenToThisScreen < LinePerFontSize[fontSize])
                    {
                        var line = lines[currentLine];
                        if (line.Contains("**line**"))
                        {
                            line = drawLine(screen, line);
                        }
                        (screen as IMyTextPanel).WritePublicText(line + "\n", true);
                        currentLine++;
                        linesWrittenToThisScreen++;
                    }
                }
            }
        }

        public static Dictionary<String, double> GetValueList(this IMyTextPanel lcd)
        {
            Dictionary<String, double> valueList = new Dictionary<String, double>();

            if ((lcd is IMyTextPanel) && lcd.IsFunctional)
            {
                string strText = lcd.GetPublicText();
                string[] lines = strText.Split(new char[] { '\n' });
                Array.ForEach(lines, line =>
                {
                    if (line.Contains(":"))
                    {
                        string[] vals = line.Split(new char[] { ':' });
                        valueList.Add(vals[0], Convert.ToDouble(vals[1].Trim()));
                    }
                });
            }
            return valueList;
        }
        private static string drawLine(this IMyTerminalBlock textPanel, string line)
        {
            var fontSize = (textPanel as IMyTextPanel).GetValueFloat("FontSize");
            string result = "";
            var multiplier = (118 / fontSize) - (fontSize / 6) - line.IndexOf("**line**");
            if (!textPanel.BlockDefinition.SubtypeName.Contains("Wide")) multiplier = multiplier/2-1;
            for (float i = 0; i <= multiplier; i++)
            {
                result += "-";
            }
            return line.Replace("**line**", result);
        }
        static private Dictionary<double, int> linePerFontSize()
        {
            var result = new Dictionary<double, int>();
            result.Add(0.1, 178);
            result.Add(0.2, 89);
            result.Add(0.3, 59);
            result.Add(0.4, 44);
            result.Add(0.5, 35);
            result.Add(0.6, 29);
            result.Add(0.7, 25);
            result.Add(0.8, 22);
            result.Add(0.9, 19);
            result.Add(1.0, 18);
            result.Add(1.1, 16);
            result.Add(1.2, 15);
            result.Add(1.3, 13);
            result.Add(1.4, 12);
            result.Add(1.5, 12);
            result.Add(1.6, 11);
            result.Add(1.7, 10);
            result.Add(1.8, 10);
            result.Add(1.9, 9);
            result.Add(2, 9);
            result.Add(2.1, 8);
            result.Add(2.2, 8);
            result.Add(2.3, 7);
            result.Add(2.4, 7);
            result.Add(2.5, 7);
            result.Add(2.6, 7);
            result.Add(2.7, 6);
            result.Add(2.8, 6);
            result.Add(2.9, 6);
            result.Add(3, 6);
            for (double i = 3.1; i <= 3.71; i += 0.1)
                result.Add(Math.Round(i, 1), 5);
            for (double i = 3.8; i <= 4.6; i += 0.1)
                result.Add(Math.Round(i, 1), 4);
            for (double i = 4.7; i <= 6.2; i += 0.1)
                result.Add(Math.Round(i, 1), 3);
            for (double i = 6.3; i <= 10; i += 0.1)
                result.Add(Math.Round(i, 1), 2);
            return result;
        }
    }
}
