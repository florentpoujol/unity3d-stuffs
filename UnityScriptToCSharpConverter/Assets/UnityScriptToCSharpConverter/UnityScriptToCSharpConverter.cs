
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions; 
using System.IO;


public class UnityScriptToCSharpConverter : RegexUtilities
{
    // list of the projct's classes. Filled in the constructor and used in Classes()
    public static List<string> projectClasses = new List<string>();

    // list of the Unity API classes, extracted from the file "CSharpToUnityScript/Editor/UnityClasses.txt"
    public static List<string> unityClasses = new List<string>();

    // list of data types, including  the built-in C# data types, the Unity classes and the project classes
    public static string dataTypes = ""; // "regex list" "(data1|data2|data3|...)"


    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="scriptsToConvertList">The collection of script sctruture</param>
    /// <param name="sourceDirectory">The directory where to look for files</param>
    public UnityScriptToCSharpConverter(string sourceDirectory)
    {
        unityClasses.Clear();
        projectClasses.Clear();

        // set dataTypes, with regularTypes + collections
        dataTypes = regularTypes.TrimEnd(')')+"|"+collections.TrimStart('(');
        // Q 29/06/13 : why not the generic collections ?


        // reading unity classes, fill unityClasses list
        TextAsset file = (TextAsset)Resources.Load("UnityClasses", typeof(TextAsset));
        if (file != null)
        {
            StringReader reader = new StringReader(file.text);
            string line = "";
            while (true)
            {
                line = reader.ReadLine();
                if (line == null)
                    break;

                unityClasses.Add(line.Trim());
                unityClasses.Add(line.Trim()+"\\[\\]"); // array version
            }
            reader.Close();
        }
        else
            Debug.LogError("UnityScriptToCSharpConverter : The file that contains all Unity classes was not found inside a Resources directory.");

        // adding UnityClasses to dataTypes
        foreach (string _class in unityClasses)
            dataTypes = dataTypes.Replace(")", "|"+_class+")");


        // loop trough all poject's file, extract the data types (classes, enums and structs)
        if (sourceDirectory != "") // allow to skip that part (ie : for the demo)
        {
            string[] paths = Directory.GetFiles(Application.dataPath+sourceDirectory, "*.js", SearchOption.AllDirectories);
            
            foreach (string scriptPath in paths)
            {
                StreamReader sreader = new StreamReader(scriptPath);
                string scriptContent = sreader.ReadToEnd();
                sreader.Close();

                pattern = "\\b(?<type>class|interface|struct|enum)"+oblWS+"(?<name>"+commonName+"\\b)";
                MatchCollection allDataTypes = Regex.Matches(scriptContent, pattern);

                foreach (Match aDataType in allDataTypes)
                {
                    string name = aDataType.Groups["name"].Value;

                    // discard results where the first letter is lowercase
                    // Q 29/06/13 : Why should there be results with the first letter lowercase
                    //              Why discard them ?
                    if (name[0] == char.ToLower(name[0]))
                        continue;

                    dataTypes = dataTypes.Replace(")", "|"+name+")");
                    dataTypes = dataTypes.Replace(")", "|"+name+"\\[\\])"); // array version

                    if (aDataType.Groups["type"].Value == "class")
                        projectClasses.Add(name);
                }
            }
        }


        // foreach (Script script in scriptsToConvertList)
        // {
        //     pattern = "\\b(?<type>class|interface|struct|enum)"+oblWS+"(?<name>"+commonName+"\\b)";
        //     MatchCollection allDataTypes = Regex.Matches(script.text, pattern);

        //     foreach (Match aDataType in allDataTypes)
        //     {
        //         string name = aDataType.Groups["name"].Value;

        //         // discard results where the first letter is lowercase
        //         if (name[0] == char.ToLower(name[0]))
        //             continue;

        //         dataTypes = dataTypes.Replace(")", "|"+name+")");
        //         dataTypes = dataTypes.Replace(")", "|"+name+"\\[\\])"); // array version

        //         if (aDataType.Groups["type"].Value == "class")
        //             projectClasses.Add(name);
        //     }

        //     // always ad the name of the file as a class 
        //     // because in UnityScript, the file may not contain the explicit class declaration
        //     if ( ! projectClasses.Contains(script.name))
        //         projectClasses.Add(script.name);
        // }


        /*
        Q 29/06/13 Why are these below commented ? > beccause the classesList list is not used anymore (but is replaced by projectClasses)
                    Why don't do this anymore? :

        // always ad the name of the file as a class 
        //     // because in UnityScript, the file may not contain the explicit class declaration
        //     if ( ! projectClasses.Contains(script.name))
        //         projectClasses.Add(script.name);
    

        
        // do the same but for the C# scripts in the project folder
        paths = Directory.GetFiles (Application.dataPath+sourceDirectory.TrimEnd ('/'), "*.cs", SearchOption.AllDirectories);
        foreach (string path in paths) {
            reader = new StreamReader (path);
            string text = reader.ReadToEnd();
            reader.Close();

            pattern = "class"+oblWS+commonName+"("+optWS+":"+optWS+commonName+")?"+optWS+"{";
            MatchCollection allClassesDeclarations = Regex.Matches (text, pattern);

            foreach (Match aClassDeclaration in allClassesDeclarations) {
                string className = aClassDeclaration.Groups[2].Value;

                if ( ! classesList.Contains (className))
                    classesList.Add(className);
            }
        }

        // do the same but for the Boo scripts in the project folder
        paths = Directory.GetFiles (Application.dataPath+sourceDirectory.TrimEnd ('/'), "*.boo", SearchOption.AllDirectories);
        foreach (string path in paths) {
            reader = new StreamReader (path);
            string text = reader.ReadToEnd ();
            reader.Close ();

            pattern = "class"+oblWS+commonName+"("+optWS+"\\("+optWS+commonName+optWS+"\\))?";
            MatchCollection allClassesDeclarations = Regex.Matches (text, pattern);

            foreach (Match aClassDeclaration in allClassesDeclarations) {
                string className = aClassDeclaration.Groups[2].Value;

                if ( ! classesList.Contains (className))
                    classesList.Add(className);
            }
        }
        */

        // append the content of MyClasses.txt to classesList
        // Q 29/06 : what MyClasses.txt is supposed to be ?
        /*_path = Application.dataPath+"/UnityScriptToCSharp/MyClasses.txt";
        if (File.Exists(_path)) {
            reader = new StreamReader (_path);

            while (true) 
            {
                string className = reader.ReadLine();
                if (className == null)
                    break;

                if (className.Trim() == "" || className.StartsWith("#")) // an epty line or a comment
                    continue;

                if ( ! classesList.Contains(className))
                    classesList.Add(className);
            }

            reader.Close ();
        }*/
    } // end of constructor

    // constructor without sourceDirectory (ie: for the demo)
    public UnityScriptToCSharpConverter(): this("") {}


    //----------------------------------------------------------------------------------

    // key: random string, value: comment being replaced
    Dictionary<string, string> commentStrings = new Dictionary<string, string>(); 

    /// <summary>
    /// Create a random string
    /// Used as key in commentStrings
    /// (comments are stripped for the script and replaced by a random string looking like #comment#a2cf3f4gf88g#/comment#" then replaced back after the conversion is done)
    /// </summary>
    string GetRandomString()
    {
        string randomString = "#comment#";
        string alphabet = "abcdeghijklmnopqrstuvwxyzABCDEGHIJKLMNOPQRSTUVWXYZ0123456789";
        // I removed f and F in alphabet because the F in patterns like [number]F 
        // will be stripped by the conversion of float values
        while (randomString.Length < 29)
        {
            int number = (int)Random.Range(0, alphabet.Length-1);
            randomString += alphabet[number].ToString();
        }
        randomString += "#/comment#";
        return randomString;
    }


    //----------------------------------------------------------------------------------

    /// <summary>
    ///  Main method that perform generic conversion and call the other method for specific conversion
    /// Assume at the beginning that convertedCode is the UnityScript code to be converted
    /// convertedCode
    /// </summary>
    /// <param name="inputCode">The code in C# to be converted in UnityScript</param>
    /// <returns>The converted code in UnityScript</returns>
    public string Convert(Script p_script)
    {
        script = p_script;
        convertedCode = script.text;
        // script and convertedCode are public static, from RegexUtilities


        // GET ITEMSANDTYPES
        // Q 29/06 Why itemsAndTypes does not seems to be used anymore but still exist in the other calsses ?
        TextAsset file = (TextAsset)Resources.Load("ItemsAndTypes", typeof(TextAsset));

        if (file != null) {
            StringReader reader = new StringReader(file.text);
            string line = "";
            while (true) {
                line = reader.ReadLine();
                if (line == null)
                    break;

                if (line.Trim() == "" || line.StartsWith("#") || ! line.Contains("=")) // an empty line, a comment, or a line that does not contains an equal sign (that would cause errors below)
                    continue;

                string[] items = line.Split('='); // item[0] is the item/value    item[1] is the type

                // if ( ! itemsAndTypes.ContainsKey(items[0].Trim())) 
                // {
                //     if (items[1].Trim() == "")
                //         continue;

                //     itemsAndTypes.Add(items[0].Trim(), items[1].Trim());
                // }    
            }

            reader.Close ();
        }
        else
            Debug.LogError("UnityScriptToCSharpConverter.Convert() : The file ItemsAndTypes.txt was not found inside a Resources directory.");
        

        // GET RID OF COMMENTS
        commentStrings.Clear();

        // block comment
        // find all block comments (allow nested comments)
        int openedCommentBlocks = 0;
        List<string> commentBlocks = new List<string>();
        int commentBlockIndex = -1;
        bool inACommentBlock = false;

        for (int i = 0; i < convertedCode.Length; i++)
        {
            if (convertedCode[i] == '/' && convertedCode[i+1] == '*' && convertedCode[i+2] != '/') 
            {
                if (!inACommentBlock) 
                {
                    inACommentBlock = true;
                    commentBlocks.Add("");
                    commentBlockIndex++; 
                }

                openedCommentBlocks++;
            }

            if (convertedCode[i] == '*' && convertedCode[i+1] == '/') 
            {
                openedCommentBlocks--;

                if (openedCommentBlocks == 0) 
                {
                    inACommentBlock = false;
                    commentBlocks[commentBlockIndex] += "*/";
                }
            }

            if (inACommentBlock)
                commentBlocks[commentBlockIndex] += convertedCode[i].ToString();
        }

        foreach (string commentBlock in commentBlocks) 
        {
            string randomString = GetRandomString();
            while (commentStrings.ContainsKey(randomString))
                randomString = GetRandomString();

            convertedCode = convertedCode.Replace(commentBlock, randomString);
            commentStrings.Add(randomString, commentBlock);
        }

        // single line comments
        // LINE ENDING
        // windows : \r\n
        // Linux :   \n
        // mac :     \r
        string[] lines = convertedCode.Split('\n');

        if (lines.Length == 1) // file has Mac line ending
            lines = convertedCode.Split('\r');
        
        foreach (string line in lines)
        {
            pattern = "//.*$";
            Match comment = Regex.Match(line, pattern);

            if (comment.Success)
            {
                if (comment.Value.Trim() == "//" || comment.Value.Trim() == "///") // commented line does not have any character
                    continue; // continue because it would convert every // in the file and mess up with the following comments

                string randomString = GetRandomString();
                while (commentStrings.ContainsKey(randomString))
                    randomString = GetRandomString();

                convertedCode = convertedCode.Replace(comment.Value, randomString);
                commentStrings.Add(randomString, comment.Value);
            }
            else
            {
                pattern = "#(region|REGION|define|DEFINE).*$";
                Match match = Regex.Match(line, pattern);

                if (match.Success)
                    convertedCode = convertedCode.Replace(match.Value, "");
            }
        }


        // ARRAY() ARRAYLIST()
        // Replace Array() (Array form Unity that works only in UnityScript) by ArrayList()
        patterns.Add("\\bArray"+optWS+"\\(");
        replacements.Add("ArrayList$1(");
        
        patterns.Add(":"+optWS+"Array"+optWS+"(=|{|,|\\))");
        replacements.Add(":$1ArrayList$2$3");

        //Array gets converted to ArrayList, so convert .length to .Count
        pattern = "\\bvar"+oblWS+"(?<varName>"+commonName+")"+optWS+":"+optWS+"Array"+optWS+"=";
        MatchCollection allVariables = Regex.Matches(convertedCode, pattern);

        foreach (Match variable in allVariables) 
        {
            string variableName = variable.Groups["varName"].Value;
            patterns.Add("("+variableName+optWS+"\\."+optWS+")(l|L)ength");
            replacements.Add("$1Count");
        }


        // GENERIC COLLECTIONS
        // Remove the dot before the chevron with generic things (Collection.<Type> GetComponent.<T>()) 
        patterns.Add("("+genericCollections+optWS+")\\.("+optWS+"<)");
        replacements.Add("$1$2");

        // remove whitespaces after a chevron < and before a>
        // Q 29/06 Why ?
        // patterns.Add(genericCollections+optWS+"<"+optWS+commonChars );
        // replacements.Add("$1<$4");
        // patterns.Add(commonChars+optWS+">");
        // replacements.Add("$1>");

        // remove space after the coma
        // Q 29/06 Why ?
        // patterns.Add("<"+optWS+commonChars+optWS+","+optWS+commonChars+optWS+">");
        // replacements.Add("<$2,$5>");

        // Remove the obligatory space between two chevron >     Dictionary<string,List<string>>
        // Q 29/06 Is it really necessary ?
        patterns.Add(">"+optWS+">");
        replacements.Add(">>");
        patterns.Add(">"+optWS+">");
        replacements.Add(">>");


        // LOOPS
        // for > foreach
        patterns.Add("for("+optWS+"\\(.+"+oblWS+"in"+oblWS+".+\\)"+optWS+"{)");
        replacements.Add("foreach$1");

        // foreach(name: Type in array) > foreach(Type name in array)
        patterns.Add("foreach"+optWS+"\\("+optWS+"var"+oblWS+"(?<name>"+commonName+")"+optWS+":"+optWS+"(?<type>"+commonChars+")(?<rest>"+optWS+"in(.+)\\))");
        replacements.Add("foreach$1(${type} ${name}${rest}");


        // GETCOMPONENT (& Co)
        // Getcomponent("T") GetComponent(T)  => Getcomponent<T>()
        // GetComponent.<T>() will have been modified to GetComponent<T>() by now

        string componentMethods = "(?<methodName>AddComponent|GetComponent|GetComponents|GetComponentInChildren|GetComponentsInChildren)";

        // GetComponent("ComponentName") GetComponent(ComponentType)
        patterns.Add("\\b"+componentMethods+optWS+"\\("+optWS+"([\"']{1})?(?<componentName>"+commonName+")([\"']{1})?"+optWS+"\\)");
        replacements.Add("${methodName}<${componentName}>()");

        // convert var declaration
        // first, the methods that returns arrays
        patterns.Add("var"+oblWS+"(?<varName>"+commonName+")(?<declaration>"+optWS+"="+optWS+"(GetComponents|GetComponentsInChildren)"+optWS+"<"+optWS+"(?<componentName>"+commonChars+")"+optWS+">)");
        replacements.Add("${componentName}[] ${varName}${declaration}");
        
        patterns.Add("var"+oblWS+"(?<varName>"+commonName+")(?<declaration>"+optWS+"="+optWS+"(GetComponent|GetComponentInChildren)"+optWS+"<"+optWS+"(?<componentName>"+commonChars+")"+optWS+">)");
        replacements.Add("${componentName} ${varName}${declaration}");

        
        // FINDOBJECTOFTYPE
        // FindObjectOfType(Type)  =>  (Type)FindObjectOfType(typeof(Type))
        patterns.Add("FindObjectOfType"+optWS+"\\("+optWS+"(?<type>"+commonName+")"+optWS+"\\)");
        replacements.Add("(${type})FindObjectOfType(typeof(${type}))");


        // YIELDS
        // yield; > yield return 0;
        patterns.Add("yield"+optWS+";");
        replacements.Add("yield return 0;");
        // yield stuff; > yield return stuff;
        patterns.Add( "yield("+oblWS+commonChars+optWS+";)");
        replacements.Add("yield return $3;");
        // yield Function(  > yield return new Function(;
        patterns.Add("yield"+oblWS+"("+commonChars+optWS+"\\()");
        replacements.Add("yield return new $2");
        // FIXME might be an error there
        // yield return new WaitForSeconds()  but  yield return new StartCoroutine()


        // #pragma
        patterns.Add("\\#pragma"+oblWS+"(strict|implicit|downcast)");
        replacements.Add("");


        // replace .length by .Length
        patterns.Add("\\."+optWS+"length");
        replacements.Add(".Length");

        DoReplacements();


        // CLASSES 
        // Convert stuffs related to classes : declaration, inheritance, parent constructor call
        // Add the "new" keyword before classes instanciation where it is missing
        // add the keyword public when no visibility (or just static) is set (the default visibility in JS is public but private in C#)
        // works also for functions
        UnityScriptToCSharp_Classes classesConverter = new UnityScriptToCSharp_Classes();
        classesConverter.Convert();

        // convert variables declarations
        // it will always resolve the variable type unless when the value is returned from a function (see VariablesTheReturn() void below)
        UnityScriptToCSharp_Variables variablesConverter = new UnityScriptToCSharp_Variables();
        variablesConverter.Variables();

        // convert properties declarations
        variablesConverter.Properties();

        // convert void declarations, including arguments declaration
        UnityScriptToCSharp_Functions functionsConverter = new UnityScriptToCSharp_Functions();
        functionsConverter.Functions ();
   
        // convert variable declaration where the value is returned from a void now that almost all functions got their returned type resolved
        variablesConverter.VariablesTheReturn();
        // functionSec
        functionsConverter.FunctionsTheReturn();


        // ASSEMBLY IMPORTS
        // can't do that in Classes() because it wouldn"t take into account the IEnumerator return type for the coroutine that may be added by Function()

        // move and convert the existing imports  (move because Classes() will have added the class declaration at the top of the files)
        // pattern = "import"+oblWS+commonChars+";"; 
        // MatchCollection matches = Regex.Matches(convertedCode, pattern);

        // foreach (Match match in matches) {
        //     script.text = script.text.Insert (0, "using "+match.Groups[2].Value+";"+EOL).Replace (match.Value, "");
        // }
        patterns.Add("\\bimport("+oblWS+commonNameWithSpace+";)");
        replacements.Add("using$1");


        // add some using instructions based on terms found in the script
        // check if the using already exists then if not, check if we need to add it

        // Generic Collections
        pattern = "\\busing"+oblWS+"System"+optWS+"\\."+optWS+"Collections"+optWS+"\\."+optWS+"Generic"+optWS+";";
        if ( ! Regex.Match(convertedCode, pattern).Success && Regex.Match(convertedCode, genericCollections).Success )
            convertedCode = convertedCode.Insert(0, "using System.Collections.Generic;"+EOL);

        // Collections
        pattern = "\\busing"+oblWS+"System"+optWS+"\\."+optWS+"Collections"+optWS+";";
        if ( ! Regex.Match(convertedCode, pattern).Success && Regex.Match(convertedCode, collections).Success )
            convertedCode = convertedCode.Insert(0, "using System.Collections;"+EOL);
    
        // UnityEngine
        pattern = "\\busing"+oblWS+"UnityEngine"+optWS+";";
        if ( ! Regex.Match(convertedCode, pattern).Success)
           convertedCode = convertedCode.Insert(0, "using UnityEngine;"+EOL);


        // TYPEOF()   add typeof() where needed
        // GameObject("name", Type) > new GameObject("name", typeof(Type))
        patterns.Add("(new"+oblWS+"GameObject"+optWS+"\\(.*,"+optWS+")(?<type>"+commonName+")(?<end>"+optWS+"\\))");
        replacements.Add("$1typeof(${type})${end}");
        // Q 29/06 Does the "new" keyword is mandatory here (in US) ?
        //          There is probably plenty of other places that needs typeof()


        //----------------------------------------------------------------------------------

        // we near the end of the convertion, it's time for patching things up 

        // convert leftover String and Boolean
        patterns.Add("((public|private|protected)"+oblWS+")String("+optWS+"\\["+optWS+"\\])?");
        replacements.Add("$1string$3");

        patterns.Add("((public|private|protected)"+oblWS+")boolean("+optWS+"\\["+optWS+"\\])?");
        replacements.Add("$1bool$3");

        // also in generic collections
        patterns.Add("((,|<)"+optWS+")?String("+optWS+"(,|>))?"); // whitespaces should have been removed, but just in case...
        replacements.Add("$2string$6");

        patterns.Add("((,|<)"+optWS+")?boolean("+optWS+"(,|>))?"); // whitespaces should have been removed, but just in case...
        replacements.Add("$2bool$6");

        // System.String got replaced by System.string
        patterns.Add("(System"+optWS+"."+optWS+")string");
        replacements.Add("$1String");

        // ToString got replaced by Tostring
        patterns.Add("(\\."+optWS+")Tostring("+optWS+"\\()");
        replacements.Add("$1ToString$3");


        // single quotation have to be replaced by double quotation marks 
        // but I can't just do :
        // patterns.Add("'(.{2,})'");
        // replacements.Add("\"$1\"");
        // bacause it would cause to many artifacts
        
        // replace simple quotation mark by double quotation mark in when returning a litteral string
        patterns.Add("return"+oblWS+"'(.{2,})'"+optWS+";");
        replacements.Add("return$1\"$2\"$3;");

        // return char
        //patterns.Add("return"+oblWS+"(\"|'){1}(.+)(\"|'){1}"+optWS+"\\["+optWS+"[0-9]+"+optWS+"\\]"+optWS+";");
        //replacements.Add("return$1'$2'$3;");
        
        // bugs/artefacts
        //patterns.Add("((public|private|protected)"+oblWS+")ring");
        //replacements.Add("$1string");

        DoReplacements();

        // REPLACING COMMENTS
        foreach (KeyValuePair<string, string> comment in commentStrings) {
            //Debug.Log("key="+comment.Key+ " value="+comment.Value);
            convertedCode = convertedCode.Replace(comment.Key, comment.Value);
        }

        return convertedCode;
    } // end of function Convert()
} // end of class UnityScriptToCSharpConverter
