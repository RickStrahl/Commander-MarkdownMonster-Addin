using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarkdownMonster.Annotations;
using Westwind.Scripting;

namespace CommanderAddin
{
    public class CommanderCommand : INotifyPropertyChanged
    {
        public CommanderCommand()
        {

        }
        
        /// <summary>
        /// A display name for the snippet.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }
        private string _name;


        /// <summary>
        /// The actual snippet text to embed and evaluate
        /// </summary>
        public string CommandText
        {
            get { return _commandText; }
            set
            {
                if (value == _commandText) return;
                _commandText = value;
                OnPropertyChanged();                
            }
        }
        private string _commandText;
        
        public string KeyboardShortcut
        {
            get { return _keyboardShortcut; }
            set
            {
                if (value == _keyboardShortcut) return;
                _keyboardShortcut = value;
                OnPropertyChanged();
            }
        }
        private string _keyboardShortcut;
        

        public ScriptCompilerModes CompilerMode
        {
            get => _scriptMode;
            set
            {
                if (value == _scriptMode) return;
                _scriptMode = value;
                OnPropertyChanged();
            }
        }
        private ScriptCompilerModes _scriptMode = ScriptCompilerModes.Roslyn;


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}