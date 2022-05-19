using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FontAwesome.WPF;
using MarkdownMonster;
using Westwind.Utilities;

namespace CommanderAddin
{
    /// <summary>
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class CommanderWindow
    {
        public CommanderAddinModel Model { get; set; }

        private bool pageLoaded = false;
        
        public CommanderWindow(CommanderAddin addin)
        {
            InitializeComponent();
            mmApp.SetThemeWindowOverride(this);

            Model = addin.AddinModel;
            Model.AddinWindow = this;


            if (Model.AddinConfiguration.Commands == null || Model.AddinConfiguration.Commands.Count < 1)
            {
                Model.AddinConfiguration.Commands = new ObservableCollection<CommanderCommand>();
                Model.AddinConfiguration.Commands.Add(
                    new CommanderCommand
                    {
                        Name = "Console Test",
                        CommandText = "for(int x = 1;  x < 5; x++) {\n    Console.WriteLine(\"Hello World \" + x.ToString());\n}\n\n// Demonstrate async functionality\nusing (var client = new WebClient())\n{\n    var uri = new Uri(\"https://albumviewer.west-wind.com/api/album/37\");\n    var json = await client.DownloadStringTaskAsync(uri);\n    Console.WriteLine($\"\\n{json}\");\n}",
                    });

                Model.AddinConfiguration.Commands.Add(
                    new CommanderCommand
                    {
                        Name = "Open in VsCode",
                        CommandText =
                            "var docFile = Model.ActiveDocument.Filename;\nif (string.IsNullOrEmpty(docFile))\n\treturn null;\n\nModel.Window.SaveFile();\n\nvar exe = @\"C:\\Program Files\\Microsoft VS Code\\Code.exe\";\nexe = Environment.ExpandEnvironmentVariables(exe);\n\nProcess pi = null;\ntry {\n\tvar lineNo = await Model.ActiveEditor.GetLineNumber();\n\tpi = Process.Start(exe,\"-g \\\"\" + docFile + $\":{lineNo + 1}\\\"\");\n}\ncatch(Exception ex) {\n\tModel.Window.ShowStatusError(\"Couldn't open editor: \" + ex.Message);\n\treturn null;\n}\n\nif (pi != null)\n    Model.Window.ShowStatus($\"VS Code  started with: {docFile}\",5000);\nelse\n    Model.Window.ShowStatusError(\"Failed to start VS Code.\");\n",
                        KeyboardShortcut = "Alt-Shift-V"
                    });
            }
            else
            {
                Model.AddinConfiguration.Commands =
                    new ObservableCollection<CommanderCommand>(Model.AddinConfiguration.Commands.OrderBy(snip => snip.Name));
                if (Model.AddinConfiguration.Commands.Count > 0)
                    Model.ActiveCommand = Model.AddinConfiguration.Commands[0];
            }

            Loaded += CommandsWindow_Loaded;
            Unloaded += CommanderWindow_Unloaded;

            WebBrowserCommand.Visibility = Visibility.Hidden;

            DataContext = Model;            

        }      

        private MarkdownEditorSimple editor;

        private void CommandsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string initialValue = null;

            if (Model.AddinConfiguration.Commands.Count > 0)
            {
                CommanderCommand selectedItem = null;
                if (!string.IsNullOrEmpty(CommanderAddinConfiguration.Current.LastCommand))
                    selectedItem =
                        Model.AddinConfiguration.Commands.FirstOrDefault(
                            cmd => cmd.Name == Model.AddinConfiguration.LastCommand);
                else
                    selectedItem = Model.AddinConfiguration.Commands[0];

                ListCommands.SelectedItem = selectedItem;
                if (selectedItem != null)
                    initialValue = selectedItem.CommandText;
            }

            editor = new MarkdownEditorSimple(WebBrowserCommand, initialValue, "csharp");         
            editor.IsDirtyAction = (isDirty, newText, oldText) =>
            {
                if (newText != null && Model.ActiveCommand != null)
                    Model.ActiveCommand.CommandText = newText;

                return newText != oldText;
            };

            Dispatcher.InvokeAsync(() =>
            {
                ListCommands.Focus();
                _ = editor.BrowserInterop.SetLanguage("csharp");                
            },System.Windows.Threading.DispatcherPriority.ApplicationIdle);

            pageLoaded = true;
        }


        private void CommanderWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            CommanderAddinConfiguration.Current.Write();
        }


        private async void ListCommands_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var command = ListCommands.SelectedItem as CommanderCommand;
            if (command == null)
                return;

            await Model.Addin.RunCommand(command);            
        }

        private void ToolButtonNewCommand_Click(object sender, RoutedEventArgs e)
        {
            Model.AddinConfiguration.Commands.Insert(0,new CommanderCommand() {Name = "New Command"});
            ListCommands.SelectedItem = Model.AddinConfiguration.Commands[0];
        }


        private void ToolButtonRemoveCommand_Click(object sender, RoutedEventArgs e)
        {
            var command = ListCommands.SelectedItem as CommanderCommand;
            if (command == null)
                return;
            CommanderAddinConfiguration.Current.Commands.Remove(command);
        }


        private async void ToolButtonRunCommand_Click(object sender, RoutedEventArgs e)
        {
            var command = ListCommands.SelectedItem as CommanderCommand;
            if (command == null)
                return;

             await Model.Addin.RunCommand(command);
        }

        
        private async void ListCommands_KeyUp(object sender, KeyEventArgs e)
        {
            
            if (e.Key == Key.Return || e.Key == Key.Space)
            {
                
                var command = ListCommands.SelectedItem as CommanderCommand;
                if (command != null)                    
                    await Model.Addin.RunCommand(command);
            }
        }

        private async void ListCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!pageLoaded)
                return;

            var command = ListCommands.SelectedItem as CommanderCommand;
            if (command != null)
            {
                try
                {
                    await editor.BrowserInterop.SetValue(command.CommandText);
                }
                catch
                { }

                CommanderAddinConfiguration.Current.LastCommand = command.Name;
            }

            
        }


        #region StatusBar

        public void ShowStatus(string message = null, int milliSeconds = 0)
        {
            if (message == null)
                message = "Ready";

            StatusText.Text = message;

            if (milliSeconds > 0)
            {
                var t = new Timer(new TimerCallback((object win) =>
                {
                    var window = win as CommanderWindow;
                    if (window == null)
                        return;

                    window.Dispatcher.Invoke(() => { window.ShowStatus(null, 0); });
                }), this, milliSeconds, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Status the statusbar icon on the left bottom to some indicator
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        /// <param name="spin"></param>
        public void SetStatusIcon(FontAwesomeIcon icon, Color color, bool spin = false)
        {
            StatusIcon.Icon = icon;
            StatusIcon.Foreground = new SolidColorBrush(color);
            if (spin)
                StatusIcon.SpinDuration = 30;
            StatusIcon.Spin = spin;
        }

        /// <summary>
        /// Resets the Status bar icon on the left to its default green circle
        /// </summary>
        public void SetStatusIcon()
        {
            StatusIcon.Icon = FontAwesomeIcon.Circle;
            StatusIcon.Foreground = new SolidColorBrush(Colors.Green);
            StatusIcon.Spin = false;
            StatusIcon.SpinDuration = 0;
            StatusIcon.StopSpin();
        }
        #endregion

        private void btnClearConsole_Click(object sender, RoutedEventArgs e)
        {
            TextConsole.Text = string.Empty;
            ConsoleGridRow.Height = new GridLength(0);
        }
    }
}
