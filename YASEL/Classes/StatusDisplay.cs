using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace StatusDisplay
{
    using ProgramExtensions;
    using TextPanelExtensions;
    using RotorExtensions;

    public class StatusDisplay
    {
        public MyGridProgram gp;
        List<StatusDisplayLCD> lcds = new List<StatusDisplayLCD>();
        StatusDisplaySettings settings;
        public int incrementer = 0;

        public Dictionary<string, StatusDisplayModule> Modules = new Dictionary<string, StatusDisplayModule>();
        public StatusDisplay(MyGridProgram gp, StatusDisplaySettings settings = null)
        {
            this.gp = gp;
            this.settings = settings == null ? new StatusDisplaySettings() : settings;
            if (this.settings.lcds == null)
            {
                this.settings.lcds = new List<IMyTerminalBlock>();
                gp.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(this.settings.lcds, gp.OnGrid);
            }
            gp.dbout("Settings loaded. Cylcing " + this.settings.lcds.Count + " TextPanels");
            foreach (IMyTextPanel lcd in this.settings.lcds)
            {
                var pt = lcd.GetPrivateTitle();
                gp.dbout("Checking TextPanel " + lcd.CustomName + " which has pt of:\n " + pt);
                if (!pt.Contains("$display"))
                    continue;
                gp.dbout("Found LCD with trigger title");
                if (pt.Contains("$displaySpan#"))
                {
                    try
                    {
                        var newLCDS = gp.SearchBlocks(pt.Split('#')[1], true);
                        if (newLCDS.Count > 0)
                            lcds.Add(new StatusDisplayLCDSpan(this, newLCDS));
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException("StatusDisplay Ctor: Error constructing SpannedLCDS for " + lcd.CustomName + " - '" + pt + "'" + "\n\nInnerExceptoion:\n" + e.Message);
                    }
                }
                else
                {
                    lcds.Add(new StatusDisplayLCD(this, lcd));
                }
            }
        }
        public void AddModule(StatusDisplayModule mod)
        {
            gp.dbout("Adding Module " + mod.CommandName);
            Modules.Add(mod.CommandName, mod);
        }
        public void UpdateDisplays()
        {
            foreach (StatusDisplayLCD lcd in lcds)
            {
                lcd.Update();
            }
        }

    }

    public class StatusDisplaySettings
    {
        public List<IMyTerminalBlock> lcds = null;
        public bool debug = true;
    }

    public class StatusDisplayLCDSpan : StatusDisplayLCD
    {
        List<IMyTerminalBlock> lcds = new List<IMyTerminalBlock>();
        public StatusDisplayLCDSpan(StatusDisplay sd, List<IMyTerminalBlock> lcds) : base(sd)
        {
            sd.gp.dbout("Creating new SpannedLCD set with " + lcds.Count + " lcds");
            if (lcds.Count == 0)
                throw new InvalidOperationException("SpannedLCDS Write(): lcds.Count==0 ");
            else if (lcds == null)
                throw new NullReferenceException("SpannedLCDS Write(): lcds==null ");
            this.lcds = lcds;
            lcds.Sort((x, y) => { return x.CustomName.CompareTo(y.CustomName); });
            lcd = lcds[0] as IMyTextPanel;
            loadCommands();
        }
        internal override void Write()
        {
            if (lcds.Count == 0)
                throw new InvalidOperationException("SpannedLCDS Write(): lcds.Count==0 ");
            else if (lcds == null)
                throw new NullReferenceException("SpannedLCDS Write(): lcds==null ");
            lcds.WriteToScreens(displayString);
        }
    }

    public class BaseStatusDisplayLCD
    {
        internal StatusDisplay sd;
        public BaseStatusDisplayLCD(StatusDisplay sd)
        {
            this.sd = sd;
        }
    }
    public class StatusDisplayLCD : BaseStatusDisplayLCD
    {
        internal IMyTextPanel lcd;
        internal List<StatusDisplayCommand> commands = new List<StatusDisplayCommand>();
        internal string displayString = "";
        internal string textWithSubstitutedCommands = "";

        public StatusDisplayLCD(StatusDisplay sd) : base(sd)
        {

        }
        public StatusDisplayLCD(StatusDisplay sd, IMyTextPanel lcd) : base(sd)
        {
            sd.gp.dbout("Creating new LCD");
            if (lcd == null)
                throw new NullReferenceException("LCD Ctor: LCD is Null");
            this.lcd = lcd;
            loadCommands();
        }
        public void Update()
        {
            Dictionary<int, string> commandResults = new Dictionary<int, string>();

            foreach (StatusDisplayCommand cmd in commands)
            {
                sd.gp.dbout("Updating command " + cmd.Id + " " + cmd.Name);
                commandResults.Add(Convert.ToInt32(cmd.Id), cmd.Execute(Convert.ToInt32(cmd.Id)));
            }
            sd.gp.dbout("Replacing commands:" + textWithSubstitutedCommands);
            displayString = replaceSubstitutedArgs(textWithSubstitutedCommands, commandResults);
            Write();
        }
        internal void loadCommands()
        {
            // display:Write("Batteries:\n");BatteryInfo(group="Station Batteries",count=true);
            string rawText = lcd.GetPrivateText();
            // Batteries:\n{batteryInfo(group = 'groupName' , count=false)}

            var chars = rawText.ToCharArray();
            sd.gp.dbout("LCD: Loading Commands...");
            string newCommand = "";
            bool inBrackets = false;
            foreach (char c in chars)
            {
                if (!inBrackets && c != '{')
                    textWithSubstitutedCommands += c;
                else if (!inBrackets && c == '{')
                {
                    inBrackets = true;
                    newCommand = "";
                }
                else if (inBrackets && c != '}')
                    newCommand += c;
                else if (inBrackets && c == '}')
                {
                    sd.gp.dbout("Creating new arg from: " + newCommand);
                    var cmd = new StatusDisplayCommand(sd, newCommand, sd.incrementer++);
                    commands.Add(cmd);
                    textWithSubstitutedCommands += "{#%" + cmd.Id + "}";
                    inBrackets = false;
                }
            }

        }
        static public string replaceSubstitutedArgs(string arg, Dictionary<int, string> substitutedArgs)
        {

            var check = System.Text.RegularExpressions.Regex.Match(arg, @"{#%\d{1,7}}");
            while (check.Success)
            {
                var indexParts = check.Value.Split('%');
                if (indexParts.Length < 2) continue;
                try
                {
                    var index = Convert.ToInt32(indexParts[1].Trim('}'));
                    arg = substitutedArgs.ContainsKey(index) ? arg.Replace(check.Value, substitutedArgs[index]) : arg;

                }
                catch (Exception e)
                {

                    throw new Exception("Unable to convert index " + indexParts[1].Split('}')[0] + " from \n " + arg + "\n\nInner Exception:" + e.Message);
                }
                check = check.NextMatch();
            }
            return arg;
        }
        internal virtual void Write()
        {
            if (lcd == null)
                throw new NullReferenceException("LCD Write: LCD is Null");
            lcd.WritePublicText(displayString);
        }
    }
    public class StatusDisplayCommand

    {
        StatusDisplay sd;
        public StatusDisplayCommand(StatusDisplay sd, string rawCommand, int instanceId)
        {
            this.sd = sd;
            Name = rawCommand.Split('(')[0];
            Id = instanceId.ToString();
            string rawArgs = rawCommand.Substring(rawCommand.IndexOf('(') + 1).Trim(')');
            string argsWithSubstitutedQuotedStrings = "";
            string quotedString = "";
            Dictionary<int, string> quotedStrings = new Dictionary<int, string>();
            bool inQuotes = false;
            foreach (char c in rawArgs)
            {
                if (!inQuotes && c != '"')
                    argsWithSubstitutedQuotedStrings += c;
                else if (!inQuotes && c == '"')
                {
                    inQuotes = true;
                    quotedString = "";
                }
                else if (inQuotes && c != '"')
                    quotedString += c;
                else if (inQuotes && c == '"')
                {
                    argsWithSubstitutedQuotedStrings += "{#%" + quotedStrings.Count + "}";
                    quotedStrings.Add(quotedStrings.Count, quotedString);
                    inQuotes = false;
                }
            }
            argsWithSubstitutedQuotedStrings = argsWithSubstitutedQuotedStrings.Replace("\n", "").Replace(" ", "");
            var argList = argsWithSubstitutedQuotedStrings.Split(',');
            sd.gp.dbout("Cycling arglist from " + argsWithSubstitutedQuotedStrings);
            foreach (string arg in argList)
            {
                sd.gp.dbout("Creating arg from " + StatusDisplayLCD.replaceSubstitutedArgs(arg, quotedStrings));
                var argParts = arg.Split('=');
                if (argParts.Length == 1)
                    Args.Add("default", StatusDisplayLCD.replaceSubstitutedArgs(arg, quotedStrings));
                else
                    Args.Add(argParts[0], StatusDisplayLCD.replaceSubstitutedArgs(argParts[1], quotedStrings));
            }


        }
        public string Id;
        public string Name;
        public Dictionary<string, string> Args = new Dictionary<string, string>();
        public string Execute(int instanceId)
        {
            if (sd.Modules.ContainsKey(Name))
                return sd.Modules[Name].Execute(instanceId, Args);
            return "Module " + Name + " Not Loaded";
        }
    }
    public abstract class StatusDisplayModule
    {
        public StatusDisplay sd;
        internal int id;
        internal int currentInstanceId;
        internal Dictionary<string, string> defaultArgs = new Dictionary<string, string>();
        internal Dictionary<int, Dictionary<string, string>> instanceArgs = new Dictionary<int, Dictionary<string, string>>();
        internal Dictionary<int, Dictionary<string, string>> instanceValues = new Dictionary<int, Dictionary<string, string>>();
        public StatusDisplayModule(StatusDisplay sd, Dictionary<string, string> defaultArgs, int id = -1)
        {
            this.sd = sd;
            this.id = id;
            defaultArgs.Add("pad", "");
            defaultArgs.Add("group", "#all#");
            this.defaultArgs = defaultArgs;
        }
        internal abstract string commandName { get; }
        public string CommandName { get { return commandName + (id == -1 ? "" : "-" + id); } }
        public string Execute(int instanceId, Dictionary<string, string> args)
        {
            sd.gp.dbout("Executing " + CommandName + "/" + instanceId);
            currentInstanceId = instanceId;
            if (!instanceArgs.ContainsKey(instanceId))
                instanceArgs.Add(instanceId, new Dictionary<string, string>());
            else
                instanceArgs[instanceId].Clear();

            foreach (var defaultArg in defaultArgs)
            {
                if (args.ContainsKey(defaultArg.Key))
                    instanceArgs[instanceId].Add(defaultArg.Key, args[defaultArg.Key]);
                else
                    instanceArgs[instanceId].Add(defaultArg.Key, defaultArg.Value);
            }
            if (!instanceValues.ContainsKey(currentInstanceId)) instanceValues.Add(currentInstanceId, new Dictionary<string, string>());
            sd.gp.dbout("Loaded args");

            // Update values
            update();

            return buildString();

        }
        private string buildString()
        {

            string result = "";

            if (instanceArgs[currentInstanceId].ContainsKey("display"))
            {

                var itemsToDisplay = getArg("display").Split(';');
                foreach (var item in itemsToDisplay)
                {
                    if (instanceValues[currentInstanceId].ContainsKey(item))
                        result += getArg("pad") +
                           getTypedValue(item) +
                           "\n";
                }
            }

            return result.TrimEnd('\n');
        }
        internal float getValueFloat(string key)
        {
            return Convert.ToSingle(getValue(key, true));
        }
        internal void setValueFloat(string key, float value)
        {
            setValue(key, value.ToString());
        }
        internal int getValueInt(string key)
        {
            return Convert.ToInt32(getValue(key, true));
        }
        internal void setValueInt(string key, int value)
        {
            setValue(key, value.ToString());
        }
        internal bool getValueBool(string key)
        {
            return Convert.ToBoolean(getValue(key, true));
        }
        internal void setValueBool(string key, bool value)
        {
            setValue(key, value.ToString());
        }
        internal string getValue(string key, bool zeroOnError = false)
        {
            sd.gp.dbout("Getting instance value:" + key);
            if (instanceValues[currentInstanceId].ContainsKey(key))
            {
                sd.gp.dbout(key + " = " + instanceValues[currentInstanceId][key]);
                return instanceValues[currentInstanceId][key];
            }
            return zeroOnError ? "0" : "";
        }
        internal string getValuePercent(string key)
        {
            return getValueFloat(key) * 100 + "%";
        }
        internal string getValueTime(string key)
        {
            TimeSpan chargeTime = new TimeSpan(0, 0, 0, 0, getValueInt(key));
            return (chargeTime.Hours > 0 ? chargeTime.Hours + "Hr" + (chargeTime.Hours > 1 ? "s " : " ") : " ") + chargeTime.Minutes + "min";
        }
        internal string getValuePower(string key)
        {
            var power = getValueFloat(key);
            var unit = "MW";
            if (power < 1)
            {
                power = power * 1000;
                unit = "kW";
            }
            else if (power < 0.001)
            {
                power = power * 1000 * 1000;
                unit = "W";
            }
            return Math.Round(power, 2) + unit;
        }
        public void setValue(string key, string value)
        {
            if (instanceValues[currentInstanceId].ContainsKey(key))
                instanceValues[currentInstanceId][key] = value;
            else
                instanceValues[currentInstanceId].Add(key, value);
        }
        internal float getArgFloat(string key)
        {
            return Convert.ToSingle(getArg(key));
        }
        internal int getArgInt(string key)
        {
            return Convert.ToInt32(getArg(key));
        }
        internal bool getArgBool(string key)
        {
            return Convert.ToBoolean(getArg(key));
        }
        internal string getArg(string key)
        {
            if (instanceArgs[currentInstanceId].ContainsKey(key))
                return instanceArgs[currentInstanceId][key];
            return "";
        }
        internal abstract void update();

        internal string getTypedValue(string key, bool withPrefix = true)
        {
            string result = "";
            if (withPrefix && instanceArgs[currentInstanceId].ContainsKey(key + "Prefix"))
                result += getArg(key + "Prefix");
            if (instanceArgs[currentInstanceId].ContainsKey(key + "Type"))
            {
                switch (getArg(key + "Type"))
                {
                    case "time":
                        result += getValueTime(key);
                        break;
                    case "percent":
                        result += getValuePercent(key);
                        break;
                    case "power":
                        result += getValuePower(key);
                        break;
                }
            }
            else
                result += getValue(key);
            return result;
        }
        internal bool hasArg(string key)
        {
            return instanceArgs[currentInstanceId].ContainsKey(key);
        }
        internal bool hasValue(string key)
        {
            return instanceValues[currentInstanceId].ContainsKey(key);
        }
        internal void removeArg(string key)
        {
            if (hasArg(key))
                instanceArgs[currentInstanceId].Remove(key);
        }
        internal void removeValue(string key)
        {
            if (hasValue(key))
                instanceValues[currentInstanceId].Remove(key);
        }
    }


}