using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;

namespace CommanderAddin
{
    public class CommanderAddin : MarkdownMonster.AddIns.MarkdownMonsterAddin
    {

        private CommanderWindow commanderWindow;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            Id = "Commander";

            // by passing in the add in you automatically
            // hook up OnExecute/OnExecuteConfiguration/OnCanExecute
            var menuItem = new AddInMenuItem(this)
            {
                Caption = "Commander Command Line Execution Launcher",
                FontawesomeIcon = FontAwesomeIcon.Terminal,
                KeyboardShortcut = CommanderAddinConfiguration.Current.KeyboardShortcut
            };

            // if you don't want to display config or main menu item clear handler
            //menuItem.ExecuteConfiguration = null;

            // Must add the menu to the collection to display menu and toolbar items            
            this.MenuItems.Add(menuItem);
        }

        public override void OnExecute(object sender)
        {
            if(commanderWindow == null || !commanderWindow.IsLoaded)
            {
                commanderWindow = new CommanderWindow(this);

                commanderWindow.Top = Model.Window.Top;
                commanderWindow.Left = Model.Window.Left + Model.Window.Width -
                                      Model.Configuration.WindowPosition.SplitterPosition;
            }
            commanderWindow.Show();
            commanderWindow.Activate();
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

        public override void OnApplicationShutdown()
        {
            commanderWindow?.Close();
        }


        public void RunCommand(CommanderCommand command)
        {
            string code = command.CommandText;

            var parser = new ScriptParser();            
            if (!parser.EvaluateScript(code, Model))
            {

                if (MessageBox.Show(parser.ErrorMessage +
                                    "\r\n\r\n" +
                                    "Do you want to display the generated source?",
                        "Snippet Execution failed",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    string fname = Path.Combine(Path.GetTempPath(),"_MM_Commander_Compiled.cs");
                    File.WriteAllText(fname, parser.ScriptInstance.SourceCode);
                    var tab = Model.Window.OpenTab(fname);
                    if (tab != null)
                    {
                        var editor = tab.Tag as MarkdownDocumentEditor;
                        editor.SetEditorSyntax("csharp");
                        editor.SetMarkdown(parser.ScriptInstance.SourceCode);

                        Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                        {
                            if (editor.AceEditor == null)
                                Task.Delay(400);
                            editor.AceEditor.setshowlinenumbers(true);                            
                        },DispatcherPriority.ApplicationIdle);
                        

                    }
                }




            }            
        }
    }
}
