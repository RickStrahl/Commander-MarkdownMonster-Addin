using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows;

namespace CommanderAddin
{
    public class CommanderAddin : MarkdownMonsterAddin
    {

        private CommanderWindow commanderWindow;

        public CommanderAddinModel AddinModel { get; set; }

        public override void OnApplicationStart()
        {
            AddinModel = new CommanderAddinModel {
                AppModel = Model,
                AddinConfiguration = CommanderAddinConfiguration.Current,                
                Addin = this
            };

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
            try
            {
                menuItem.IconImageSource = new ImageSourceConverter()
                        .ConvertFromString("pack://application:,,,/CommanderAddin;component/icon_22.png") as ImageSource;
            }
            catch { }

            // if you don't want to display config or main menu item clear handler
            //menuItem.ExecuteConfiguration = null;                
            // Must add the menu to the collection to display menu and toolbar items            
            MenuItems.Add(menuItem);
        }

        public override void OnExecute(object sender)
        {
            if(commanderWindow == null || !commanderWindow.IsLoaded)
            {
                AddinModel.Window = Model.Window;
                AddinModel.AppModel = Model;                
                
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
            OpenTab(Path.Combine(mmApp.Configuration.CommonFolder, "CommanderAddin.json"));            
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
            
            bool showConsole = commanderWindow != null && commanderWindow.Visibility == Visibility.Visible;
            if (showConsole)
            {
                var tbox = commanderWindow.TextConsole;
                tbox.Clear();
                Console.SetOut(new ConsoleTextWriter()
                {
                    tbox = tbox
                });
                
            }

            using (var process = Process.GetCurrentProcess())
            {                                
                var parser = new ScriptParser();            
                if (!parser.EvaluateScript(code, AddinModel))
                {

                    Console.WriteLine($"*** Error running Script code:\r\n{parser.ErrorMessage}");
                    
                    if (CommanderAddinConfiguration.Current.OpenSourceInEditorOnErrors)
                    {
                        string fname = Path.Combine(Path.GetTempPath(), "Commander_Compiled_Code.cs");
                        File.WriteAllText(fname, parser.ScriptInstance.SourceCode);
                        var tab = OpenTab(fname);
                        File.Delete(fname);

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

                                commanderWindow.Activate();

                            }, DispatcherPriority.ApplicationIdle);
                        }
                    }
                }
            }

            if (showConsole)
            {
                commanderWindow.TextConsole.ScrollToHome();
                StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);
            }
        }
    }

    public class ConsoleTextWriter : TextWriter
    {
        public TextBox tbox;

        public override void Write(char value)
        {
            tbox.Text += value;
            if (value == '\n')
                Dispatcher.CurrentDispatcher.Delay(1,(s)=>tbox.ScrollToEnd());
        }

        public override void Write(string value)
        {
            base.Write(value);
            WindowUtilities.DoEvents();
            tbox.ScrollToEnd();
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }        
    }
}
