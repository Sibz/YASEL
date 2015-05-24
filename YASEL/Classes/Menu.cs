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
        #region MenuClass
        //# Requires BlockFunctions
        public class SE_Menu
        {
            Dictionary<string, SE_MenuPage> pages;
            SE_MenuPage curPage;
            IMyTextPanel display;

            public SE_Menu(string sensorName, string LCDName)
            {
                pages = new Dictionary<string, SE_MenuPage>();
                display = (IMyTextPanel)GetBlock(LCDName);
                if (!(display is IMyTextPanel)) throw new Exception("Unable to init Menu, unable to access LCD:" + LCDName);
            }
            public void AddPage(SE_MenuPage page)
            {
                if (!pages.ContainsKey(page.Name))
                    pages.Add(page.Name, page);
                else throw new Exception("Unable to add page to menu, page name exists");
            }
            public void ShowPage(string name)
            {
                if (pages.ContainsKey(name))
                { display.WritePublicText(pages[name].GetText()); curPage = pages[name]; }
            }

            public void OnButtonPress(int button)
            {
                SE_MenuOption option = curPage.GetOption(button);
                if (option != null)
                {
                    if (option.GotoPage != "")
                        ShowPage(option.GotoPage);
                    if (option.OnSelect != null)
                        option.OnSelect();
                }
            }

        }

        public class SE_MenuPage
        {
            public string Name, Title;
            private Dictionary<int, SE_MenuOption> options;

            public SE_MenuPage(string name, string title = "")
            {
                options = new Dictionary<int, SE_MenuOption>();
                this.Name = name;
                this.Title = title;
            }

            public void AddOption(SE_MenuOption option)
            {
                if (!options.ContainsKey(option.ActionButton))
                    options.Add(option.ActionButton, option);
                else throw new Exception("Unable to add option to menu page, option actionbutton exists");
            }
            public string GetText()
            {
                string strText = Title;
                for (int i = 1; i < 10; i++)
                    if (options.ContainsKey(i)) strText += "\n" + i + ": " + options[i].Text;

                return strText;
            }
            public SE_MenuOption GetOption(int button)
            {
                if (options.ContainsKey(button))
                    return options[button];
                else return null;
            }
        }

        public class SE_MenuOption
        {
            public string GotoPage;
            public Action OnSelect;
            public int ActionButton;
            public string Text;

            public SE_MenuOption(int actionButton, string text, string gotoPage = "", Action onSelect = null)
            {
                if (actionButton < 1 || actionButton > 9)
                    throw new Exception("Unable to create menu option, action button must be between 1 and 9");
                ActionButton = actionButton;
                Text = text;
                GotoPage = gotoPage;
                OnSelect = onSelect;
            }

        }
        #endregion
    }
}
