using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarkdownMonster;
using MarkdownMonster.Annotations;


namespace CommanderAddin
{
    public class CommanderAddinModel : INotifyPropertyChanged
    {

        public CommanderAddinModel()
        {
        }

        public CommanderAddin Addin { get; set;  }               
        public MainWindow Window { get; set; }
                
        public AppModel AppModel
        {
            get { return _appModel; }
            set
            {
                if (Equals(value, _appModel)) return;
                _appModel = value;
                OnPropertyChanged();
            }
        }
        private AppModel _appModel;

        
        public CommanderCommand ActiveCommand
        {
            get { return _activeCommand; }
            set
            {
                if (Equals(value, _activeCommand)) return;
                _activeCommand = value;
                OnPropertyChanged();
            }
        }
        private CommanderCommand _activeCommand;
        

        
        public CommanderAddinConfiguration Configuration { get; set; }

        private ObservableCollection<CommanderCommand> _commands = new ObservableCollection<CommanderCommand>();


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

   
}
