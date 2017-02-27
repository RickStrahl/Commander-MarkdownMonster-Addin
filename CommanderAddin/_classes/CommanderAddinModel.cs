using System.Collections.Generic;
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

        /// <summary>
        /// Access to the main Markdown Monster applicaiton Model
        /// Some of the properties are duplicated on this model
        /// to make it easier to access in code.
        /// </summary>
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
        

        /// <summary>
        /// Access to the Addin implementation
        /// </summary>
        public CommanderAddin Addin { get; set;  }


        /// <summary>
        /// Access to Markdown Monster's Main Configuration
        /// </summary>
        public ApplicationConfiguration Configuration { get; set; }

        
        /// <summary>
        /// Access to the Addin's Configuration
        /// </summary>
        public CommanderAddinConfiguration AddinConfiguration { get; set; }

        /// <summary>
        /// Instance of the Addin's Window object
        /// </summary>
        public CommanderWindow AddinWindow { get; set; }

        /// <summary>
        /// Access to the main Markdown Monster Window
        /// </summary>
        public MainWindow Window { get; set; }
                

        
        /// <summary>
        /// Holds the currently active command instance in the
        /// editor/viewer.
        /// </summary>
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


        /// <summary>
        /// Instance of the currently active Markdown Document object
        /// </summary>
        public MarkdownDocument ActiveDocument => AppModel?.ActiveDocument;


        /// <summary>
        /// Instance of the Markdown Editor browser and ACE Editor instance
        /// </summary>
        public MarkdownDocumentEditor ActiveEditor => AppModel?.ActiveEditor;


        /// <summary>
        /// List of all the open document objects in the editor
        /// </summary>
        public List<MarkdownDocument> OpenDocuments => AppModel?.OpenDocuments;


        #region INotifyPropertyChanged        

        private ObservableCollection<CommanderCommand> _commands = new ObservableCollection<CommanderCommand>();


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

   
}
