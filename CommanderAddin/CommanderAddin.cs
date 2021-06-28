using System;
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
                        var cmd = new CommandBase((s, e) => RunCommand(command),
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

            var parser = new ScriptParser()
            {
                CompilerMode = command.CompilerMode
            };


            bool result =parser.EvaluateScript(code, AddinModel);

            //bool result = await Task.Run<bool>(() =>
            //{               
            //    return 
            //});

            if (!result)
                {
                    if (!showConsole)
                    {
                        AddinModel.Window.ShowStatus("*** Addin execution failed: " + parser.ErrorMessage, 6000);
                        AddinModel.Window.SetStatusIcon(FontAwesomeIcon.Warning, Colors.Red);
                    }
                    else
                        Console.WriteLine($@"*** Error running Script code:
{parser.ErrorMessage}

{parser.ScriptInstance.GeneratedClassCodeWithLineNumbers}
");

                

                if (CommanderAddinConfiguration.Current.OpenSourceInEditorOnErrors)
			    {
				    string fname = Path.Combine(Path.GetTempPath(), "Commander_Compiled_Code.cs");
				    File.WriteAllText(fname, parser.ScriptInstance.GeneratedClassCode);

				    var tab = await OpenTab(fname);
				    File.Delete(fname);

				    if (tab != null)
				    {
					    var editor = tab.Tag as MarkdownDocumentEditor;
					    editor.SetEditorSyntax("csharp");
					    editor.SetMarkdown(parser.ScriptInstance.GeneratedClassCode);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        _ = Dispatcher.CurrentDispatcher.InvokeAsync(() =>
					    {
						    if (editor.JsEditor == null)
							    Task.Delay(400);
						    _ = editor.JsEditor.SetShowLineNumbers(true);

						    if (commanderWindow == null)
						    {
							    commanderWindow = new CommanderWindow(this);
							    commanderWindow.Show();
						    }
						    else
							    commanderWindow.Activate();

					    }, DispatcherPriority.ApplicationIdle);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
			    }
		    }
		    //else
		    //{
			   // AddinModel.Window.ShowStatus("Command execution for " + command.Name + " completed successfully",6000);
		    //}


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
