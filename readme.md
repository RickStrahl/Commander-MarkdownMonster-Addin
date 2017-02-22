# Markdown Monster Commander Addin

#### A C# scripting addin for Markdown Monster

<img src="icon.png" Height="128"  />

> #### Pre-release 
> This addin is currently in alpha state and only available in source code format.

This add-in lets you extend Markdown Monster via custom C# scripts or by easily calling external executables that can be tied to a hotkey or can be executed via the Commander addin's user interface. Commander scripts can be thought of as mini add-ins that can be created without requiring the creation of a full blown addin using small snippets of C# Script code.

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

### Simple Examples
The following script in the figure retrieves the active document's filename, then shows a directory listing of Markdown Files in the same folder in the Console, and then asks if you want to open the listed files in the editor:

![](CommanderAddin/screenshot.png)


You can also launch external code easily. For example to launch my Git client (SmartGit) in the repo for the current document I can do:

![](screenshot2.png)

### C# Script Execution
Scripts are executed as C# code, using a dynamically generated method inside of an in-memory assembly. You can pretty much execute any .NET code as long as you can reference the required assemblies you need for your code to execute.

### Assembly Reference and Namespace Dependencies
In order to execute code, the generated assembly has to explicitly reference any reference assembly references. 

The script parser used in this add-in allows for custom syntax at the top of the script to specify assembly references and namespaces as follows.

* `#r <assemblyReference>`   
This loads an assembly reference into the script - you can reference any GAC assembly or any assembly that is available through Markdown Monster's root folder. Note: **no externally loaded assemblies are allowed** which means anything you want to use you have to explicitly copy into **the Markdown Monster install folder**.

* `using <namespace>`  
This allows adding namespace references to your scripts the same way you'd use a using statement in a typical .NET class. 

Both of these commands have to be specified at the top of the script text.

Other than that you can 

