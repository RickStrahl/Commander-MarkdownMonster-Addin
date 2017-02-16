using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster;
using MarkdownMonster.Annotations;
using Westwind.Utilities.Configuration;


namespace CommanderAddin
{
    public class CommanderAddinConfiguration : AppConfiguration, INotifyPropertyChanged
    {

        public static CommanderAddinConfiguration Current;

        static CommanderAddinConfiguration()
        {
            Current = new CommanderAddinConfiguration();
            Current.Initialize();
        }


        public ObservableCollection<CommanderCommand> Commands
        {
            get { return _commands; }
            set
            {
                if (Equals(value, _commands)) return;
                _commands = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<CommanderCommand> _commands = new ObservableCollection<CommanderCommand>();

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        #endregion


        #region AppConfiguration
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new JsonFileConfigurationProvider<CommanderAddinConfiguration>()
            {
                JsonConfigurationFile = Path.Combine(mmApp.Configuration.CommonFolder, "CommanderAddin.json")
            };

            if (!File.Exists(provider.JsonConfigurationFile))
            {
                if (!Directory.Exists(Path.GetDirectoryName(provider.JsonConfigurationFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(provider.JsonConfigurationFile));
            }

            return provider;
        }
        #endregion
    }
}
