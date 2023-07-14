#if false
using System;
using System.IO;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
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
        /// If a compilation error occurs this holds the compiler error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Returns source code for the compiled script
        /// </summary>
        public string CompiledCode
        {
            get
            {
                if (ScriptInstance == null) return null;
                return ScriptInstance.GeneratedClassCodeWithLineNumbers;
            }
        }

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
        public async Task<bool> EvaluateScriptAsync(string code, CommanderAddinModel model = null)
        {
            ScriptInstance = CreateScriptObject();

            string oldPath = Environment.CurrentDirectory;
            code = "public async Task ExecuteScript(CommanderAddinModel Model)\n" +
                   "{\n" +
                   code + "\n" +
                   "}";
            //"return true;\n" +

            await ScriptInstance.ExecuteMethodAsyncVoid(code, "ExecuteScript", model);            

            Directory.SetCurrentDirectory(oldPath);

            
            if (ScriptInstance.Error)
            {
                // fix error offsets so they match just the script code
                FixupLineNumbersAndErrors(ScriptInstance);  
                ErrorMessage = ScriptInstance.ErrorMessage;
            }

            return !ScriptInstance.Error;
        }

        public static AdjustCodeLineNumberStatus FixupLineNumbersAndErrors(CSharpScriptExecution script)
        {
            var adjusted = new AdjustCodeLineNumberStatus();

            var code = script.GeneratedClassCode;

            var find = "public async Task ExecuteScript(CommanderAddinModel Model)";
            var lines = StringUtils.GetLines(code);
            int i = 0;
            for (i = 0; i < lines.Length; i++)
            {
                if (lines[i]?.Trim() == find)
                    break;
            }

            if (i < lines.Length -1)
                adjusted.startLineNumber = i + 2;

            // `(24,1): error CS0246: The type or namespace name...` 
            lines = StringUtils.GetLines(script.ErrorMessage);
            for (i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrEmpty(line))
                    continue;

                var linePair = StringUtils.ExtractString(line, "(", "):", returnDelimiters: true);
                if (!linePair.Contains(",")) continue;

                var tokens = linePair.Split(',');

                var lineNo = StringUtils.ParseInt(tokens[0].Substring(1), 0);
                if (lineNo < adjusted.startLineNumber) continue;

                // update the line number
                var newError = "(" + (lineNo - adjusted.startLineNumber ) + "," + tokens[1] + line.Replace(linePair, "");
                lines[i] = newError;
            }

            var updatedLines = string.Empty;
            foreach (var line in lines)
                updatedLines = updatedLines + line + "\n";
            
            string.Join(",",lines);
            adjusted.UpdatedErrorMessage = updatedLines.Trim();

            return adjusted;

        }

        /// <summary>
        /// Creates an instance of wwScripting for this parser
        /// with the appropriate assemblies and namespaces set
        /// </summary>
        /// <returns></returns>
        private CSharpScriptExecution CreateScriptObject()
        {
            var scripting = new CSharpScriptExecution
            {
                GeneratedNamespace = "MarkdownMonster.Commander.Scripting",
                ThrowExceptions = false,
                AllowReferencesInCode = true
            };

            // Use loaded references so **all of MM is available**
            scripting.AddLoadedReferences();
                //.AddDefaultReferencesAndNamespaces();

            scripting.AddAssembly(typeof(CommanderAddin));

            scripting.AddNamespaces("System",
                "System.Threading.Tasks",
                "System.IO",
                "System.Reflection",
                "System.Text",
                "System.Drawing",
                "System.Diagnostics",
                "System.Data",
                "System.Data.SqlClient",
                "System.Linq",
                "System.Windows",
                "System.Windows.Controls",
                "System.Collections.Generic",

                "Newtonsoft.Json",
                "Newtonsoft.Json.Linq",

                "MarkdownMonster",
                "MarkdownMonster.Windows",
                "Westwind.Utilities",
                "CommanderAddin");
            
            scripting.SaveGeneratedCode = true;

            return scripting;
        }
    }

    public class AdjustCodeLineNumberStatus
    {
        public string UpdatedErrorMessage { get; set;  }
        public int startLineNumber { get; set;  }
    }
}
#endif