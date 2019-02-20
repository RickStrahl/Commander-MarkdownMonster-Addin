using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
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
        public ApplicationConfiguration Configuration => AppModel.Configuration;

        
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

        

		/// <summary>
		/// Executes a process with given command line parameters
		/// </summary>
		/// <param name="executable">Executable to run. Full path or exe only if on system path.</param>
		/// <param name="arguments">Command line arguments</param>
		/// <param name="timeoutMs">Timeout of the process in milliseconds. Pass -1 to wait forever. Pass 0 to not wait.</param>
		/// <returns>Process exit code. 0 on success, anything else error. 1460 timed out</returns>
		public int ExecuteProcess(string executable, string arguments, int timeoutMs = 60000, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden)
        {
	        Process process;

	        try
	        {
		        using (process = new Process())
		        {
			        process.StartInfo.FileName = executable;
			        process.StartInfo.Arguments = arguments;
			        process.StartInfo.WindowStyle = windowStyle;
                    if (windowStyle == ProcessWindowStyle.Hidden)
                        process.StartInfo.CreateNoWindow = true;

                    process.StartInfo.UseShellExecute = false;

			        process.StartInfo.RedirectStandardOutput = true;
			        process.StartInfo.RedirectStandardError = true;

			        process.OutputDataReceived += (sender, args) =>
			        {
				        Console.WriteLine(args.Data);
			        };
			        process.ErrorDataReceived += (sender, args) =>
			        {
				        Console.WriteLine(args.Data);
			        };

			        process.Start();

					if (timeoutMs < 0)
				        timeoutMs = 99999999; // indefinitely

			        if (timeoutMs > 0)
					{
				        if (!process.WaitForExit(timeoutMs))
				        {
					        Console.WriteLine("Process timed out.");
					        return 1460;
				        }
			        }
			        else
				        return 0;

			        return process.ExitCode;
				}		        
			}
	        catch(Exception ex)
	        {
		        Console.WriteLine("Error executing process: " + ex.Message);
		        return -1; // unhandled error
	        }
        }



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
