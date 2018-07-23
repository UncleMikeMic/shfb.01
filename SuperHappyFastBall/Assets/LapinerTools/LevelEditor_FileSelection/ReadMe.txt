MRLE Extension - Save/Load file picker

!!!
This is an extension for the Multiplatform Runtime Level Editor (MRLE). To compile it requires the main MRLE package! Main package link: http://u3d.as/9gZ
This extension does not work in the Unity Web Player, because file access from the browser is not permitted.
!!!

Description:
Use this extension to allow your users to save and load multiple level files.
It contains a popup that allows you to enter a new file name or to pick a level from existing files.

Installation:
- import this package and the default demo (LE_ExampleEditor) will automatically show the file selection on level save or load.
- simply use LE_ExtensionInterface.Load or LE_ExtensionInterface.Save properties to trigger the popup from your code.
- Examples using the LE_ExtensionInterface are in the ExampleGame_Editor class (in the main package).