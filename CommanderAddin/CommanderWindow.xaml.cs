using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarkdownMonster;

namespace CommanderAddin
{
    /// <summary>
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class CommanderWindow
    {
        public CommanderAddinModel Model { get; set; }
        
        public CommanderWindow(CommanderAddin addin)
        {
            InitializeComponent();
            mmApp.SetThemeWindowOverride(this);


            Model = new CommanderAddinModel()
            {
                Configuration = CommanderAddinConfiguration.Current,
                Window = addin.Model.Window,
                AppModel = addin.Model.Window.Model,                
                Addin = addin                               
            };


            if (Model.Configuration.Commands == null || Model.Configuration.Commands.Count < 1)
            {
                Model.Configuration.Commands = new System.Collections.ObjectModel.ObservableCollection<CommanderCommand>();
                Model.Configuration.Commands.Add(new CommanderCommand
                {
                    Name = "Copyright Notice",
                });
            }
            else
            {
                Model.Configuration.Commands =
                    new ObservableCollection<CommanderCommand>(Model.Configuration.Commands.OrderBy(snip => snip.Name));
                if (Model.Configuration.Commands.Count > 0)
                    Model.ActiveCommand = Model.Configuration.Commands[0];
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
            if (Model.Configuration.Commands.Count > 0)
            {
                ListCommands.SelectedItem = Model.Configuration.Commands[0];
                initialValue = Model.Configuration.Commands[0].CommandText;
            }

            editor = new MarkdownEditorSimple(WebBrowserCommand, initialValue);            
            editor.IsDirtyAction =  () =>
            { 
                string val = editor.GetMarkdown();
                if (val != null && Model.ActiveCommand != null)
                    Model.ActiveCommand.CommandText = val;

                return true;
            };

            Dispatcher.InvokeAsync(() =>
            {
                ListCommands.Focus();
                editor.SetEditorSyntax("csharp");
            },System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }


        private void CommanderWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            CommanderAddinConfiguration.Current.Write();
        }


        private void ListCommands_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var command = ListCommands.SelectedItem as CommanderCommand;
            if (command == null)
                return;

            Model.Addin.RunCommand(command);            
        }

        private void ToolButtonNewCommand_Click(object sender, RoutedEventArgs e)
        {
            Model.Configuration.Commands.Insert(0,new CommanderCommand() {Name = "New Command"});
            ListCommands.SelectedItem = Model.Configuration.Commands[0];
        }


        private void ToolButtonRemoveCommand_Click(object sender, RoutedEventArgs e)
        {
            var command = ListCommands.SelectedItem as CommanderCommand;
            if (command == null)
                return;
            CommanderAddinConfiguration.Current.Commands.Remove(command);
        }


        private void ToolButtonRunCommand_Click(object sender, RoutedEventArgs e)
        {
            var command = ListCommands.SelectedItem as CommanderCommand;
            if (command == null)
                return;

            Model.Addin.RunCommand(command);
        }

        
        private void ListCommands_KeyUp(object sender, KeyEventArgs e)
        {
            
            if (e.Key == Key.Return || e.Key == Key.Space)
            {
                
                var command = ListCommands.SelectedItem as CommanderCommand;
                if (command != null)                    
                    Model.Addin.RunCommand(command);
            }
        }

        private void ListCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var command = ListCommands.SelectedItem as CommanderCommand;


            if (command != null)
            {
                try { 
                    editor?.SetMarkdown(command.CommandText);
                }catch { }}
        }
        

    }
}
