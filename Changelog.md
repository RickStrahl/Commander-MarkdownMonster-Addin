<img src="icon.png" Height="128"  align="right" />

# Change Log Commander Addin for Markdown Monster 

### 0.3.9 
<i><small>not released yet</small></i>

* **Add WindowStyle flag to ExecuteProcess Helper**   
Add flag to allow controlling window size and default to hidden, so processes run invisibly.

* **ShortCut Parsing Improvements**  
Fix a number of edge cases for shortcut maps for commands. Only support single gestures (Ctrl-o, Ctrl-Shift-o, but not Ctrl-O,L) for example.

* **Fix: Load Errors for Key Bindings**  
Fix the load errors for key bindings and continue loading bindings.

### 0.3
<i><small>May 14th, 2017</small></i>

* **Remember Last Command Used**  
The Addin now remembers which common was last selected and restores it when the dialog is restarted so you can more easily re-run your last command.

* **Error Messages in main Status Bar**   
Command execution now shows a confirmation or failure message in the main window's status bar so you can see that something is happening when the Commander window is not open. 

* **Add default Scripts for HelloWorld and Push to Git**  
Added two default scripts that demonstrate basic operation of the addin, one to run a loop and output a string, one to execute a Git to stage, commit and push the current document to Git.

* **Fix Console Output to scroll to Bottom**  
Change behavior of the console to always display the end of the console output by scrolling the textbox to the bottom.

* **Fix: Execution off shortcut key without Window Open**  
This fixes an issue where you couldn't successfully execute an addin that used the Model when the Command Window wasn't open. Model data is now properly passed even if the Window has not been opened.

### 0.2
<i><small>February 22, 2017</small></i>

* **Console Output Display in Editor**   
When you test your scripts in the Addin editor, you now can use Console output to write to the captured console window on the form. The Console displays output from your code when writing to the Console, as well as displaying compiler and runtime error messages.

* **Errors can now optionally open Compiled Source**  
When a compilation error occurs during script execution you can now optionally pop open a new Markdown Monster editor window with the full compiled source code. You can match error line numbers to code lines.

### 0.1
<i><small>February 12, 2017</small></i>

* **Add Keyboard Hotkey Support**  
You can now bind a keyboard hotkey to your Script so you can invoke it from within the editor. Hotkeys take the format of Alt-Shift-N, Ctrl+Alt+F1 etc.

