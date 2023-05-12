using System;
using System.IO;
using System.Linq;
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
using Westwind.Utilities;


namespace CommanderAddin
{
    public class CommanderAddin : MarkdownMonsterAddin
    {

        private CommanderWindow commanderWindow;

        public CommanderAddinModel AddinModel { get; set; }

        public override async Task OnApplicationStart()
        {
            AddinModel = new CommanderAddinModel {
                AppModel = Model,
                AddinConfiguration = CommanderAddinConfiguration.Current,                
                Addin = this
            };

            await base.OnApplicationStart();

            Id = "Commander";

            // by passing in the add in you automatically
            // hook up OnExecute/OnExecuteConfiguration/OnCanExecute
            var menuItem = new AddInMenuItem(this)
            {
                Caption = "Commander C# Script Automation",
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

        #region Addin Implementation Methods
        public override async Task OnExecute(object sender)
        {
            await base.OnExecute(sender);

            if(commanderWindow == null || !commanderWindow.IsLoaded)
            {
                InitializeAddinModel();

	            commanderWindow = new CommanderWindow(this);

                commanderWindow.Top = Model.Window.Top;
                commanderWindow.Left = Model.Window.Left + Model.Window.Width -
                                      Model.Configuration.WindowPosition.SplitterPosition;                
            }
            commanderWindow.Show();
            commanderWindow.Activate(); 
        }

	    private void InitializeAddinModel()
	    {
		    AddinModel.Window = Model.Window;
		    AddinModel.AppModel = Model;
	    }

	    public override async Task OnExecuteConfiguration(object sender)
        {
            await OpenTab(Path.Combine(mmApp.Configuration.CommonFolder, "CommanderAddin.json"));            
        }

        public override bool OnCanExecute(object sender)
        {
            return true;
        }


	    public override Task OnWindowLoaded()
	    {
		    foreach (var command in CommanderAddinConfiguration.Current.Commands)
		    {

			    if (!string.IsNullOrEmpty(command.KeyboardShortcut))
			    {
			        var ksc = command.KeyboardShortcut.ToLower().Replace("-", "+");
				    KeyBinding kb = new KeyBinding();

                    try
			        {
                        var gesture = new KeyGestureConverter().ConvertFromString(ksc) as InputGesture;
                        var cmd = new CommandBase((s, e) => RunCommand(command).FireAndForget(),
                            (s, e) => Model.IsEditorActive);
                        
			            kb.Gesture = gesture;
                        
			            // Whatever command you need to bind to
			            kb.Command = cmd;
                        
			            Model.Window.InputBindings.Add(kb);
                        
			        }
			        catch (Exception ex)
			        {
			            mmApp.Log("Commander Addin: Failed to create keyboard binding for: " + ksc + ". " + ex.Message);
			        }
			    }
		    }

            return Task.CompletedTask;
        }

        public override Task OnApplicationShutdown()
	    {
		    commanderWindow?.Close();
            return Task.CompletedTask;
        }
        #endregion

		
		public async Task RunCommand(CommanderCommand command)
        {
            
		    if (AddinModel.AppModel == null)
			    InitializeAddinModel();

		    string code = command.CommandText;
            ConsoleTextWriter consoleWriter = null;

		    bool showConsole = commanderWindow != null && commanderWindow.Visibility == Visibility.Visible;
		    if (showConsole)
		    {
			    var tbox = commanderWindow.TextConsole;
			    tbox.Clear();

			    WindowUtilities.DoEvents();

                consoleWriter = new ConsoleTextWriter()
                {
                    tbox = tbox
                };
                Console.SetOut(consoleWriter);             
		    }

            AddinModel.AppModel.Window.ShowStatusProgress("Executing Command '" + command.Name + "' started...");

            var parser = new ScriptParser();
            bool result = await parser.EvaluateScriptAsync(code, AddinModel);

            if (!result)
            {
                var msg = parser.ErrorMessage;
                if (parser.ScriptInstance.ErrorType == Westwind.Scripting.ExecutionErrorTypes.Compilation)
                    msg = "Script compilation error.";

                AddinModel.AppModel.Window.ShowStatusError("Command '" + command.Name + "' execution failed: " + msg);
                if (showConsole)
                {
                    if (parser.ScriptInstance.ErrorType == Westwind.Scripting.ExecutionErrorTypes.Compilation)
                    {
                        var adjusted = ScriptParser.FixupLineNumbersAndErrors(parser.ScriptInstance);
                        parser.ErrorMessage = adjusted.UpdatedErrorMessage;
                        Console.WriteLine($"*** Script Compilation Errors:\n{parser.ErrorMessage}\n");

                        Console.WriteLine("\n*** Generated Code:");
                        Console.WriteLine(parser.ScriptInstance.GeneratedClassCode);
                    }
                    else
                    {
                        Console.WriteLine($"*** Runtime Execution Error:\n{parser.ErrorMessage}\n");
                    }
                }
            }
		    else
            {
                if (mmApp.Model.Window.StatusText.Text.StartsWith("Executing Command ")) 
                    AddinModel.AppModel.Window.ShowStatusSuccess("Command '" + command.Name + "' executed successfully");
            }


		    if (showConsole)
            {
                consoleWriter.Close();
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
        private StringBuilder sb = new StringBuilder();

        public override void Write(char value)
        {
            sb.Append(value);

            if (value == '\n')
            {
                tbox.Dispatcher.Invoke(() => tbox.Text = sb.ToString());
            }
        }

     

        public override void Close()
        {
            tbox.Text = sb.ToString();
            base.Close();
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }        
    }
}
