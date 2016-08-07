/// <summary>
/// UnityScriptToCSharpExtension class for Unity3D
///
/// This class is part of the "UnityScript to C# Converter" extension for Unity3D.
/// It uses the UnityScriptToCSharpConverter class to convert scripts from C# to UnityScript.
///
/// Check out the online manual at : http://florentpoujol.github.com/UnityScriptToCSharpConverter
///
/// Created by Florent POUJOL
/// florent.poujol@gmail.com
/// http://www.florent-poujol.fr/en
/// Profile on Unity's forums : http://forum.unity3d.com/members/23148-Lion
/// </summary>


using UnityEngine;
using UnityEditor;
using System.Collections; // Stack
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO; // StreamReader/Writer
    

public class UnityScriptToCSharpExtension : EditorWindow
{
    // the directory where to get the scripts to be converted
    public string sourceDirectory = "/CSharpToUnityScript/ScriptsToBeConverted/";

    // the directory where to put the converted scripts
    public string targetDirectory = "/CSharpToUnityScript/ConvertedScripts/";

    // a list of structure that contains all needed infos about the script to be converted
    List<Script> scriptsToConvertList = new List<Script>();

    bool doConvertScripts = false;

    // the instance of the converter class
    UnityScriptToCSharpConverter converter;

    string conversionDescription = "";


    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Method that will be called when the extension will be openen in the Unity editor
    // The MenuItem attribute defineds which menu item will trigger the call to this method
    /// </summary>
    [MenuItem("Window/UnityScript To C# Converter")]
    static void ShowWindow() 
    {
        UnityScriptToCSharpExtension window = (UnityScriptToCSharpExtension)EditorWindow.GetWindow(typeof(UnityScriptToCSharpExtension));
        window.title = "US to C#";
    }


    /// <summary>
    /// Draw the GUI
    /// </summary>
    void OnGUI() 
    {
        GUILayout.Label("The two paths below are relative to the \"Assets\" directory");

        sourceDirectory = EditorGUILayout.TextField("Source directory : ", sourceDirectory);
        targetDirectory = EditorGUILayout.TextField("Target directory : ", targetDirectory);
        GUILayout.Label("A copy of the content of the source directory will be created inside the target directory with the converted scripts.");

        GUILayout.Space(10);
        GUILayout.Label("Step 1 :");

        if (GUILayout.Button("Do preparation work", GUILayout.MinHeight(50))) 
        {
            DoPreparations();
        }

        GUILayout.Space(10);
        GUILayout.Label("Step 2 : Edit the \"Assets/UnityScriptToCSharp/ItemsAndTypes.txt\" file.");

        GUILayout.Space(10);
        GUILayout.Label("Step 3 :");

        if (GUILayout.Button("Convert", GUILayout.MinHeight(50))) 
        {
            doConvertScripts = true;
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Reset/Abord", GUILayout.MinHeight(50))) 
        {
            Reset();
        }
        

        GUILayout.Space(10);

        GUILayout.Label("State of the conversion : ");
        GUILayout.Label(conversionDescription);
        
        GUILayout.Space(10);

        if (scriptsToConvertList.Count > 0)
            GUILayout.Label(scriptsToConvertList.Count+" scripts left to convert.");
    }


    // ----------------------------------------------------------------------------------
    
    /// <summaray>
    /// Called up to 100 times per second
    /// Perform the conversion, one file per frame
    /// </summary>
    void Update() 
    {
        if (scriptsToConvertList.Count > 0 && doConvertScripts) 
        {
            conversionDescription = scriptsToConvertList.Count+" scripts left to convert.";

            Script scriptInConversion = scriptsToConvertList[0];
            
            // makes sure the target directory exists
            string targetScriptPath = Application.dataPath+targetDirectory+scriptInConversion.relativePath;
            Directory.CreateDirectory(targetScriptPath); // make sure the directory exist, or create it

            if (converter == null)
                converter = new UnityScriptToCSharpConverter(sourceDirectory);
            
            StreamWriter writer = new StreamWriter(targetScriptPath+scriptInConversion.name+".cs");
            writer.Write(converter.Convert(scriptInConversion));
            writer.Flush();
            writer.Close();

            Debug.Log("Converted "+scriptInConversion.relativePath+scriptInConversion.name);

            scriptsToConvertList.RemoveAt(0);
            Repaint(); // update the GUI

            // auto refresh the project once all files have been converted
            if (scriptsToConvertList.Count <= 0) 
            {
                Debug.LogWarning("Conversion done ! Refreshing project.");
                conversionDescription = "Conversion done !";
                doConvertScripts = false;
                AssetDatabase.Refresh();
            }
        }
    }


    /// <summary>
    /// fill scriptsToConvertList
    /// fill itemsAndType 
    /// </summary>
    void DoPreparations() 
    {
        conversionDescription = "Preparing conversion...";

        // check source directory
        if ( ! sourceDirectory.StartsWith("/"))
            sourceDirectory = "/"+sourceDirectory;

        if ( ! Directory.Exists(Application.dataPath+sourceDirectory)) 
        {
            Debug.LogError("UnityScript to C# converter : Abording convertion because the source directory ["+sourceDirectory+"] does not exists.");
            return;
        }

        // fill scriptsToConvertList
        string[] paths = Directory.GetFiles(Application.dataPath+sourceDirectory, "*.js", SearchOption.AllDirectories); // only US scripts in the whole hyerarchie of the source directory

        foreach (string path in paths) 
        {
            StreamReader reader = new StreamReader(path);
            string text = reader.ReadToEnd();
            reader.Close();

            string relativePath = path.Replace(Application.dataPath+sourceDirectory, ""); // just keep the relative path from the source directory
            scriptsToConvertList.Add(new Script(relativePath, text));
        }
        
        // check target directory
        if ( ! targetDirectory.StartsWith("/"))
            targetDirectory = "/"+targetDirectory;
        

        // read all the files again and look for variable declaration without type 
        // where the value is another variable that is not declared somewhere in the project
        // and add the value to the itemsAndTypes.txt file
        // the script can't resolve itself the type of these variable, unless the user associate an item with a type in this file
        
        StreamWriter writer = new StreamWriter(Application.dataPath+"/UnityScriptToCSharpConverter/Resources/ItemsAndTypes.txt", true);
        //writer.WriteLine ("# all keys below are values that need to be associated with a type.");
        List<string> addedValues = new List<string>();

        // re write from RegexUtilities
        string collections = "(ArrayList|BitArray|CaseInsensitiveComparer|Comparer|Hashtable|Queue|SortedList|Stack|StructuralComparisons|DictionnaryEntry"+
        "|ICollection|IComparer|IDictionary|IDictionaryEnumerator|IEnumerable|IEnumerator|IEqualityComparer|IHashCodeProvider|IList|IStructuralComparable|IStructuralEquatable)";
        string genericCollections = "(Comparer|Dictionary|KeyValuePair|HashSet|KeyedByTypeCollection|LinkedList|LinkedListNode|List|Queue|SortedDictionary"+
        "|SortedList|SortedSet|Stack|SynchronizedCollection|SynchronizedKeyedCollection|SynchronizedReadOnlyCollection|"+
        "ICollection|IComparer|IDictionary|IEnumerable|IEnumerator|IEqualityComparer|IList|IReadOnlyCollection|IReadOnlyDictionary|IReadOnlyList|ISet|"+
        "Action|Func)";

        foreach (Script script in scriptsToConvertList)
        {
            // search for variable declaration pattern
            // pattern = "\\bvar"+oblWS+commonName+optWS+"="+optWS+"(?<value>"+commonName+")("+optWS+"\\(.*\\))?"+optWS+";";
            // can't use RegexUtilities here :
            string pattern = "\\bvar(\\s|\\n)+([A-Za-z_]{1}[A-Za-z0-9_\\.]*)(\\s|\\n)*=(\\s|\\n)*(?<value>([A-Za-z_]{1}[A-Za-z0-9_\\.]*))(.*)(\\s|\\n)*;";
            MatchCollection allVars = Regex.Matches(script.text, pattern);

            foreach (Match var in allVars) 
            {
                string value = var.Groups["value"].Value;

                // value can't be a char, a string, a numeric value 
                // nor a boolean > Why ?
                // it must then be a variable name, a boolean, a class instanciation or function call (parenthesis allowed by (.*))

                // discard other know values
                if (addedValues.Contains(value) ||
                    value.Contains(".") || // why does I allow the dot in the regex then ?
                    collections.Contains(value) ||
                    genericCollections.Contains(value) ||
                    value == "true" || value == "false"
                )
                    continue;
                else
                {
                    addedValues.Add(value);
                    writer.WriteLine(value+"=");
                }
            }


            // search for variable return pattern
            //pattern = "\\breturn"+oblWS+"(?<value>"+commonName+")("+optWS+"\\(.*\\))?"+optWS+";"; 
            pattern = "\\breturn(\\s|\\n)+(?<value>([A-Za-z_]{1}[A-Za-z0-9_\\.]*))(.*)(\\s|\\n)*;"; 
            MatchCollection allReturns = Regex.Matches(script.text, pattern);

            foreach (Match aReturn in allReturns) 
            {
                string value = aReturn.Groups["value"].Value;

                if (addedValues.Contains(value) || 
                    value.Contains(".") || 
                    collections.Contains(value) || 
                    genericCollections.Contains(value) ||
                    value == "true" || value == "false"
                )
                    continue;
                else 
                {
                    addedValues.Add(value);
                    writer.WriteLine(value+"=");
                }
            }
        } // end foreach(script in scriptList)

        writer.Flush();
        writer.Close();

        conversionDescription = "Preparation done. \n Added "+addedValues.Count+" entries to \"Assets/UnityScriptToCSharpConverter/Resources/ItemsAndTypes.txt\". \n Ready to convert "+scriptsToConvertList.Count+" scripts.";
    } // nd of method DoPreparations()


    /// <summary>
    /// Reset the values of variables to stop the convertion
    /// </summary>
    void Reset() 
    {
        Debug.LogWarning("Abording conversion and refreshing project.");
        doConvertScripts = false;
        scriptsToConvertList.Clear();
        converter = null;
        conversionDescription = "Conversion reseted";
        AssetDatabase.Refresh();
    }
    
} // end of class UnityScriptToCSharpExtension
