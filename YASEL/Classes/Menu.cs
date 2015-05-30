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

namespace Menu
{
    using Grid;
    /// <summary>
    /// Menu class for building menus
    /// </summary>
    /// <example>
    /// <code>
    /// Menu myMenu;
    /// void Main(string argument) {
    ///     if (myMenu == null) {
    ///         myMenu = new Menu("Menu TextPanel");
    ///         var myMenuPage = new MenuPage("Main", "Main Menu");
    ///         myMenuPage.AddOption(new MenuOption(1, "Option 1", "", OnAction1));
    ///         myMenuPage.AddOption(new MenuOption(2, "Menu 2", "Menu 2"));
    ///         myMenu.AddPage(myMenuPage);
    ///         myMenuPage = new MenuPage("Menu 2", "Menu Page 2");
    ///         myMenuPage.AddOption(new MenuOption(1, "Option 2", "", OnAction2));
    ///         myMenuPage.AddOption(new MenuOption(2, "Main Menu", "Main"));
    ///         myMenu.AddPage(myMenuPage);
    ///     }
    ///     int actionButton;
    ///     if (int.TryParse(argument, out actionButton);
    ///         myMenu.OnButtonPress(actionButton);
    /// }
    /// 
    /// public void OnAction1()
    /// {
    ///     Echo("Option 1 Pressed");
    /// }
    /// public void OnAction2()
    /// {
    ///     Echo("Option 2 Pressed");
    /// }
    /// </code>
    /// </example>
    public class Menu
    {
        Dictionary<string, MenuPage> pages;
        MenuPage curPage;
        IMyTextPanel display;

        public Menu(string LCDName)
        {
            pages = new Dictionary<string, MenuPage>();
            display = (IMyTextPanel)Grid.GetBlock(LCDName);
            if (!(display is IMyTextPanel)) throw new Exception("Unable to init Menu, unable to access LCD:" + LCDName);
        }
        public void AddPage(MenuPage page)
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
            MenuOption option = curPage.GetOption(button);
            if (option != null)
            {
                if (option.GotoPage != "")
                    ShowPage(option.GotoPage);
                if (option.OnSelect != null)
                    option.OnSelect();
            }
        }

    }

    public class MenuPage
    {
        public string Name, Title;
        private Dictionary<int, MenuOption> options;

        public MenuPage(string name, string title = "")
        {
            options = new Dictionary<int, MenuOption>();
            this.Name = name;
            this.Title = title;
        }

        public void AddOption(MenuOption option)
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
        public MenuOption GetOption(int button)
        {
            if (options.ContainsKey(button))
                return options[button];
            else return null;
        }
    }

    public class MenuOption
    {
        public string GotoPage;
        public Action OnSelect;
        public int ActionButton;
        public string Text;

        public MenuOption(int actionButton, string text, string gotoPage = "", Action onSelect = null)
        {
            if (actionButton < 1 || actionButton > 9)
                throw new Exception("Unable to create menu option, action button must be between 1 and 9");
            ActionButton = actionButton;
            Text = text;
            GotoPage = gotoPage;
            OnSelect = onSelect;
        }

    }

}
