
using System.Collections.Generic;
using System.Text.RegularExpressions; 

public class UnityScriptToCSharp_Variables: RegexUtilities 
{
	/// <summary>
    /// Search for and convert variables declarations
    /// </summary>
    public void Variables() 
    {
        // add an f at the end of a float (and double) value 
        patterns.Add("([0-9]+\\.{1}[0-9]+)(f|F){0}");
        replacements.Add("$1f");


        // ARRAYS

            // replace square brackets by curly brackets
            // works for variable declaration and litteral array in foreach loop "foreach(type var in {array})"

            // patterns.Add("(=|in)"+optWS+"\\[(.*)\\]"+optWS+"(;|\\))");
            // replacements.Add("$1$2{$3}$4$5");

            pattern = "(?s)((=|return|\\()"+optWS+")(?<arrayContent>\\[.*\\])(?<end>"+optWS+"(;|\\)))";
            // the (?s) means that the dot represent every character, including new line \n
        
            while (true)
            {
                allMatches = Regex.Matches(convertedCode, pattern); // allMatches is of type MatchColection and is defined in RegexUtilities
                if (allMatches.Count <= 0)
                    break; // get out of the loop when no more pattern is matched
                
                foreach (Match aMatch in allMatches)
                {
                    // The pattern will have matched more than we need
                    // find the matching square bracket
                    // and replace square bracket by curly bracket inside that array content only
                    int openedBrackets = 0;
                    string USarrayContent = "";
                    
                    foreach (char letter in aMatch.Groups["arrayContent"].Value)
                    {
                        if (letter == '[')
                            openedBrackets++;

                        if (letter == ']') {
                            openedBrackets--;

                            if (openedBrackets == 0 ) { // we have reached the final closing bracket
                                USarrayContent += letter.ToString();
                                break;
                            }
                        }

                        USarrayContent += letter.ToString();
                    }
                    
                    string CSarrayContent = USarrayContent.Replace("[", "{").Replace("]", "}");
                    convertedCode = convertedCode.Replace(USarrayContent, CSarrayContent);
                }
            }


            // replace single quotation marks with strings by double quotation marks (between brackets)
            /*int openedQuotes = 0;
            char lastLetter = ' ';
            bool inAString = false;
            string stringContent = "";
            List<string> strings = new List<string>();

            foreach (char letter in convertedCode)
            {
                lastLetter = letter;

                if (inAString)
                    stringContent += letter.ToString();

                if (letter == '\'') 
                {
                    if ( ! inAString)
                    {
                        openedQuotes++;
                        inAString = true;
                        stringContent += letter.ToString();
                        continue;
                    }
                    else if (lastLetter.ToString()+letter.ToString() != "\\'") // if inAString && not an escaped single quotation mark, this is the end of the string
                    {
                        strings.Add(stringContent);
                    }
                }
            }*/
            
            // replace single quotation marks with strings by double quotation marks
            patterns.Add("(?<start>({|,){1}"+optWS+")'{1}(?<char>"+commonCharsWithoutComma+")'{1}(?<end>"+optWS+"(}|,){1})");
            replacements.Add("${start}\"${char}\"${end}");
            // now, as regex doesn't overlap themselves, only half of the argument have been converted
            // I need to run the regex again
            patterns.Add("(?<start>({|,){1}"+optWS+")'{1}(?<char>"+commonCharsWithoutComma+")'{1}(?<end>"+optWS+"(}|,){1})");
            replacements.Add("${start}\"${char}\"${end}");


            // array with type declaration (with or without value setting)
            
            // arrays with type declaration without space    "string[]" instead of "string [ ]"" are already converted because square brackets are among commonChars 
            // Q 30/06 WTF ? when ?
            // A 30/06 > they will be converted below (general case of var declaration with type and value)

            // general case
            patterns.Add("\\bvar"+oblWS+"(?<varName>"+commonName+")"+optWS+":"+optWS+"(?<type>"+commonName+optWS+"\\[[, ]*\\](?<end>"+optWS+"(=|;))");
            replacements.Add("${type} ${varName} ${end}");


            // arrays with value setting but no type declaration. Have to guess the type with the value's look
            // var variable = [bla];  =>  Type[] variable = new Type[] {bla};
            // square brackets have already been converted to curly brackets by now

                // char
                patterns.Add("\\bvar"+oblWS+"("+commonName+optWS+"="+optWS+"{"+optWS+"((\"|'){1}"+commonCharsWithoutComma+"(\"|'){1}"+optWS+"\\["+optWS+"[0-9]+"+optWS+"\\])");
                replacements.Add("char[] $2");

                // replace 'a'[0] and "a"[0] by 'a' in char[] declaration
                //patterns.Add("(?<start>({|,){1}"+optWS+")(\"|'){1}(?<char>.{1})(\"|'){1}"+optWS+"\\["+optWS+"0"+optWS+"\\](?<end>"+optWS+"(}|,){1})");
                //replacements.Add("${start}'${char}'${end}");
                patterns.Add("(\"|'){1}(?<char>.{1})(\"|'){1}"+optWS+"\\["+optWS+"0"+optWS+"\\]");
                replacements.Add("'${char}'");
                // now, as regex doesn't overlap themselves, I need to run the regex again
                patterns.Add("(\"|'){1}(?<char>.{1})(\"|'){1}"+optWS+"\\["+optWS+"0"+optWS+"\\]");
                replacements.Add("'${char}'");

                // string
                patterns.Add("\\bvar"+oblWS+"("+commonName+optWS+"="+optWS+"{"+optWS+"((\"|'){1})");
                replacements.Add("string[] $2");

                // bool
                patterns.Add("\\bvar"+oblWS+"("+commonName+optWS+"="+optWS+"{"+optWS+"((true|false))");
                replacements.Add("bool[] $2");

                // int
                patterns.Add("\\bvar"+oblWS+"("+commonName+optWS+"="+optWS+"{"+optWS+"-?[0-9]+)");
                replacements.Add("int[] $2");

                // float
                patterns.Add("\\bvar"+oblWS+"("+commonName+optWS+"="+optWS+"{"+optWS+"-?[0-9]+\\.{1}[0-9]+(f|F){1})");
                replacements.Add("float[] $2");                                       
 

            // empty arrays declarations without type declaration  
            // var variable = new Type[num];  =>  Type[] variable = new Type[num];
            
                // string
                patterns.Add("\\bvar"+oblWS+"(?<varName>"+commonName+optWS+"="+optWS+"new"+oblWS+")String(?<end>"+optWS+"\\["+optWS+"[0-9]+"+optWS+"\\]"+optWS+";)");
                replacements.Add("string[] ${varName}string${end}");

                // replace leftover ": string[" or "new string["
                // patterns.Add("(new|:)"+optWS+"string("+optWS+"\\[)");
                // replacements.Add("$1$2string$3");

                // bool
                patterns.Add("\\bvar"+oblWS+"("+commonName+optWS+"="+optWS+"new"+oblWS+")boolean(?<end>"+optWS+"\\["+optWS+"[0-9]+"+optWS+"\\]"+optWS+";)");
                replacements.Add("bool[] $2bool${end}");
                
                // general case
                patterns.Add("\\bvar"+oblWS+"("+commonName+optWS+"="+optWS+"new"+oblWS+"(?<type>"+commonName+")"+optWS+"\\["+optWS+"[0-9]+"+optWS+"\\]"+optWS+";)");
                replacements.Add("${type}[] $2");

                // Q 30/06 Searching for "var name = new type[" sould be enought ?

        // /ARRAYs

        // variable with type declaration but no value setting 
        // var variable: type; > type variable;

            // string
            patterns.Add("\\bvar"+oblWS+commonName+optWS+":"+optWS+"String"+optWS+";");
            replacements.Add("string $2;");

            // others cases are actually handled below :

        // variables with type declaration and value setting 
        // also works without value setting and in foreach loops
        // var variable: type = value;  >  type variable = value;

            // string    put string lowercase and replace "' by ""
            patterns.Add("\\bvar"+oblWS+"(?<name>"+commonName+")"+optWS+":"+optWS+"String"+optWS+"="+optWS+"(\"|'){1}(?<text>.*)(\"|'){1}"+optWS+";");
            replacements.Add("string ${name} = \"${text}\";");

            // char   char _char4 = 'a'[0];    char _char5 = "a';    => char _char4 = 'a";
            patterns.Add("\\bvar"+oblWS+"(?<name>"+commonName+")"+optWS+":"+optWS+"char"+optWS+"="+optWS+"(\"|'){1}(?<text>.*)(\"|'){1}"+optWS+"\\["+optWS+"[0-9]+"+optWS+"\\]"+optWS+";");
            replacements.Add("char ${name} = '${text}';");
            // FIXME will cause eeror if the text is more than one character long

            // bool
            patterns.Add("\\bvar"+oblWS+"(?<name>"+commonName+")"+optWS+":"+optWS+"boolean"+optWS+"(?<end>=|;|in)");
            replacements.Add("bool ${name} ${end}");

            // particular case : float
			patterns.Add("\\bvar"+oblWS+commonName+optWS+":"+optWS+"float"+optWS+"="+optWS+"(-?[0-9]+\\.[0-9]+(f|F){1})"+optWS+";");
			replacements.Add("float $2$5=$6$7$8;");

            // general case 
            patterns.Add("\\bvar"+oblWS+"(?<name>"+commonName+")"+optWS+":"+optWS+"(?<type>"+commonChars+")(?<end>"+optWS+"(=|;|in))");
            replacements.Add("${type} ${name}${end}");
            // This would Also works for some arrays when there is no space around and between square brackets

			// remove the f at the end of value when it's a double
			patterns.Add ("(double"+oblWS+commonName+optWS+"="+optWS+"-?[0-9]+\\.[0-9]+)(f|F){1}");
			replacements.Add ("$1");


        // variable with value setting but no type declaration. Have to guess the type with the value's look
    
            // string
            patterns.Add("\\bvar"+oblWS+"(?<name>"+commonName+")"+optWS+"="+optWS+"(\"|'){1}(?<text>.*)(\"|'){1}"+optWS+";");
            replacements.Add("string ${name} = \"${text}\";");

            // char   
            patterns.Add("\\bvar"+oblWS+"(?<name>"+commonName+")"+optWS+"="+optWS+"(\"|'){1}(?<text>.*)(\"|'){1}"+optWS+"(\\["+optWS+"[0-9]+"+optWS+"\\])"+optWS+";");
            replacements.Add("char ${name} = '${text}';");

            // bool
            patterns.Add("\\bvar("+oblWS+commonName+optWS+"="+optWS+"(true|false)"+optWS+";)");
            replacements.Add("bool$1");

            // int
            patterns.Add("\\bvar("+oblWS+commonName+optWS+"="+optWS+"-?[0-9]+"+optWS+";)"); // long will be converted to int
            replacements.Add("int$1");

            // float     value already contains an f or F   3.5f  or 1.0f
            patterns.Add("\\bvar("+oblWS+commonName+optWS+"="+optWS+"-?[0-9]+\\.{1}[0-9]+(f|F){1}"+optWS+";)");
            replacements.Add("float$1");
        

        // variable without type declaration or value setting

            // if a variable name begins by "is" there is a chance that's a bool
            patterns.Add("\\bvar("+oblWS+"is"+commonName+optWS+";)");
            replacements.Add("bool$1");

            // if a variable name ends by "Rect" there is a chance that's a Rect
            patterns.Add("\\bvar("+oblWS+commonName+"Rect"+optWS+";)");
            replacements.Add("Rect$1");

            // one thing I could do is guessing the type the first time the value is set


        // other types (classes instantiation)    Type variable = new Type();
        // "new" keywords are already added everywhere they are needed by the method "UnityScriptToCSharp_Classes.AddNewKeyword()"
        patterns.Add("\\bvar"+oblWS+"("+commonName+optWS+"="+optWS+"new"+oblWS+commonChars+optWS+"\\((.*);)");
        replacements.Add("$7 $2");

		
        // var declaration vithout a type and the value comes from a function :
            // The type can be resolved if the function exists in itemsAndTypes (see below) or if the function declaration is done in the script, 
            // As UnityScript allows not to specify which type returns a function, wait until the functions declarations are processed (in UnityScriptToCSharp_Functions.Functions()) to try to convert those variables

        // meanwhile, check for values and function calls that are within itemsAndTypes
        // foreach (KeyValuePair<string, string> item in itemsAndTypes) {
        //     if (convertedCode.Contains (item.Key)) { // it just reduce the number of elements in patterns and replacements lists
                
        //         // if the item is a method
        //         patterns.Add("\\bvar("+oblWS+commonName+optWS+"="+optWS+item.Key+optWS+"(\\(.*\\)"+optWS+")?;)");
        //         replacements.Add ( item.Value+"$1");
                
        //         // if the item is a variable
        //         //patterns.Add("\\bvar("+oblWS+commonName+optWS+"="+optWS+item.Key+optWS+";)");
        //         //replacements.Add ( item.Value+"$1");
        //     }
        // }
        // about the same code is run again in Function()
        

        //casting while using the Instanciate method
        // Type variable = Instanciate(object);   >   Type variable = Instanciate(object) as Type;
        patterns.Add("("+"(?<type>"+commonName+")"+oblWS+commonName+optWS+"="+optWS+"Instantiate"+optWS+"\\(.*)(?<end>;)");
        replacements.Add("$1 as ${type}${end}");

        
        // patching time
        // sometimes float value gets 2 f ??????
        patterns.Add ("([0-9]+\\.{1}[0-9]+)(f|F){2,}");
        replacements.Add ("$1f");


        DoReplacements();
    } // end Variables()


    /// <summary>
    /// Convert Properties declarations
    /// </summary>
    public void Properties() 
    {
        // change string return type to string, works also for functions (properties in JS are functions)
        patterns.Add("(\\)"+optWS+":"+optWS+")String");
        replacements.Add("$1string");

        // change bool return type to bool
        patterns.Add("(\\)"+optWS+":"+optWS+")boolean");
        replacements.Add("$1bool");

        DoReplacements();


        // first, get all property getters (if a property exists, I assume that a getter always exists for it)
        pattern = "(?<visibility>public|private|protected)"+oblWS+"function"+oblWS+"get"+oblWS+"(?<propName>"+commonName+")"+optWS+"\\("+optWS+"\\)"+optWS+":"+optWS+"(?<returnType>"+commonChars+")"+optWS+
        "{.*return"+oblWS+"(?<varName>"+commonName+")"+optWS+";"+optWS+"}";
        // need to add (s)   to make the dot count for line ending
        List<Match> allGetters = ReverseMatches (convertedCode, pattern);
        
        string unModifiedScript = convertedCode;

        foreach (Match aGetter in allGetters) 
        {
            string propName = aGetter.Groups["propName"].Value;
            string variableName = aGetter.Groups["bvarName"].Value;

            // now I have all infos nedded to start building the property in C#
            // match.Groups["visibility"].Value is the getter's visibility. It always exists by now because it has been added by UnityScriptToCSharp_Classes.AddVisibility()
            // match.Groups["returnType"].Value is the getter's return type
            string property = aGetter.Groups["visibility"].Value+" "+aGetter.Groups["returnType"].Value+" "+propName+" {"+EOL 
            +"\t\tget { return "+variableName+"; }"+EOL; // getter (closing bracket missing on purpose)

            // now look for the corresponding setter
            pattern = "(public|private|protected)"+oblWS+"function"+oblWS+"set"+oblWS+propName+".*}";
            Match theSetter = Regex.Match(unModifiedScript, pattern);

            if (theSetter.Success)
                property += "\t\t"+theSetter.Groups[1].Value+" set { "+variableName+" = value; }"+EOL; // setter

            property +="\t}"+EOL; // property closing bracket

            // now do the modifs in the script
            convertedCode = convertedCode.Replace(aGetter.Value, property); // replace getter by property

            if (theSetter.Success)
                convertedCode = convertedCode.Replace(theSetter.Value, ""); // remove setter if it existed
        }
    } // end Properties ()



    // ----------------------------------------------------------------------------------

    /// <summary> 
    /// Now that all functions have a return type, try to convert the few variables whose value are set from a function
    /// </summary>
    public void VariablesTheReturn () 
    {
        pattern = "\\bvar"+oblWS+"("+commonName+optWS+"="+optWS+"(?<functionName>"+commonName+")"+optWS+"\\()";
        List<Match> allVariableDeclarations = ReverseMatches(convertedCode, pattern);

        foreach (Match aVarDeclaration in allVariableDeclarations) 
        {
            // look for the function declaration that match the function name
            pattern = "(?<returnType>"+commonChars+")"+oblWS+aVarDeclaration.Groups["functionName"].Value+optWS+"\\(";
            Match theFunction = Regex.Match(convertedCode, pattern); 
            // Q 30/06 Quid if the same function name return sevral types of values ??

            if (theFunction.Success) {
                string newVarDeclaration = aVarDeclaration.Value.Replace("var ", theFunction.Groups["returnType"].Value+" ");
                convertedCode = convertedCode.Replace(aVarDeclaration.Value, newVarDeclaration);
            }
        }
    }
} // end class UnityScriptToCSharp_Variables
