/// <summary>
/// SweetSyntax class for Unity3D
///  
/// This class handle the detection of new or modified scripts in a source language and creates its brother script in a target language.
/// The source language can be anything but the target language must be one 
/// of the three scripting language supported by Unity3D : C#, UnityScript or Boo.
///
/// This "brother script" is created in the same directory and with the same name (but with the extension of the target language).
/// Its content has been translated/converted from the source language to the target language by another class (see inside the Update() method).
///
/// This class is not specific to the source or target language, you just have to change the settings at the beginning of the class below.
///
/// Website : http://www.florent-poujol.fr/en/unity3d/sweetsyntax
/// Documentation : http://www.florent-poujol.fr/content/unity3d/sweetsyntax/
///
///
/// Version: 1.0 (sole version that was ever released)
/// Release: september 2012
///
/// Created by Florent POUJOL
/// florent.poujol@gmail.com
/// http://www.florent-poujol.fr/en
/// Profile on Unity's forums : http://forum.unity3d.com/members/23148-Lion
/// <summary>


using UnityEngine; // Debug
using UnityEditor; // EditorWindow
using System.Collections; // Stack
using System.Collections.Generic; // List, Dico
using System.IO; // File Directory StreamReader/Writer
using System; // DateTime Double Int32


public class SweetSyntax : EditorWindow {


	// You can customize the class's behaviour below by setting some default values


	/// <summary>
	/// Display the extension's window
	/// </summary>
	[MenuItem ("Window/SweetSyntax")]
    static void ShowWindow () {
        SweetSyntax window = (SweetSyntax)EditorWindow.GetWindow (typeof(SweetSyntax));
        window.title = "SweetSyntax";
    }

    /// <summary>
    /// This is where you call your class that perform the conversion
    /// </summary>
    /// <param name="inputCode">The code from the source script, to be converted</param>
    /// <returns>The code converted in the target language</returns>
    string GetConvertedCode (string inputCode) {

    	if (scriptType == ScriptType.SweetScript) {
    		SweetScriptToUnityScript convertor = new SweetScriptToUnityScript (inputCode);
			return convertor.convertedCode;
		}

		else if (scriptType == ScriptType.SweetSharp) {
    		SweetSharpToCSharp convertor = new SweetSharpToCSharp (inputCode);
			return convertor.convertedCode;
		}

		return "";
    }


    // some members below are made public in order to be serialized
    // and thus saved across Unity sessions while the extension is still opened

    // the interval (in second) time between two project checking
	// can be set in the extension's window
	public double m_checkInterval = 1.0;

    // Name and path of the log file
    // the path is relative to the Asset folder (Application.dataPath) and must begin by a slash
	public string m_logPath = "/SweetSyntax/SweetSyntax.log";

	// default number of rows shown in the console
	public string m_strConsoleRows = "10";

	// tell wether to show some debug info at the bottom of the extension's window
	public bool m_showDebug = false;

	// the type of the scripts to convert	
	public enum ScriptType { SweetScript, SweetSharp }
	public ScriptType scriptType = ScriptType.SweetScript;
	
	// script's extension in the source language 
	string[] m_sourceScriptExtension = {".sweetscript", ".sweetsharp"};

	// script's extension in the target language 
	// must be .cs .js or .boo
	string[] m_targetScriptExtension = {".js", ".cs"};

	// name (without extension) and path of the new source scripts 
	// the path is relative to the Asset folder (Application.dataPath) and must begin by a slash
	string[] m_newScriptPath = {"/NewSweetScript", "/NewSweetSharp"};

	// name and path of the template to be used for new sources scripts
	// the path is relative to the Asset folder (Application.dataPath) and must begin by a slash
	// leave empty if you don't want to use a template
	string[] m_newScriptTemplatePath = {
		"/SweetSyntax/SweetScriptTemplate.txt",
		""
	};


	// Don't change anything below this point.
	// You may need to restart the extension for the changes to take effect.
    //--------------------


	// the list of the source scripts that currently exists in the project
	List<string> m_sourceScriptsList = new List<string> ();

	// The list of source scripts that needs to be converted (because they are new or updated)
	List<string> m_scriptsToConvertList = new List<string> ();

	// The list of string to be shown in the extension's console
	public List<string> m_logList = new List<string> ();

	// the last time the project has been checked (the UpdateScripts() method has been called)
	DateTime m_lastCheckTime = DateTime.Now.ToLocalTime (); // using a local time because File.GetLastWriteTime() returns a local time


	// ----------------------------------------------------------------------------------

	/// <summary>
	/// This method check the project for new or modified scripts in the source language
	/// These scripts are added to m_scriptsToConvertList and will be converted in the Update() method below
	/// </summary>
	/// <param name="forceConversion">Force the (re)conversion of every existing script ? Default is false.</param>
	void UpdateScripts (bool forceConversion) {
		if (m_scriptsToConvertList.Count > 0) // prevent checking if a conversion is running
			return;


		// get all files paths
		string[] paths = Directory.GetFiles (Application.dataPath, "*"+m_sourceScriptExtension[(int)scriptType], SearchOption.AllDirectories);
		

		// populate the list of project's scripts
		m_sourceScriptsList.Clear ();

		foreach (string sourceScriptPath in paths) 
			m_sourceScriptsList.Add (GetCleanPath (sourceScriptPath));
		
		/*for (int i = 0; i<paths.Length; i++) {
			paths[i] = paths[i].Replace ("\\", "/");
			m_sourceScriptsList.Add (GetCleanPath (paths[i]));
		}*/


		// populate the list of scripts that needs to be converted
		m_scriptsToConvertList.Clear ();

		if (forceConversion) {
            m_scriptsToConvertList = new List<string> (m_sourceScriptsList);
            Log ("Full re-conversion of "+m_scriptsToConvertList.Count+" source scripts.");
            return;
        }


        // if forceConversion is false, check if some conversion is needed
		foreach (string sourceScriptPath in paths) {
			string cleanSourceScriptPath = GetCleanPath (sourceScriptPath);
			string targetScriptPath = sourceScriptPath.Replace (m_sourceScriptExtension[(int)scriptType], m_targetScriptExtension[(int)scriptType]);

			// first of all check if the related script exists (if not, convert it)
			if ( ! File.Exists (targetScriptPath)) {
				m_scriptsToConvertList.Add (cleanSourceScriptPath);
				Log (m_targetScriptExtension[(int)scriptType]+" script does not exists for source script at path ["+cleanSourceScriptPath+"]");
				continue;
			}

			// does the source file has been modified since the last conversion ?   if yes => convert
			// check if the last write time of the source file is superior to the one on the converted file
			if (File.GetLastWriteTime (sourceScriptPath) > File.GetLastWriteTime (targetScriptPath)) { // GetLastWriteTime returns a local time
				m_scriptsToConvertList.Add (cleanSourceScriptPath);
				Log ("Source script at path ["+cleanSourceScriptPath+"] was modified.");
				continue;
			}
		}

		if (m_scriptsToConvertList.Count > 0) {
			Log ("Converting "+m_scriptsToConvertList.Count+" source scripts.");
		}
	}

	
	// ----------------------------------------------------------------------------------

	/// <summary>
	/// Called 100 times per second
	/// Check for new or updated source scripts in the project all [m_checkInterval] seconds
	/// Convert one file per frame if needed.
	/// </summary>
	void Update () {
		// is it time to check the project ?
		DateTime currentTime = DateTime.Now.ToLocalTime ();

		if (m_scriptsToConvertList.Count <= 0 && currentTime > m_lastCheckTime.AddSeconds (m_checkInterval)) {
			m_lastCheckTime = currentTime;
			
			// check if there is a need for conversion
			UpdateScripts (false);
		}

		// perform the conversion in this block, one file per frame
		else if (m_scriptsToConvertList.Count > 0) {
			string cleanPath = m_scriptsToConvertList[0];
			string path = GetFullPath (cleanPath) + m_sourceScriptExtension[(int)scriptType];
			
			if ( ! File.Exists (path)) {
				path = path.Replace (Application.dataPath, "");
				Debug.LogError ("SweetSyntax.Update() : Source file at ["+path+"] can't be converted because it is missing.");
				m_scriptsToConvertList.Remove (path);
				return;
			}


			// read, convert, write
			StreamReader reader = new StreamReader (path);
			string inputCode = reader.ReadToEnd ();
			reader.Close ();


			string outputCode = "";

			if (inputCode.Trim () != "")
				outputCode = GetConvertedCode (inputCode);
			

			path = path.Replace (m_sourceScriptExtension[(int)scriptType], m_targetScriptExtension[(int)scriptType]);
			StreamWriter writer = new StreamWriter (path);
			writer.Write (outputCode);
			writer.Flush ();
			writer.Close ();

			m_scriptsToConvertList.Remove (cleanPath);
			
			Log ("Converted "+path);

			if (m_scriptsToConvertList.Count <= 0) // all files have been converted, refresh the project now
				AssetDatabase.Refresh ();
		}
	}


	// ----------------------------------------------------------------------------------

	/// <summary>
	/// Write infos in the Log window and file
	/// </summary>
	/// <param name="logInfos">The information to log</param>
	void Log (string logInfos) {
		string entry = DateTime.Now.ToLocalTime ()+" "+logInfos;

		m_logList.Add (entry);
		Repaint (); // refresh the extension's GUI

		// write in the log file
		string path = GetFullPath (m_logPath);
		string logDirectory = path.Remove (path.LastIndexOf ("/"));
		Directory.CreateDirectory (logDirectory); // make sure the directory exist, or create it
		
		StreamWriter writer = new StreamWriter (path, true); // open/create the file and append content
		writer.WriteLine (entry);
		writer.Flush ();
		writer.Close ();
	}


	// ----------------------------------------------------------------------------------

	/// <summary>
	/// Read the log file and populate the m_logList list whose content is shown in the extension window
	/// </summary>
	void PopulateLogWindow () {
		string path = GetFullPath (m_logPath);
		m_logList.Clear ();

		if (File.Exists (path)) {
			StreamReader reader = new StreamReader (path);
			string line = "";

			while (true) {
				line = reader.ReadLine ();
				if (line == null)
					break;

				m_logList.Add (line);
			}

			reader.Close ();
		}
		else
			m_logList.Add ("Can't populate console because file ["+m_logPath+"] does not exists.");

		Repaint (); 
	}


	// ----------------------------------------------------------------------------------

	Vector2 scrollPosition;
	bool showLogWindow = true;

	/// <summary>
	/// Draw the GUI
	/// </summary>
	void OnGUI () {
		GUILayout.Space (10);

		scriptType = (ScriptType)EditorGUILayout.EnumPopup ("Script Type", scriptType);

		GUILayout.Space (10);

		// force project refresh
		if (GUILayout.Button ("Force project refresh/recompile", GUILayout.MaxWidth (300))) {
			AssetDatabase.Refresh ();
		}

		// create new source script
		if (GUILayout.Button ("Create "+m_sourceScriptExtension[(int)scriptType]+" script in project", GUILayout.MaxWidth (300))) {
			string newScriptPath = GetFullPath (m_newScriptPath[(int)scriptType]);
			
			if (m_newScriptPath[(int)scriptType] == "")
				newScriptPath = Application.dataPath+"/NewBehaviourScript";

			StreamWriter newSourceScript = new StreamWriter (newScriptPath + m_sourceScriptExtension[(int)scriptType]);

			// populate the new script with the template
			string newScriptContent = "";

			if (m_newScriptTemplatePath[(int)scriptType] != "") {
				string templatePath = GetFullPath (m_newScriptTemplatePath[(int)scriptType]);

				if (File.Exists (templatePath)) {
					StreamReader template = new StreamReader (templatePath);
					newScriptContent = template.ReadToEnd ();
					template.Close ();
				}

				newSourceScript.Write (newScriptContent);
				newSourceScript.Flush ();
			}
			
			newSourceScript.Close ();
			AssetDatabase.Refresh ();
		}

		// force update and conversion
		GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Force update", GUILayout.MaxWidth (150))) {
				m_scriptsToConvertList.Clear ();
				Log ("Update scripts");
				UpdateScripts (false);
			}

			GUILayout.Space (20);

			if (GUILayout.Button ("Force the re-conversion of all scripts", GUILayout.MaxWidth (250))) {
				m_scriptsToConvertList.Clear ();
				Log ("Force re-conversion");
				UpdateScripts (true); // force complete conversion
			}
		GUILayout.EndHorizontal ();	

		GUILayout.Space (10);

		GUILayout.BeginHorizontal ();
			GUILayout.Label ("Time in seconds between two automatic updates", GUILayout.MaxWidth (300));
			string stringCheckInterval = EditorGUILayout.TextField (m_checkInterval.ToString(), GUILayout.MaxWidth (50));
			m_checkInterval = Double.Parse (stringCheckInterval);
		GUILayout.EndHorizontal ();	


		//--------------------
		// log

		GUILayout.Space (20);
		GUILayout.Label ("########## Log ##########");
		GUILayout.Space (20);

		GUILayout.BeginHorizontal ();
			GUILayout.Label ("Path and name of the log file", GUILayout.MaxWidth (300));
			m_logPath = GUILayout.TextField (m_logPath);
		GUILayout.EndHorizontal ();

		GUILayout.Space (10);

		GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Populate log window", GUILayout.MaxWidth (170)))
				PopulateLogWindow ();

			GUILayout.Space (5);

			if (GUILayout.Button ("Clear log window", GUILayout.MaxWidth (170))) {
				m_logList.Clear ();
				Repaint ();
			}
				
			GUILayout.Space (5);

			if (GUILayout.Button ("Clear log file", GUILayout.MaxWidth (170))) {
				string path = GetFullPath (m_logPath);
				
				if (File.Exists (path)) {
					StreamWriter writer = new StreamWriter (path);
					writer.Close ();
				}
				
				// file cleared, now clear console as well
				m_logList.Clear ();
				Repaint ();
			}

		GUILayout.EndHorizontal ();

		GUILayout.Space (10);

		GUILayout.BeginHorizontal ();
			GUILayout.Label ("Number of rows shown in the console", GUILayout.MaxWidth (300));
			m_strConsoleRows = GUILayout.TextField (m_strConsoleRows, GUILayout.MaxWidth (50));

			if (m_strConsoleRows.Trim () == "" || Int32.Parse (m_strConsoleRows.Trim ()) == 0)
				showLogWindow = false;
			else
				showLogWindow = true;
		GUILayout.EndHorizontal ();

		if (showLogWindow) {
			GUILayout.Label ("The logs are showns below, the newest at the top :");

			GUILayout.Space (10);
			GUILayout.Label ("			------------------------------");

			// a height of 50 shows 3 rows
			int consoleHeight = Int32.Parse(m_strConsoleRows) * (50/3);
			scrollPosition = EditorGUILayout.BeginScrollView (scrollPosition, false, true, GUILayout.Height (consoleHeight));

			Stack stack = new Stack (m_logList); // the latest logs are at the end of m_logList but I want to show them at the top of the window

			// populate the stack
			//foreach (string text in m_logList)
			//	stack.Push (text);

			foreach (string text in stack)
				GUILayout.Label (text);
			
			EditorGUILayout.EndScrollView();

			GUILayout.Label ("			------------------------------");
		}

		GUILayout.Space (10);


		//--------------------
		// debug

		m_showDebug = GUILayout.Toggle (m_showDebug, "Show debug infos");

		if (m_showDebug) {
			GUILayout.Space (10);
			GUILayout.Label ("########## Debug ##########");
			GUILayout.Space (20);

			GUILayout.Label ("Number of source scripts in the project : "+m_sourceScriptsList.Count+" (m_sourceScriptsList)");
			GUILayout.Label ("Number of scripts to convert : "+m_scriptsToConvertList.Count+" (m_scriptsToConvertList)");
		}
	}


	// ----------------------------------------------------------------------------------

    /// <summary>
    /// Return the input path without Application.dataPath, without extension and with all backward slashes converted to forward slahes
    /// </summary>
    /// <param name="inputPath">A path to clean</param>
    /// <returns>The clean path</returns>
	string GetCleanPath (string inputPath) {
		return inputPath.
		Replace (Application.dataPath, "").
		Replace (m_sourceScriptExtension[(int)scriptType], "").
		Replace (m_targetScriptExtension[(int)scriptType], "").
		Replace ("\\", "/");
	}


	// ----------------------------------------------------------------------------------

	/// <summary>
    /// Return the input relative (to the Asset folder) path as a absolute path 
    // It just add Application.dataPath to the input path
    /// </summary>
    /// <param name="relativePath">The inpout relative (to the Asset folder) path</param>
    /// <returns>The absolute path</returns>
	string GetFullPath (string relativePath) {
		if ( !relativePath.StartsWith ("/") )
			relativePath = "/"+relativePath;

		return Application.dataPath+relativePath;
	}
} // end of class SweetSyntax
