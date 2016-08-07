# SweetSyntax

## Notice

SweetSyntax is a side project I work on while working on script converters for Unity3D.
The extension is actualy not much practical to use since you must keep the focus on the extension's tab at all times.

## What is SweetSyntax

SweetSyntax is an extension for the Unity3D engine that aims to sweeten the syntax of UnityScript and C# mostly by providing syntactic sugars.

SweetSyntax allows you to use two pseudo language : SweetScript and Sweet# that are overlays of UnityScript and C#, respectively.

More precisely, SweetSyntax aims to provide a syntax :

* more human-friendly, using actual words instead of expressions (ie : "and" instead of "&&").
* more lightweight, and less verbose. It allows to omit unnecessary code, or provide shortcuts for character-heavy expression (ie : a readonly property in SweetScript : prop PropertyName get;).
* less error-prone. It allows beginners (and more experienced coder as well !) to focus on the logic of the code more than on syntactic bug-generating details.

## How to install

Import the SweetSyntax.unitypackage in your project.
Just put the "SweetSyntax" folder anywhere in your project's Assets folder.

Open the extension by clicking "SweetSyntax" in the "Window" menu. As you must keep the extension running at all times, pin the tab somewhere and keep the focus on it (no side tab). 

## Documentation

Check the "Manual" folder