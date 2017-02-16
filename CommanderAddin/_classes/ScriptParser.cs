//if false

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Westwind.Utilities;
using Westwind.wwScripting;

namespace CommanderAddin
{

    /// <summary>
    /// A minimal Script Parser class that uses {{ C# Expressions }} to evaluate
    /// string based templates.
    /// </summary>
    /// <example>
    /// string script = @"Hi {{Name}}! Time is {{DateTime.sNow.ToString(""MMM dd, yyyy HH:mm:ss"")}}...";    
    /// var parser = new ScriptParser();
    /// string result = await parser.EvaluateScriptAsync(script, new Globals { Name = "Rick" });
    /// </example>
    public class ScriptParser
    {        
        // Additional namespaces to add to the script
        public List<string> Namespaces = new List<string>();

        /// <summary>
        ///  Additional references to add beyond MsCoreLib and System 
        /// Pass in a type from a give assembly
        /// </summary>        
        public List<string> References = new List<string>();

        
        public string ErrorMessage { get; set; }

        public wwScripting ScriptInstance { get; set; }


        public static Dictionary<string, Assembly> CodeBlocks = new Dictionary<string, Assembly>();

        /// <summary>
        /// Evaluates the embedded script parsing out {{ C# Expression }} 
        /// blocks and evaluating the expressions and embedding the string
        /// output into the result string.
        /// 
        /// 
        /// </summary>
        /// <param name="code">The code to execute
        /// <param name="model">Optional model data accessible in Expressions as `Model`</param>
        /// <returns></returns>
        public bool EvaluateScript(string code, object model = null)
        {

            ScriptInstance = CreatewwScripting();


            if (CodeBlocks.ContainsKey(code))
            {
                Debug.WriteLine("wwScripting Cached Code: \r\n" + code);
                ScriptInstance.ExecuteCodeFromAssembly(code, CodeBlocks[code], model);
            }
            else
            {

                var snippetLines = StringUtils.GetLines(code);
                var sb = new StringBuilder();
                foreach (var line in snippetLines)
                {
                    if (line.Trim().Contains("#r "))
                    {
                        string assemblyName = line.Replace("#r ", "").Trim();

                        if (assemblyName.Contains("\\") || assemblyName.Contains("/"))
                        {
                            ErrorMessage = "Assemblies loaded from external folders are not allowed: " + assemblyName +
                                           "\r\n\r\n" +
                                           "Referenced assemblies can only be loaded out of the Markdown Monster startup folder.";
                            return false;
                        }

                        var fullAssemblyName = FileUtils.GetFullPath(assemblyName);
                        if (File.Exists(fullAssemblyName))
                            assemblyName = fullAssemblyName;

                        // Add to Engine since host is already instantiated
                        ScriptInstance.AddAssembly(assemblyName);
                        continue;
                    }
                    if (line.Trim().Contains("using "))
                    {
                        string ns = line.Replace("using ", "").Replace(";","").Trim();

                        if (!ScriptInstance.Namespaces.Contains("using " + ns + ";"))                            
                            ScriptInstance.AddNamespace(ns);
                        continue;
                    }

                    sb.AppendLine(line);
                }

                string oldPath = Environment.CurrentDirectory;


                code = sb.ToString();
                code = "dynamic Model = Parameters[0];\r\n" +
                       code + "\r\n" + 
                       "return null;";

                ScriptInstance.ExecuteCode(code, model);
                
                // cache the generated assembly for reuse on subsequent runs
                if (ScriptInstance.Assembly != null)
                    CodeBlocks[code] = ScriptInstance.Assembly;

                Directory.SetCurrentDirectory(oldPath);
            }

            if (ScriptInstance.Error)
                ErrorMessage = ScriptInstance.ErrorMessage;

            return !ScriptInstance.Error;
        }

        /// <summary>
        /// Creates an instance of wwScripting for this parser
        /// with the appropriate assemblies and namespaces set
        /// </summary>
        /// <returns></returns>
        private static wwScripting CreatewwScripting()
        {
            var scripting = new wwScripting()
            {
                DefaultAssemblies = false,
                AssemblyNamespace = "MarkdownMonster.Commander.Scripting"
            };
            scripting.AddAssembly("System.dll");
            scripting.AddAssembly("System.Core.dll");
            scripting.AddAssembly("Microsoft.CSharp.dll");
            scripting.AddAssembly("System.Windows.Forms.dll");
            scripting.AddAssembly("System.Data.dll");
            scripting.AddAssembly("MarkdownMonster.exe");
            scripting.AddAssembly("Westwind.Utilities.dll");
            scripting.AddAssembly("System.Configuration.dll");

            scripting.AddAssembly("WPF\\PresentationCore.dll");
            scripting.AddAssembly("WPF\\PresentationUI.dll");
            scripting.AddAssembly("WPF\\PresentationFramework.dll");
            scripting.AddAssembly("WPF\\WindowsBase.dll");
            scripting.AddAssembly("System.Xaml.dll");

            scripting.AddNamespace("System");
            scripting.AddNamespace("System.IO");
            scripting.AddNamespace("System.Reflection");            
            scripting.AddNamespace("System.Text");
            scripting.AddNamespace("System.Drawing");
            scripting.AddNamespace("System.Diagnostics");                        
            scripting.AddNamespace("System.Data");
            scripting.AddNamespace("System.Data.SqlClient");
            scripting.AddNamespace("System.Linq");
            
            scripting.AddNamespace("MarkdownMonster");
            scripting.AddNamespace("Westwind.Utilities");

            scripting.SaveSourceCode = true;

            return scripting;
        }        
    }
}
//#endif