using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

class ScriptConvertorDevExtension_UnityScriptToCSharp : EditorWindow { 

	[MenuItem ("Window/Dev. extension UStoCSharp")]
    static void ShowWindow () {
        ScriptConvertorDevExtension_UnityScriptToCSharp window = (ScriptConvertorDevExtension_UnityScriptToCSharp)EditorWindow.GetWindow(typeof(ScriptConvertorDevExtension_UnityScriptToCSharp));
        window.title = "ScriptConvertorDevExtension_UnityScriptToCSharp";
    }
    
    public string m_scriptName = "Test1";
    //public DateTime m_scriptLastWriteTime = DateTime.Now.ToLocalTime ();
    public string m_scriptRelativePath = "/GitIgnore/ScriptsToBeConverted/";

    // the interval (in second) time between two project checking
	// can be set in the extension's window
	double m_checkInterval = 1;
	public DateTime m_lastCheckTime = DateTime.Now.ToLocalTime ();

    // script's extension in the source language 
	public string m_sourceScriptExtension = ".js";

	// script's extension in the target language 
	// must be .cs .js or .boo
	public string m_targetScriptExtension = ".cs";


	public bool doConvert = false;


	// ----------------------------------------------------------------------------------


	void OnGUI() {

		m_scriptRelativePath = EditorGUILayout.TextField ("Script relative path : ", m_scriptRelativePath);
		m_scriptName = EditorGUILayout.TextField("Script name : ", m_scriptName);
		m_sourceScriptExtension = EditorGUILayout.TextField("Source extension : ", m_sourceScriptExtension);
		m_targetScriptExtension = EditorGUILayout.TextField("Target extension : ", m_targetScriptExtension);

		//doConvert = GUILayout.Toggle(doConvert, "Do Convert");

		// CSharpToUnityScriptConverter.convertMultipleVarDeclaration = GUILayout.Toggle(CSharpToUnityScriptConverter.convertMultipleVarDeclaration, "ConvertMultipleVarDeclaration");
		// CSharpToUnityScriptConverter.removeRefKeyword = GUILayout.Toggle(CSharpToUnityScriptConverter.removeRefKeyword, "removeRefKeyword");
		// CSharpToUnityScriptConverter.removeOutKeyword = GUILayout.Toggle(CSharpToUnityScriptConverter.removeOutKeyword, "removeOutKeyword");


		if (GUILayout.Button("Force Conversion", GUILayout.MinHeight(100)))
    		Convert(true);
    }


    void Update() {
    	if (DateTime.Now.ToLocalTime () > m_lastCheckTime.AddSeconds(m_checkInterval)) {
			m_lastCheckTime = DateTime.Now.ToLocalTime();

			if (doConvert)
				Convert(false);
		}
    }

    UnityScriptToCSharpConverter converter;

    void Convert (bool forceConversion) {

    	
    	
		string sourceScriptPath = Application.dataPath + m_scriptRelativePath + m_scriptName + m_sourceScriptExtension;
		string targetScriptPath = sourceScriptPath.Replace (m_sourceScriptExtension, m_targetScriptExtension);

		if ( ! File.Exists (sourceScriptPath)) {
			Debug.LogError ("CustomScriptDev.Convert() : source script does not exists at path ["+sourceScriptPath+"]");
			return;
		}

		// convert if forcer, or target does not exist, or source newer than target
		if (forceConversion || 
			! File.Exists (targetScriptPath) || 
			File.GetLastWriteTime (sourceScriptPath) > File.GetLastWriteTime (targetScriptPath)) {

			

			StreamReader reader = new StreamReader (sourceScriptPath);
			string inputCode = reader.ReadToEnd ();
			reader.Close ();

			string outputCode;

            Script script = new Script(m_scriptRelativePath, inputCode);
            List<Script> scriptList = new List<Script>();
            scriptList.Add(script);
    		converter = new UnityScriptToCSharpConverter();
			//CSharpToUnityScriptConverter convertor = new CSharpToUnityScriptConverter (inputCode);
			outputCode = converter.Convert(script);
			
			StreamWriter writer = new StreamWriter (targetScriptPath);
			writer.Write(outputCode);
			writer.Flush ();
			writer.Close ();

			Debug.Log ("Convert "+m_scriptName+" at "+DateTime.Now.ToLocalTime ());
			AssetDatabase.Refresh ();
		}
    }
} // end of class CustomScriptDev_CSharpToUnityScript
