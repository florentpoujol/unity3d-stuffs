
using System.Collections.Generic;
using System.Text.RegularExpressions; 

public class UnityScriptToCSharp_Classes: RegexUtilities
{
    /// <summary> 
    /// Convert stuffs related to classes : declaration, inheritance, parent constructor call, Assembly imports
    /// </summary>
    public void Convert() 
    {        
        // CLASSES DECLARATION
        pattern = "\\bclass"+oblWS+commonName+
        "("+oblWS+"extends"+oblWS+commonName+")?"+
        "("+oblWS+"implements"+oblWS+commonName+"("+optWS+",.+)?)?"+
        optWS+"{";
        List<Match> allClasses = ReverseMatches(convertedCode, pattern);

        // list of functions and variable declaration that are found within a class
        int itemsInClasses = 0;
        
        foreach (Match aClass in allClasses) 
        {
            Block classBlock = new Block(aClass, convertedCode);
            if (classBlock.isEmpty)
                continue;

            // look for constructors in the class
            pattern = "function"+optWS+classBlock.name+optWS+"\\(.*\\)"+optWS+"{";
            List<Match> allConstructors = ReverseMatches(classBlock.text, pattern); // all constructors in that class

            foreach (Match constructor in allConstructors)
            {
                // here, we are inside one of the constructors of the current class (classBlock)
                Block constructorBlock = new Block(constructor, classBlock.text);
                if (constructorBlock.isEmpty)
                    continue;
                // start and end index are relative to classBlock.text, not relative to convertedCode !

                // look for parent constructor call 
                // super(); (in the constrctor's body)  >  public TheClass(arg) : base(args) {
                if (constructorBlock.text.Contains("super"))
                {
                    pattern = "\\bsuper"+optWS+"\\((?<args>.*)\\)"+optWS+";";
                    Match parentConstructorCall = Regex.Match(constructorBlock.text, pattern);

                    if (parentConstructorCall.Success)
                    {
                        // remove super() from the constructor body
                        constructorBlock.newText = constructorBlock.text.Replace(parentConstructorCall.Value, ""); 

                        // add base() to the constructor declaration
                        string call = ": base("+parentConstructorCall.Groups["args"].Value+") {";
                        constructorBlock.newText = constructorBlock.newText.Replace("{", call);
                        // Q 30/06 shouldn't this replace all opening brackets and not just the first one ?
                        //            The first opening brakcets should be at index 0
                        // see also below

                        classBlock.newText = classBlock.newText.Replace(constructorBlock.text, constructorBlock.newText);
                        continue;
                    }
                }

                // look for alternate constructor call for that current class
                // TheClass(); (in the constrctor's body)  >  public TheClass(arg) : this(args) {
                pattern = "\\b"+classBlock.name+optWS+"\\((?<args>.*)\\)"+optWS+";";
                Match altConstructorCall = Regex.Match(constructorBlock.text, pattern);

                if (altConstructorCall.Success) 
                {
                    // remove TheClass() from the constructor body
                    constructorBlock.newText = constructorBlock.text.Replace(altConstructorCall.Value, ""); 

                    // add this() to the constructor declaration
                    string call = ": this("+altConstructorCall.Groups["args"].Value+") {";
                    constructorBlock.newText = constructorBlock.newText.Replace("{", call);

                    classBlock.newText = classBlock.newText.Replace(constructorBlock.text, constructorBlock.newText);
                }
            } // end looping throught constructors in classBlock
            
            // we won't do more search/replace for this class 
            convertedCode = convertedCode.Replace(classBlock.text, classBlock.newText);

            // makes the count of all functions and variables inside the class 
            pattern = "\\bfunction"+oblWS+commonName+optWS+"\\(";
            itemsInClasses += Regex.Matches(classBlock.text, pattern).Count;

            pattern = "\\bvar"+oblWS+commonName+optWS+"(:|;|=)";
            itemsInClasses += Regex.Matches(classBlock.text, pattern).Count;
        } // end looping through classes in that file


        // we made a list of functions and variables inside all classes
        // now make a list of functions and variable inside the script ...
        int itemsInScript = 0;

        pattern = "\\bfunction"+oblWS+commonName+optWS+"\\(";
        itemsInScript += Regex.Matches(convertedCode, pattern).Count;

        pattern = "\\bvar"+oblWS+commonName+optWS+"(:|;|=)";
        itemsInScript += Regex.Matches(convertedCode, pattern).Count;

        // ... then compare the two lists
        // if there is a difference, that mean that some variables or function declarations lies outside a class
        // that means that the script is a MonoBehaviour derived class and we need to add the class declaration
        if (itemsInClasses != itemsInScript) 
        { 
            convertedCode = convertedCode.Insert(0, EOL+"public class "+script.name+" : MonoBehaviour {"+EOL);
            convertedCode = convertedCode+EOL+"} // end of class "+script.name; // the closing public class bracket
        }


        // ATTRIBUTES
        // move some of them to the beginning of the convertedCode before converting
        // pattern = "@?"+optWS+"(script"+oblWS+")?RequireComponent"+optWS+"\\("+optWS+commonChars+optWS+"\\)("+optWS+";)?";
        // MatchCollection matches = Regex.Matches(convertedCode, pattern);
        // foreach (Match match in matches)
        //     convertedCode = convertedCode.Replace(match.Value, "").Insert (0, match.Value+EOL);
    
        // pattern = "@?"+optWS+"(script"+oblWS+")?ExecuteInEditMode"+optWS+"(\\("+optWS+"\\))?("+optWS+";)?";
        // matches = Regex.Matches (convertedCode, pattern);
        // foreach (Match match in matches)
        //     convertedCode = convertedCode.Replace (match.Value, "").Insert (0, "[ExecuteInEditMode]"+EOL);

        // pattern = "@?"+optWS+"(script"+oblWS+")?AddComponentMenu(.*\\))("+optWS+";)?";
        // matches = Regex.Matches (convertedCode, pattern);
        // foreach (Match match in matches)
        //     convertedCode = convertedCode.Replace (match.Value, "").Insert (0, match.Value+EOL);


        // patterns.Add("@"+optWS+"script");
        // replacements.Add("@");

        // //patterns.Add("@"+optWS+"script"+optWS+"AddComponentMenu("+optWS+"\\("+optWS+"\""+optWS+commonChars+optWS"\""+optWS+"\\))");
        // //patterns.Add("@"+optWS+"AddComponentMenu(.*\\))("+optWS+";)?");
        // //replacements.Add("[AddComponentMenu$2]");

        // //  => [RequireComponent (typeof(T))]
        // patterns.Add("@?"+optWS+"RequireComponent"+optWS+"\\("+optWS+commonChars+optWS+"\\)("+optWS+";)?");
        // replacements.Add("[RequireComponent$2(typeof($4))]");

        //  => [ExecuteInEditMode]
        // patterns.Add("@"+optWS+"ExecuteInEditMode"+optWS+"\\("+optWS+"\\)("+optWS+";)?");
        // replacements.Add("[ExecuteInEditMode]");
   

        // no script, no params
        patterns.Add("@"+optWS+"(?<attribute>RPC|HideInInspector|Serializable|System.NonSerialized|SerializeField)");
        replacements.Add("[${attribute}]");

        // no script, with params
        patterns.Add("@"+optWS+"(?<attribute>DrawGizmo|Conditional|MenuItem|System.Obsolete)"+optWS+"(?<params>\\([^\\)]*\\))");
        replacements.Add("[${attribute}${params}]");
        
        // require component  need to add typeof()
        // why don't we need to do that with CustomEditor ??
        patterns.Add("@"+optWS+"script"+oblWS+"RequireComponent"+optWS+"\\("+optWS+"(?<type>"+commonName+")"+optWS+"\\)");
        replacements.Add("[RequireComponent(typeof(${type}))]");

        // script + params
        string attributes = "(?<attributes>AddComponentMenu|ContextMenu|ExecuteInEditMode|ImageEffectOpaque|"+
        "ImageEffectTransformsToLDR|NotConvertedAttribute|NotRenamedAttribute|System.Serializable|"+
        "CanEditMultipleObjects|CustomEditor|PostProcessAttribute|PreferenceItem)";
        patterns.Add("@"+optWS+"script"+optWS+attributes+optWS+"(?<params>\\([^\\)]*\\))?");
        replacements.Add("[${attributes}${params}]");


        // STRUCT
        // in JS, the way to define struct is to makes a public class inherits from System.ValueType (or just a regular class)
        patterns.Add("class"+oblWS+"(?<name>"+commonName+")"+oblWS+"extends"+oblWS+"System"+optWS+"\\."+optWS+"ValueType");
        replacements.Add("struct ${name}");


        // CLASS INHERITANCE
        // class TheClass extends Parent { > class TheClass: Parent
        // class TheClass implements Interface, Interace { > class TheClass: Interface, Interace
        patterns.Add("(\\class"+oblWS+commonName+oblWS+")extends(?<end>"+oblWS+commonName+optWS+"({|implements))");
        replacements.Add("$1:${end}");


        // super. => base.      
        patterns.Add("super("+optWS+"\\.)");
        replacements.Add("base$1");

        DoReplacements ();
    } // end Classes()


    /// <summary>
    /// Add the "new" keyword before classes instanciation where it is missing
    /// </summary>
    public void AddNewKeyword() 
    {
        // get pattern like "var name: Type = ClassOrMethod ();"  and search for "Class name = Class ();"
        pattern = "var"+oblWS+commonName+optWS+":"+optWS+"(?<type>"+commonChars+")("+optWS+"="+optWS+"(?<className>"+commonChars+")"+optWS+"\\(.*\\)"+optWS+";)";
        List<Match> allMatches = ReverseMatches(convertedCode, pattern);

        foreach (Match match in allMatches) {
            if (match.Groups["type"].Value == match.Groups["className"].Value) { // if the type == the class/method name
                convertedCode = convertedCode.Insert(match.Groups["className"].Index, "new "); // add "new " in front of Class ()
            }
        }
        // Q 30/06 This will add the new keyword everywhere, even when it is already here !!
        // A 30/06 Nope because it uses Insert and not Replace
        
        //also add a new keyword in front of collections
        pattern = "="+optWS+collections+optWS+"\\("; // when setting the value of a variable
        InsertInPatterns(pattern, 1, " new");

        pattern = "return"+oblWS+collections+optWS+"\\("; // when returning an empty instance
        InsertInPatterns(pattern, 6, " new");
        
        // and Generic collections
        pattern = "="+optWS+genericCollections+"<"+commonChars+">"+optWS+"\\(";
        InsertInPatterns(pattern, 1, " new");

        pattern = "return"+oblWS+genericCollections+optWS+"\\(";
        InsertInPatterns(pattern, 6, " new");


        // and classes in ClassesList
        // foreach (string className in classesList) {
        //     pattern = "="+optWS+className+optWS+"\\(";
        //     InsertInPatterns (pattern, 1, " new");

        //     // do the same with return keyword
        //     pattern = "return"+oblWS+className+optWS+"\\(";
        //     InsertInPatterns (pattern, 6, " new");
        // }
        // Q 30/06 Shouldn't I do the same with projectClasses ?

    } // end AddNewKeyword()


    /// <summary> 
    /// Insert [text] at the fixed position [patternOffset] in all [pattern]s found
    /// </summary>
    private void InsertInPatterns (string pattern, int patternOffset, string text) 
    {
        List<Match> allMatches = ReverseMatches(convertedCode, pattern);
        foreach (Match match in allMatches)
            convertedCode = convertedCode.Insert(match.Index + patternOffset, text);
    }


    /// <summary>
    /// Add the keyword public when no visibility (or just static) is set (the default visibility is public in JS but private in C#)
    /// Works also for functions, classes and enums
    /// </summary>
    public void AddVisibility()
    {
        // the default visibility for variable and functions is public in JS but private in C# => add the keyword public when no visibility (or just static) is set 
        patterns.Add("([;{}\\]]+"+optWS+")((var|function|enum|class)"+oblWS+")");
        replacements.Add("$1public $3");

        patterns.Add("(\\*/"+optWS+")((var|function|enum|class)"+oblWS+")");
        replacements.Add("$1public $3");

        patterns.Add("(//.*"+optWS+")((var|function|enum|class)"+oblWS+")");
        replacements.Add("$1public $3");

        patterns.Add("((\\#else|\\#endif)"+oblWS+")((var|function|enum|class)"+oblWS+")");
        replacements.Add("$1public $4");


        // static
        patterns.Add("([;{}\\]]+"+optWS+")static"+oblWS+"((var|function)"+oblWS+")");
        replacements.Add("$1public static $4");

        patterns.Add("(\\*/"+optWS+")static"+oblWS+"((var|function)"+oblWS+")");
        replacements.Add("$1public static $4");

        patterns.Add("(//.*"+optWS+")static"+oblWS+"((var|function)"+oblWS+")");
        replacements.Add("$1public static $4");

        patterns.Add("((\\#else|\\#endif)"+oblWS+")((var|function)"+oblWS+")");
        replacements.Add("$public static $4");

        DoReplacements ();


        // all variables gets a public or static public visibility but this shouldn't happend inside functions, so remove that

        pattern = "function"+oblWS+commonName+optWS+"\\(.*\\)"+optWS+"(:"+optWS+commonChars+optWS+")?{";
        List<Match> allFunctions = ReverseMatches (convertedCode, pattern);

        foreach (Match aFunction in allFunctions) {
            Block function = new Block (aFunction, convertedCode);
            if (function.isEmpty)
                continue;

            patterns.Add("public"+oblWS+"(static"+oblWS+"var)");
            replacements.Add("$2");
            patterns.Add("(static"+oblWS+")?public"+oblWS+"var");
            replacements.Add("$1var");

            function.newText = DoReplacements(function.text);
            convertedCode = convertedCode.Replace(function.text, function.newText);
        } // end for
    } // end AddVisibility()
} // end of class UnityScriptToCSharp_Classes()
