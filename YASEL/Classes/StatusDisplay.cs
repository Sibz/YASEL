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
        List<StatusDisplayLCD> displays = new List<StatusDisplayLCD>();
        StatusDisplaySettings settings;
        public int incrementer = 0;

        public Dictionary<string, StatusDisplayModule> Modules = new Dictionary<string, StatusDisplayModule>();
        public StatusDisplay(MyGridProgram gp, StatusDisplaySettings settings = null)
        {
            this.gp = gp;
            this.settings = settings == null ? new StatusDisplaySettings() : settings;
            foreach (var module in settings.Modules)
            {
                if (!Modules.ContainsKey(module.CommandName))
                    AddModule(module);
            }
            if (this.settings.textPanels == null)
            {
                this.settings.textPanels = new List<IMyTerminalBlock>();
                gp.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(this.settings.textPanels, gp.OnGrid);
            }
            gp.dbout("Settings loaded. Cylcing " + this.settings.textPanels.Count + " TextPanels");
            importLCDs();
        }
        private void importLCDs()
        {
            foreach (IMyTextPanel lcd in this.settings.textPanels)
            {
                if (displays.Find(x => x.Name == lcd.CustomName) != null)
                    continue;
                
                var pt = lcd.GetPrivateTitle();
                //gp.dbout("Checking TextPanel " + lcd.CustomName + " which has pt of:\n " + pt);
                if (!pt.Contains("$display"))
                    continue;
                //gp.dbout("Found LCD with trigger title");

                var statusDisplayLCDSettings = new StatusDisplayLCDSettings();
                if (pt.Contains("$displaySpan#"))
                {
                    try
                    {
                        statusDisplayLCDSettings.Displays = gp.SearchBlocks(pt.Split('#')[1], true);
                        statusDisplayLCDSettings.Displays.Sort((x, y) => { return x.CustomName.CompareTo(y.CustomName); });
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException("StatusDisplay Ctor: Error constructing SpannedLCDS for " + lcd.CustomName + " - '" + pt + "'" + "\n\nInnerExceptoion:\n" + e.Message);
                    }
                }
                else
                {
                    statusDisplayLCDSettings.Displays = new List<IMyTerminalBlock>() { lcd };
                }
                displays.Add(new StatusDisplayLCD(this, statusDisplayLCDSettings));
            }
        }
        public void AddModule(StatusDisplayModule mod)
        {
            gp.dbout("Adding Module " + mod.CommandName);
            Modules.Add(mod.CommandName, mod);
        }
        public void UpdateDisplays()
        {
            incrementer = 0;
            importLCDs();
            foreach (StatusDisplayLCD lcd in displays)
            {
                gp.dbout("updating display...");
                lcd.Update();
            }
        }

    }

    public class StatusDisplaySettings
    {
        public List<IMyTerminalBlock> textPanels = null;
        public List<StatusDisplayModule> Modules = new List<StatusDisplayModule>();
        public bool debug = true;
    }

    public class StatusDisplayLCDSettings
    {
        public IMyTextPanel PrimaryLCD { get { return Displays[0] as IMyTextPanel; } }  
        public List<IMyTerminalBlock> Displays = new List<IMyTerminalBlock>();
    }

    public class StatusDisplayLCD 
    {
        internal List<StatusDisplayCommand> commands = new List<StatusDisplayCommand>();
        Dictionary<int, string> commandResults = new Dictionary<int, string>();
        internal string displayString = "";
        internal string textWithSubstitutedCommands = "";
        StatusDisplayLCDSettings settings;
        StatusDisplay sd;
        public string Name;

        public StatusDisplayLCD(StatusDisplay sd, StatusDisplayLCDSettings settings)
        {
            this.sd = sd;
            this.settings = settings;
            sd.gp.dbout("Creating new LCD");
            if (this.settings.PrimaryLCD == null)
                throw new NullReferenceException("LCD Ctor: settings.primaryLCD is Null");
            this.Name = settings.PrimaryLCD.CustomName;

        }
        private void parseCommands()
        {
            sd.gp.dbout("parsing commands...");
            commandResults.Clear();
            commands.Clear();
            textWithSubstitutedCommands = settings.PrimaryLCD.GetPrivateText();
            var commandstringMatch = System.Text.RegularExpressions.Regex.Match(textWithSubstitutedCommands, @"{[^#][^}]*}");
            while(commandstringMatch.Success)
            {
                int id = sd.incrementer++;
                textWithSubstitutedCommands = textWithSubstitutedCommands.Replace(commandstringMatch.Value, "{#%" + id + "}");
                var commandMatch = System.Text.RegularExpressions.Regex.Match(commandstringMatch.Value, @"{(.*?)[}(]");
                if (commandMatch.Success)
                {
                    var argsMatch = System.Text.RegularExpressions.Regex.Match(commandstringMatch.Value, @"\(([\s\S]*?)\)");
                    var cmdMatchTrimmed = commandMatch.Value.Trim('{').Trim('}').Trim('(');
                    sd.gp.dbout("Command Parts:\n -" + cmdMatchTrimmed);
                    var commandParts = cmdMatchTrimmed.Split('.');
                    var rawArgs = argsMatch.Success ? argsMatch.Value.Trim('(').Trim(')') : "";
                    if (commandParts.Length == 2)
                    {
                        sd.gp.dbout("Value request:" + commandParts[1] + " from " + commandParts[0]);
                        var command = commands.Find(x => { return x.Name == commandParts[0]; });
                        if (command != null)
                            commandResults.Add(id, command.GetValue(commandParts[1], parseArgs(rawArgs)));
                    }
                    else
                    {
                        sd.gp.dbout("New Command:" + commandParts[0]);
                        var newCmd = new StatusDisplayCommand(sd, commandParts[0], parseArgs(rawArgs), id);
                        commands.Add(newCmd);
                        commandResults.Add(id, newCmd.Execute());
                    }
                }
                else
                    throw new Exception("Unable to parseCommands from LCD");

                commandstringMatch = System.Text.RegularExpressions.Regex.Match(textWithSubstitutedCommands, @"{[^#][^}]*}");
            }
            sd.gp.dbout("Parsed Commands:");
            foreach(var cmd in commands)
            {
                sd.gp.dbout(cmd.Name + " - " + cmd.Id);
            }
        }
        
        private Dictionary<string, string> parseArgs(string rawArgs)
        {
            sd.gp.dbout("Parsing Args... (" + rawArgs + ")");
            var result = new Dictionary<string, string>();
            // Remove quoted strings and replace with a placeholder so we don't get confused with quoted , and =
            Dictionary<int, string> removedStrings = new Dictionary<int, string>();
            var quotedStringMatches = System.Text.RegularExpressions.Regex.Matches(rawArgs, @"""([\s\S]*?)""");

            foreach (System.Text.RegularExpressions.Match quotedStringMatch in quotedStringMatches)
            {
                int id = removedStrings.Count;
                if (quotedStringMatch.Success)
                    removedStrings.Add(id, quotedStringMatch.Value.Replace("\"",""));
                rawArgs = rawArgs.Replace(quotedStringMatch.Value, "{#%" + id + "}");
            }
            sd.gp.dbout("Removed Strings\n " + rawArgs);
            rawArgs = rawArgs.Replace("\n", "").Replace(" ", "");

            var argArray = rawArgs.Split(',');
            foreach (var arg in argArray)
            {
                var argParts = arg.Split('=');
                if (argParts.Length == 2) result.Add(argParts[0], replaceSubstitutedArgs(argParts[1], removedStrings));
            }

            sd.gp.dbout("Parsed Args:");
            foreach (var r in result)
            {
                sd.gp.dbout(r.Key + "/" + r.Value);
            }
            return result;
        }
        public void Update()
        {
            parseCommands();
            sd.gp.dbout("StatusDisplayLCD.Update:\n -Replacing commands in:\n\n" + textWithSubstitutedCommands);
            displayString = replaceSubstitutedArgs(textWithSubstitutedCommands, commandResults);
            sd.gp.dbout("Replaced. Writing to screens");
            Write();
            sd.gp.dbout("Screen text updated");
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
            if (settings.PrimaryLCD == null)
                throw new NullReferenceException("StatusDisplayLCD.Write: LCD is Null");
            if (settings.Displays.Count>1)
                settings.Displays.WriteToScreens(displayString);
            else
                settings.PrimaryLCD.WritePublicText(displayString);
        }
    }
    public class StatusDisplayCommand

    {
        StatusDisplay sd;
        public StatusDisplayCommand(StatusDisplay sd, string name, Dictionary<string,string> args, int id)
        {
            this.sd = sd;
            Name = name;
            Id = id;
            Args = args;
        }
        public int Id;
        public string Name;
        public Dictionary<string, string> Args = new Dictionary<string, string>();
        public string Execute()
        {
            if (sd.Modules.ContainsKey(Name))
                return sd.Modules[Name].Execute(Id, Args);
            return "Module " + Name + " Not Loaded";
        }

        internal string GetValue(string key, Dictionary<string, string> args)
        {
            if (!sd.Modules.ContainsKey(Name)) return "Module for value not loaded - " + Name + "." + key;
            sd.gp.Echo("Getting Value " + key + " from " + Name);

            if ( args.ContainsKey("formatAs"))
            {
                switch (args["formatAs"])
                {
                    case "time":
                        return sd.Modules[Name].getValueTime(key);
                    case "percent":
                        return sd.Modules[Name].getValuePercent(key);
                    case "power":
                        return sd.Modules[Name].getValuePower(key);
                    default:
                        return sd.Modules[Name].getValue(key);
                }
            }
            return sd.Modules[Name].getTypedValue(key);
        }
    }
    public abstract class StatusDisplayModule
    {
        public MyGridProgram gp;
        internal int id;
        internal int currentInstanceId;
        internal Dictionary<string, string> defaultArgs = new Dictionary<string, string>();
        internal Dictionary<int, Dictionary<string, string>> instanceArgs = new Dictionary<int, Dictionary<string, string>>();
        internal Dictionary<int, Dictionary<string, string>> instanceValues = new Dictionary<int, Dictionary<string, string>>();
        public StatusDisplayModule(MyGridProgram gp, Dictionary<string,string> defaultArgs = null, int id = -1)
        {
            this.gp = gp;
            this.id = id;

            setDefaultArg("pad", "");
            setDefaultArg("group", "#all#");
            setDefaultArgBool("onGrid", true);

            if (defaultArgs != null)
                foreach (var arg in defaultArgs)
                    setDefaultArg(arg.Key, arg.Value);


        }
        internal abstract string commandName { get; }
        public string CommandName { get { return commandName + (id == -1 ? "" : "-" + id); } }
        public string Execute(int instanceId, Dictionary<string, string> args)
        {
            gp.dbout("Executing " + CommandName + "/" + instanceId);
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
            gp.dbout("Loaded args");

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
            gp.dbout("Getting instance value:" + key);
            if (instanceValues[currentInstanceId].ContainsKey(key))
            {
                gp.dbout(key + " = " + instanceValues[currentInstanceId][key]);
                return instanceValues[currentInstanceId][key];
            }
            return zeroOnError ? "0" : "";
        }
        internal string getValuePercent(string key)
        {
            return Math.Round(getValueFloat(key) * 100,2) + "%";
        }
        internal string getValueTime(string key)
        {
            TimeSpan chargeTime = new TimeSpan(0, 0, 0, 0, getValueInt(key));
            return (chargeTime.Hours > 0 ? 
                chargeTime.Hours + "Hr" + (chargeTime.Hours > 1 ? "s " : " ") : 
                " ") + (chargeTime.Minutes > 0 ? chargeTime.Minutes + "min" : (chargeTime.Hours>0?"": chargeTime.Seconds + "sec"));
        }
        internal string getValuePower(string key, float? value = null)
        {
            var power = value.HasValue ? value.Value : getValueFloat(key);
            if (power == 0)
                return ("-");
            var unit = "MW";
            if (power < 1 && power > 0.001)
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
        internal void setArgBool(string key, bool val)
        {
            setArg(key, val.ToString());
        }
        internal string getArg(string key)
        {
            if (instanceArgs[currentInstanceId].ContainsKey(key))
                return instanceArgs[currentInstanceId][key];
            return "";
        }
        
        internal void setArg(string key, string val)
        {
            if (instanceArgs[currentInstanceId].ContainsKey(key))
                instanceArgs[currentInstanceId][key] = val;
            else
                instanceArgs[currentInstanceId].Add(key, val);
        }
        internal void setDefaultArgBool(string key, bool val)
        {
            setDefaultArg(key, val.ToString());
        }
        internal void setDefaultArg(string key, string val)
        {
            if (defaultArgs.ContainsKey(key))
                defaultArgs[key] = val;
            else
                defaultArgs.Add(key, val);
        }
        internal string getDefaultArg(string key)
        {
            if (defaultArgs.ContainsKey(key))
                return defaultArgs[key];
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
        internal void addValueDefinition(string name, string prefix = "", string type = "", bool displayByDefault = true)
        {
            if (displayByDefault)
                setDefaultArg("display", (getDefaultArg("display") + ";" + name).TrimStart(';'));
            if (prefix != "")
                setDefaultArg(name + "Prefix", prefix);
            if (type != "")
                setDefaultArg(name + "Type", type);
        }
    }


}