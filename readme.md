# Markdown Monster Commander Addin

#### A C# scripting addin for Markdown Monster

<img src="icon.png" Height="128"  />

This addin allows you to script Markdown Monster via C# script code. It's an easy way to add quick automation features to Markdown Monster without creating a [full blown Markdown Monster .NET Addin](http://markdownmonster.west-wind.com/docs/_4ne0s0qoi.htm).

![](CommanderAddin/screenshot.png)

Some things you can do:

* Access the current document
* Modify the active or any other open document
* Launch a new process 
* Open documents or folders in an external Program
* Load data from a database and embed it in a document
* Open a Git Client in the right folder context
* Open all related documents
* Launch an image optimizer to optimize all images
* Create pre-filled documents into the editor  
(which you can also do with the [Snippets addin]())
* etc.

Using the Command Addin you can access the [Markdown Monster Application Model](http://markdownmonster.west-wind.com/docs/_5dv018sft.htm) (via `Model.AppModel`) to access many of the core Markdown Document and UI features that let you automate and act upon open or selected documents in the editor.

You can tie a hot key to a script (may require a restart of the application) to make functionality more easily accessible as well.

### Features
Commander has the following  useful features:

* C# Scripting - full C# language access
* Access to Markdown Monster's Model
    * Access to Document
    * Access to Editor
    * Access to Main Window and the entire WPF UI
* Console Output
* Language Features
    * Latest C# compiler features
    * Compilation and Runtime Error reporting
    * Source Code display


### C# Script Execution
Scripts are executed as C# code that is compiled dynamically as 'in memory' assemblies. The addin caches generated assemblies so that multiple executions don't keep generating new assemblies.

You can execute any .NET code as long as you can reference the required assemblies you need for your code.

#### Script Execution
Scripts are turned into a C# class that is then compiled into an assembly and executed. Assemblies are created dynamically and cached once generated based on the code snippet's content (plus the compiler mode). If you execute the same script repeatedly, one assembly is generated and used repeatedly. Each new or modified snippet generates a new assembly when it is executed for the first time - subsequent invocations are cached and thus faster.

Scripts include default assemblies and namespaces that are used in Markdown Monster , so most features that are used in Markdown Monster are already in scope and accessible without adding explicit `#r` assembly references or `using` statements. Add references and namespaces only if you get compilation errors to that effect.

Scripts execute in the context of a class method in the following format:

```cs
24.  public object ExecuteCode(params object[] parameters)
25.  {dynamic Model = parameters[0];
26.  var docFile = Model?.ActiveDocument?.Filedname;
27.  if(docFile == null)
28.      return false;
29.  
30.  var pf =  Environment.GetEnvironmentVariable("ProgramW6432");
31.  var folder = Path.Combine(pf,"SmartGit\\bin");
32.  var exe = Path.Combine(folder,"smartgit.exe");
33.  
34.  var pi = Process.Start(exe,"\"" + docFile + "\"");
35.  if (pi != null)
36.      Model.Window.ShowStatus("Smartgit started...",5000);
37.  else
38.      Model.Window.ShowStatus("Failed to load Smartgit...",5000);
39.  
40.  
42.  return null;  // generated
43.  }
```

The method is passed a `CommanderAddinModel` instance which is made available as a `Model` variable. This type exposes most of the common top level objects in Markdown Monster plus MM's main application model:

* **[AppModel](http://markdownmonster.west-wind.com/docs/_5dv018sft.htm)**  
Markdown Monster's Main Application Model that gives access to configuration, window, document, editor, addins and much more. A subset of the properties on this object are exposed in this AddinModel reference for easier access.
* **[Window](http://markdownmonster.west-wind.com/docs/_5dv018t7x.htm)**  
The main Markdown Monster UI Window which includes access to open tabs, the folder browser and more.
* **[ActiveDocument](http://markdownmonster.west-wind.com/docs/_5dv018tmz.htm)**  
The document in the active tab. Contains the content, filename, load and save operations etc.
* **[ActiveEditor](http://markdownmonster.west-wind.com/docs/_5dv018twa.htm)**  
provides access to the active editor instance, for manipulating the document's content in the editor.
* **Window.FolderBrowser**  
lets you access the open folder browser or open a new folder in the folder browser.

Methods are executed as code snippets so you don't need to return a value. However, if you need to exit early from a snippet use `return null` or `return someValue`. 

> #### Early Exit via `return`
> If you need to exit the script early using `return` make sure that return some value as the wrapper method signature requires. The code returns `return false` although the return value of the script is irrelevant and ignored. But some value has to be returned.
>   
> A return value is **not required** if you don't exit early as the wrapper method has a `return null` at the end.

#### Assembly Reference and Namespace Dependencies
In order to execute code, the generated assembly has to explicitly load referenced assemblies. The addin runs in the context of Markdown Monster so all of Markdown Monster's references are already preloaded. The addin also pre-loads most of the common namespaces into the generated class. Ideally don't add references or namespaces unless you get a compilation error to that effect. You can look at the source code to see what namespaces are auto-generated.

If you do need to load additional assemblies you can do so using special reference syntax:

```cs
#r System.Windows.Forms.dll
using System.Windows.Forms;

// Your method code here
System.Windows.MessageBox.Show("Got it!");
```

##### #r <assemblyDll>
The `#r` directive is used to reference an assembly by filename. Assemblies should be referenced as `.dll` file names and cannot contain a path. Assemblies referenced have to either be a GAC installed assembly or they must live in Markdown Monster's startup code to be found. 

> #### No external Assemblies allowed
> We don't allow referencing of pathed assemblies. All assemblies should be referenced just by their .dll file name (ie. `mydll.dll`).
>
> You can only load assemblies located in the GAC, the Markdown Monster Root Folder or the `Addins` folder and below (any sub-folder). These folders are restricted on a normal install and require admin rights to add files, so random files cannot be copied there easily and maliciously executed.
>
> If you need external assemblies in your Scripts or Snippets we recommend you create a folder below the Addins folder like `SupportAssemblies` and put your referenced assemblies there.. 

##### using <namespace>
This allows adding namespace references to your scripts the same way you'd use a using statement in a typical .NET class. Make sure any Assemblies you need are loaded. The Command Addin pre-references many common references and namespaces.

Both of these commands have to be specified at the top of the script text as they are parsed out and added back when code is generated and compiled.

> #### No Support for Using Aliases
> This addin **does not support C# using aliases**.  The following does not work and will result in a compilation error:
>
> ```cs
> using MyBuilder = System.Text.StringBuilder;
> ```

#### Error Handling
The Command Addin has support for capturing and displaying Compiler and Runtime errors in the Console Output of the interface.

![](CommanderAddin/ErrorInConsole.png)

Each script is generated into a self-contained CSharp class that is compiled into its own assembly and then loaded and executed from the generated in-memory assembly. 

If there are source code errors that cause a compiler error, the Addin displays the compiler error messages along with the line number where errors occurred.

Runtime errors capture the last Call Stack information and provide the last executing line of code that caused the error. This may not always represent the real source of the error since you are executing generated code, but often it does provide some insight into code generated.

Both Compiler and Runtime errors also display the source code along with line numbers so you can co-relate compiler errors to specific lines in the source code:

### Simple Examples
The following script in the figure retrieves the active document's filename, then shows a directory listing of Markdown Files in the same folder in the Console, and then asks if you want to open the listed files in the editor:

![](CommanderAddin/screenshot.png)

Here are a few more useful examples that you can cut and paste right into the Commander Addin for your own use:

#### Open in SmartGit
Example demonstrates how to launch an external process and open a folder in a GIT client using the .NET `Process` class.

```cs
var docFile = Model?.ActiveDocument?.Filename;
if(docFile == null)
    return false;

var pf =  Environment.GetEnvironmentVariable("ProgramW6432");
var folder = Path.Combine(pf,"SmartGit\\bin");
var exe = Path.Combine(folder,"smartgit.exe");

var pi = Process.Start(exe,"\"" + docFile + "\"");
if (pi != null)
    Model.Window.ShowStatus("Smartgit started...",5000);
else
    Model.Window.ShowStatus("Failed to load Smartgit...",5000);
```

#### Open All Documents in a Folder in MM
The following retrieves the active document's filename and based on that gets a directory listing for all other `.md` files and optionally opens them all in the editor:

```cs
#r Westwind.Utilities.dll

using Westwind.Utilities;
using System.Windows;
using System.IO;

var doc = Model.ActiveDocument.Filename;
var docPath = Path.GetDirectoryName(doc);

Console.WriteLine("Markdown Files in: " + docPath);

foreach(var file in Directory.GetFiles(docPath,"*.md"))
{
    Console.WriteLine(" - " + file + " - " + 
                     StringUtils.FromCamelCase(Path.GetFileName(file)));    
}

if(MessageBox.Show("Do you want to open these Markdown Files?",
                 "Open All Markdown Files",
                 MessageBoxButton.YesNo,
                 MessageBoxImage.Information) == MessageBoxResult.Yes)
{
    foreach(var file in Directory.GetFiles(docPath,"*.md"))
        Model.Window.OpenTab(file);
}        
```

Note that the `Westwind.Utilities` assembly and namespace definitions are actually optional since they are automatically included in the default list of added assemblies and namespaces - they serve mainly for demonstration purposes.

For security reasons you can load assemblies only from the GAC or from assemblies that are located in the startup folder of Markdown Monster.

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

#### Open Folder Browser File in VS Code
Here's a command that opens the active FolderBrowser item in Visual Studio Code. It uses the FolderBrowser's `GetSelectedPathItem()` method to retrieve the Path item from and extracts the filename then tries to find VS Code to open the file

```cs
var folderBrowser = Model.Window?.FolderBrowser as FolderBrowerSidebar;

string docFile = null;

var item = folderBrowser.GetSelectedPathItem();
docFile = item?.FullPath;

if (string.IsNullOrEmpty(docFile) || docFile == ".." ||
   (!File.Exists(docFile) && !Directory.Exists(docFile) ))
{
    docFile = Model?.ActiveDocument?.Filename;
    if (docFile == null)
        return false;
}

var exe = @"%localappdata%\Programs\Microsoft VS Code\Code.exe";
exe = Environment.ExpandEnvironmentVariables(exe);

var pi = Process.Start(exe,"\"" + docFile + "\"");
if (pi != null)
    Model.Window.ShowStatus($"VS Code  started with: {docFile}",5000);
else
    Model.Window.ShowError("Failed to start VS Code.");

```

#### Print an Assembly List
This is a trivial example, but useful to track what assemblies and addins were loaded by Markdown Monster.

```cs
var assemblies = AppDomain.CurrentDomain.GetAssemblies();

foreach(var assembly in assemblies)
{
	Console.WriteLine(assembly);
}
```

