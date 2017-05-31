# Markdown Monster Commander Addin

#### A C# scripting addin for Markdown Monster

<img src="icon.png" Height="128"  />

> #### Currently in Beta release 

This addin lets you extend Markdown Monster via custom C# scripts or by easily calling external executables that can be tied to a hotkey or can be executed via the Commander addin's user interface. Commander scripts can be thought of as mini addins that can be created without requiring the creation of a full blown addin using small snippets of C# Script code.

### Features
Commander has the following  useful features:

* C# Scripting - full C# language access
* Reference and Namespace imports
* Access to Markdown Monster's Model
    * Access to Document
    * Access to Editor
    * Access to Window and WPF UI
* Console Output for Debugging

### What can you do with it?
Heck the sky's the limit, but here are some simple things I've done with it just in the last couple of weeks:

* Open my Git Client in the right folder context
* Open all related documents
* Launch an image optimizer to optimize all images
* Creating pre-filled documents into the editor  
(which you can also do with the [Snippets addin]())
* Shell out to any external application using `ExecuteProcess()`

### C# Script Execution
Scripts are executed as C# code, using a dynamically generated method inside of an in-memory assembly. You can pretty much execute any .NET code as long as you can reference the required assemblies you need for your code to execute.

### Assembly Reference and Namespace Dependencies
In order to execute code, the generated assembly has to explicitly load referenced assemblies. The script parser used in this addin allows for custom syntax at the top of the script to specify assembly references and namespaces as follows:

```cs
#r Westwind.Utilities.dll
using Westwind.Utilities

// Your method code here
System.Windows.MessageBox.Show("Got it!");
```

### `#r <assemblyReference>`
The `#r` directive is used to reference an assembly by filename. Note you don't have to specify a path as the addin only looks for assemblies in the Markdown Monster install path.

This loads an assembly reference into the script - you can reference any GAC assembly or any assembly that is available through Markdown Monster's root or `Addins` folder. 

> #### No external Assemblies allowed
> For security reasons, we don't allow execution of pathed assemblies. All assemblies should be referenced just by their .dll file name (ie. `mydll.dll`).
>
>You can only load assemblies located in the GAC, the Markdown Monster Root Folder or the `Addins` folder and below (any sub-folder). These folders are restricted on a normal install and require admin rights to add files, so random files cannot be copied there easily and maliciously executed.
>
> If you need external assemblies in your Scripts or Snippets we recommend you create a folder below the Addins folder like `SupportAssemblies` and put your referenced assemblies there.. 

### `using <namespace>`  
This allows adding namespace references to your scripts the same way you'd use a using statement in a typical .NET class. Make sure any Assemblies you need are loaded. The Command Addin pre-references many common references and namespaces.

Both of these commands have to be specified at the top of the script text.

### Simple Examples
The following script in the figure retrieves the active document's filename, then shows a directory listing of Markdown Files in the same folder in the Console, and then asks if you want to open the listed files in the editor:

![](CommanderAddin/screenshot.png)

Here are a few more useful examples that you can cut and paste right into the Commander Addin for your own use:

#### Open in SmartGit
Example demonstrates how to launch an external process and open a folder in a GIT client using the .NET `Process` class.

```cs
// Open current Git Repo in SmartGit
// with Repo and file pre-selected
using System.IO;
using System.Diagnostics;

var docFile = Model.ActiveDocument.Filename;

var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
var folder = Path.Combine(pf86,"SmartGit\\bin");
var exe = Path.Combine(folder,"smartgit.exe");

var pi = Process.Start(exe,docFile);
if (pi != null)
    Model.Window.ShowStatus("Smartgit started...",5000);
else
    Model.Window.ShowStatus("Failed to load Smartgit...",5000);
```

#### Commit Active File to Git Origin
Demonstrates committing the current document to Git locally and then pushing changes to a Git host using the `ExecuteHelper()` function:

```cs
// ASSSUMES: Git is in your system path. Otherwise add path here
var gitExe = "git.exe";

var doc = Model.ActiveDocument.Filename;
var docFile = Path.GetFileName(doc);
var docPath = Path.GetDirectoryName(doc);

// MM will reset path when script is complete
Directory.SetCurrentDirectory(docPath);

Console.WriteLine("Staging " + docFile);
int result = Model.ExecuteProcess(gitExe,"add --force -- \"" + docFile + "\"");
Console.WriteLine(result == 0 ? "Success" : "Nothing to stage. Exit Code: " + result);

Console.WriteLine("Committing...");
result = Model.ExecuteProcess(gitExe,"commit -m \"Updating documentation for " + docFile +"\"");
Console.WriteLine(result == 0 ? "Success" : "Nothing to commit. Exit Code: " + result);

if (result != 0)
   return null;

Console.WriteLine("Pushing...");
result = Model.ExecuteProcess(gitExe,"push --porcelain --progress --recurse-submodules=check origin refs/heads/master:refs/heads/master");
Console.WriteLine(result == 0 ? "Success" : "Nothing to push. Exit Code: " + result);
```



