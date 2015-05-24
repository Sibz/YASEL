using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game;


namespace YASEL
{
    partial class Program
    {
        #region VarStoreClass
        //# Requires LCDFunctions
        //# Requires BlockFunctions
        //# Requires GenericFunctions
        public class VarStore
        {
            public string LCDDebugScreenName,
                                            LCDVariableStoreName;
            public IMyTextPanel LCDVariableStore;

            private bool varStoreInit;
            private Dictionary<string, string> varList;

            public VarStore(string lcdName = "LCDVariableStore")
            {
                LCDVariableStoreName = lcdName;
                varStoreInit = false;
                varList = new Dictionary<string, string>();
            }
            public string GetVarFromStore(string key)
            {
                if (!varStoreInit) initVarStore();
                readStore();
                if (varList.ContainsKey(key))
                { string v = varList[key]; return v.Trim(); }

                dbug("GetVarFromStore: Unable to retrieve key: " + key, 2);
                return null;

            }
            public void SetVarToStore(string key, string val)
            {
                if (!varStoreInit) initVarStore();
                readStore();
                if (varList.ContainsKey(key))
                    varList[key] = val;
                else
                    varList.Add(key, val);
                writeStore();
            }
            public void DelVarFromStore(string key)
            {
                if (!varStoreInit) initVarStore();
                readStore();
                if (varList.ContainsKey(key))
                    varList.Remove(key);
                writeStore();

            }

            public void initVarStore()
            {
                LCDVariableStore = GetBlock(LCDVariableStoreName) as IMyTextPanel;
                if (LCDVariableStore == null)
                    throw new Exception("SEManager: Unable to initialise var store, check LCD '" + LCDVariableStoreName + "' exists");

                if (!(LCDVariableStore is IMyTextPanel) && LCDVariableStore.IsFunctional)
                    throw new Exception("SEManager: Unable to initialise var store, check '" + LCDVariableStoreName + "' is a LCD/TextPanel and in functional");
                dbug("Var Store Initialised", 3);
                varStoreInit = true;

            }
            private void readStore()
            {
                varList.Clear();
                string strText = LCDVariableStore.GetPublicText();
                string[] lines = strText.Split(new char[] { '\n' });
                Array.ForEach(lines, line =>
                {
                    if (InStrI(line, ":"))
                    {
                        string[] vals = line.Split(new char[] { ':' });
                        varList.Add(vals[0], vals[1]);
                    }
                });

            }
            private void writeStore()
            {
                var vEnum = varList.GetEnumerator();
                string contents = "";
                //DisplayOnScreen(LCDVariableStoreName, "", false);

                while (vEnum.MoveNext())
                {
                    contents += vEnum.Current.Key + ":" + vEnum.Current.Value + "\n";

                }
                contents.Trim(new char[] { '\n' });
                DisplayOnScreen(LCDVariableStoreName, contents, false);
            }
        }
        #endregion
    }
}
