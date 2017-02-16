﻿using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;

namespace CommanderAddin
{
    public class CommanderAddin : MarkdownMonster.AddIns.MarkdownMonsterAddin
    {

        private CommanderWindow commanerWindow;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            Id = " CommanderAddin";

            // by passing in the add in you automatically
            // hook up OnExecute/OnExecuteConfiguration/OnCanExecute
            var menuItem = new AddInMenuItem(this)
            {
                Caption = "Commander Command Line Execution Launcher",

                // if an icon is specified it shows on the toolbar
                // if not the add-in only shows in the add-ins menu
                FontawesomeIcon = FontAwesomeIcon.Terminal
            };

            // if you don't want to display config or main menu item clear handler
            //menuItem.ExecuteConfiguration = null;

            // Must add the menu to the collection to display menu and toolbar items            
            this.MenuItems.Add(menuItem);
        }

        public override void OnExecute(object sender)
        {
            if(commanerWindow == null || !commanerWindow.IsLoaded)
            {
                commanerWindow = new CommanderWindow(this);

                commanerWindow.Top = Model.Window.Top;
                commanerWindow.Left = Model.Window.Left + Model.Window.Width -
                                      Model.Configuration.WindowPosition.SplitterPosition;
            }
            commanerWindow.Show();
            commanerWindow.Activate();
        }

        public override void OnExecuteConfiguration(object sender)
        {
            Model.Window.OpenTab(Path.Combine(mmApp.Configuration.CommonFolder, "CommanderAddin.json"));            
        }

        public override bool OnCanExecute(object sender)
        {
            return true;
        }


        public override void OnWindowLoaded()
        {
            foreach (var command in CommanderAddinConfiguration.Current.Commands)
            {

                if (!string.IsNullOrEmpty(command.KeyboardShortcut))
                {
                    var ksc = command.KeyboardShortcut.ToLower();
                    KeyBinding kb = new KeyBinding();

                    if (ksc.Contains("alt"))
                        kb.Modifiers = ModifierKeys.Alt;
                    if (ksc.Contains("shift"))
                        kb.Modifiers |= ModifierKeys.Shift;
                    if (ksc.Contains("ctrl") || ksc.Contains("ctl"))
                        kb.Modifiers |= ModifierKeys.Control;

                    string key =
                        ksc.Replace("+", "")
                            .Replace("-", "")
                            .Replace("_", "")
                            .Replace(" ", "")
                            .Replace("alt", "")
                            .Replace("shift", "")
                            .Replace("ctrl", "")
                            .Replace("ctl", "");

                    key = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key);
                    if (!string.IsNullOrEmpty(key))
                    {
                        KeyConverter k = new KeyConverter();
                        kb.Key = (Key)k.ConvertFromString(key);
                    }

                    // Whatever command you need to bind to
                    kb.Command = new CommandBase((s, e) => RunCommand(command),
                                                 (s, e) => Model.IsEditorActive);

                    Model.Window.InputBindings.Add(kb);
                }
            }
        }

   

        public void RunCommand(CommanderCommand command)
        {
            string code = command.CommandText;

            var parser = new ScriptParser();
            if (!parser.EvaluateScript(code, Model))
            { 
                MessageBox.Show(parser.ErrorMessage, "Snippet Execution failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);              
            }            
        }
    }
}
