using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Westwind.Scripting;
using Westwind.Utilities;

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

        /// <summary>
        ///  Compiler mode used to compile this script with
        /// </summary>
        public ScriptCompilerModes CompilerMode { get; set; } = ScriptCompilerModes.Roslyn;

        public string ErrorMessage { get; set; }

        public CSharpScriptExecution ScriptInstance { get; set; }

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
            ScriptInstance = CreateScriptObject();


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

                    var fullAssemblyName = FileUtils.GetPhysicalPath(assemblyName);
                    if (File.Exists(fullAssemblyName))
                        assemblyName = fullAssemblyName;

                    // Add to Engine since host is already instantiated
                    ScriptInstance.AddAssembly(assemblyName);
                    continue;
                }

                if (line.Trim().Contains("using ") && !line.Contains("("))
                {
                    string ns = line.Replace("using ", "").Replace(";", "").Trim();

                    if (!ScriptInstance.Namespaces.Contains("using " + ns + ";"))
                        ScriptInstance.AddNamespace(ns);
                    continue;
                }

                sb.AppendLine(line);
            }

            string oldPath = Environment.CurrentDirectory;

            code = sb.ToString();
            //code = "dynamic Model = parameters[0];\r\n" +
            //       code + "\r\n" +
            //       "return null;";

            code = "public void ExecuteScript(dynamic Model)\n" +
                   "{\n" +
                   code + "\n" +
                   "}";
            
            ScriptInstance.ExecuteMethod(code, "ExecuteScript",model);
            
            //.ExecuteCode(code, model);

            Directory.SetCurrentDirectory(oldPath);
            
            if (ScriptInstance.Error)
            {
                ErrorMessage = ScriptInstance.ErrorMessage;
                var callStack = ScriptInstance.LastException?.StackTrace;
                if (!string.IsNullOrEmpty(callStack))
                {
                    callStack = StringUtils.GetLines(callStack).FirstOrDefault();

                    if (!string.IsNullOrEmpty(callStack))
                        ErrorMessage += "\r\n" + "Error at: " + callStack;
                }
            }

            return !ScriptInstance.Error;
        }

        /// <summary>
        /// Creates an instance of wwScripting for this parser
        /// with the appropriate assemblies and namespaces set
        /// </summary>
        /// <returns></returns>
        private CSharpScriptExecution  CreateScriptObject()
        {
            using (var scripting = new CSharpScriptExecution {GeneratedNamespace = "MarkdownMonster.Commander.Scripting"})
            {
                scripting.AddAssemblies("System.dll",
                    "System.Core.dll",
                    "System.Drawing.dll",
                    "Microsoft.CSharp.dll",
                    "System.Windows.Forms.dll",
                    "System.Data.dll",
                    "MarkdownMonster.exe",

                    "Westwind.Utilities.dll",
                    "System.Configuration.dll",

                    "WPF\\PresentationCore.dll",
                    "WPF\\PresentationUI.dll",
                    "WPF\\PresentationFramework.dll",
                    "WPF\\WindowsBase.dll",
                    "System.Xaml.dll",
                    "Newtonsoft.Json.dll");

                scripting.AddNamespaces("System",
                    "System.IO",
                    "System.Reflection",
                    "System.Text",
                    "System.Drawing",
                    "System.Diagnostics",
                    "System.Data",
                    "System.Data.SqlClient",
                    "System.Linq",
                    "System.Windows",
                    "System.Collections.Generic",

                    "Newtonsoft.Json",
                    "Newtonsoft.Json.Linq",

                    "MarkdownMonster",
                    "MarkdownMonster.Windows",
                    "Westwind.Utilities");


                scripting.SaveGeneratedCode = true;
                scripting.CompilerMode = CompilerMode;

                return scripting;
            }
        }        
    }

}