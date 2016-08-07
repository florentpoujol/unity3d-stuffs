/// <summary>
/// SweetScriptToUnityScript class
///
/// This class handle the conversion from the SweetScript scripting language to the regular UnityScript.
///
/// Used by the SweetSyntax extension for Unity3D.
/// Website : http://www.florent-poujol.fr/en/unity3d/sweetsyntax
/// Documentation : http://www.florent-poujol.fr/assets/unity3d/sweetsyntax/
///
///
/// Version: 1.0 (sole version that was ever released)
/// Release: september 2012
///
/// Created by Florent POUJOL aka Lion on Unity's forums
/// florent.poujol@gmail.com
/// http://www.florent-poujol.fr/en
/// Profile on Unity's forums : http://forum.unity3d.com/members/23148-Lion
/// <summary>


using UnityEngine;
using System; // Int32, Single, Double
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class SweetScriptToUnityScript : RegexUtilities {

	/// <summary>
	/// Constructor and main method
	/// </summary>
	public SweetScriptToUnityScript (string inputCode) {
		convertedCode = inputCode;

		Variables ();
		Properties ();
        Classes ();
        Loops ();
        Conditions ();

        // FUNCTION DECLARATION

		// use "func" keyword instead of "function"
		patterns.Add ("func"+oblWS+commonName);
		replacements.Add ("function$1$2");

		string function = "(function|func)";

		// parenthsesis becomes optionnal if there is no arguments
		patterns.Add ("("+function+oblWS+commonName+")("+optWS+":"+optWS+commonCharsWithSpace+")?("+optWS+"{)");
		replacements.Add ("$1()$5$9");


		// BOOL AND STRING

		// allow to use "bool" instead of "boolean" and "string" instead of "String"
		// works with var declaraion with or without value
			// string      
	        patterns.Add ( "("+commonName+optWS+":"+optWS+")(string|str)(("+optWS+"\\["+optWS+"\\])?"+optWS+"(=|;))" );
	        replacements.Add ( "$1String$6" );

	        // bool
	        patterns.Add ( "("+commonName+optWS+":"+optWS+")bool(("+optWS+"\\["+optWS+"\\])?"+optWS+"(=|;))" );
	        replacements.Add ( "$1boolean$5" );

        // with arguments:
	        // string
	        patterns.Add ( "("+commonName+optWS+":"+optWS+")(string|str)(("+optWS+"\\["+optWS+"\\])?"+optWS+"(,|\\)))" );
	        replacements.Add ( "$1String$6" );

	        // bool
	        patterns.Add ( "("+commonName+optWS+":"+optWS+")bool(("+optWS+"\\["+optWS+"\\])?"+optWS+"(,|\\)))" );
	        replacements.Add ( "$1boolean$5" );

	    // with generic collections
	    	// string
	        patterns.Add ( "((<|,)"+optWS+")(string|str)(("+optWS+"\\["+optWS+"\\])?"+optWS+"(>|,))" );
	        replacements.Add ( "$1String$5" );

	        // bool
	        patterns.Add ( "((<|,)"+optWS+")bool(("+optWS+"\\["+optWS+"\\])?"+optWS+"(>|,))" );
	        replacements.Add ( "$1boolean$4" );

	    // with function
	    	// string
	        patterns.Add ( "(\\)"+optWS+":"+optWS+")(string|str)(("+optWS+"\\["+optWS+"\\])?"+optWS+"{)" );
	        replacements.Add ( "$1String$5" );

	        // bool
	        patterns.Add ( "(\\)"+optWS+":"+optWS+")bool(("+optWS+"\\["+optWS+"\\])?"+optWS+"{)" );
	        replacements.Add ( "$1boolean$4" );

		DoReplacements ();
	}


	// ----------------------------------------------------------------------------------

	/// <summary>
	/// Handle everything related to variable conversion
	/// </summary>
	void Variables () {
		// CHAR DECLARATION

		// if the value of a variable is a single character surounded by single quotation marks
		// this variable becomes of type char (and not String)
		// var name = 'a';		=>		var name: char = 'a'[0];
		patterns.Add ("(var"+oblWS+commonName+optWS+":"+optWS+"char"+optWS+"="+optWS+"'.{1}')("+optWS+";)");
		replacements.Add ("$1[0]$8");

		patterns.Add ("(var"+oblWS+commonName+")("+optWS+"="+optWS+"'.{1}')("+optWS+";)");
		replacements.Add ("$1: char$4[0]$7");
		
		patterns.Add ("('.{1}')("+optWS+"[^\\[]{1})");
		replacements.Add ("$1[0]$2");


		//--------------------

		// NEW OBJECT INSTANCIATION

		// allow to declare a variable with instanciating a new empty instance of a class
		// var name new class    =>  var name: class = new class ();
		patterns.Add ("(var"+oblWS+commonName+")"+oblWS+"new"+oblWS+"(?<varType>"+commonCharsWithSpace+")"+optWS+"(\\((?<params>"+commonCharsWithSpace+")\\)"+optWS+")?;");
		replacements.Add ("$1: ${varType} = new ${varType}(${params});");
		
		DoReplacements ();


		//--------------------

		// MULTIPLE INLINE VARIABLE DECLARATION 

		// (with allowed value setting)
		// var: type name1, name2 = value, name3=value;	=>  var name: type;   var name2: type = value; var name3: type;
		pattern = "(?<visibility>("+visibility+oblWS+")?(static"+oblWS+")?)?var"+optWS+":"+optWS+"(?<type>"+commonChars+")"+
		oblWS+"(?<varList>"+commonName+optWS+"(="+optWS+commonCharsWithoutComma+optWS+")?,{1}"+optWS+".*)+"+optWS+";";
		List<Match> allDeclarations = ReverseMatches (convertedCode, pattern);
		//Debug.Log (allDeclarations.Count);

		foreach (Match aDeclaration in allDeclarations) {
			// split the varlist using the coma
            string[] varList = aDeclaration.Groups["varList"].Value.Split (',');
            string type = aDeclaration.Groups["type"].Value;
            string _visibility = aDeclaration.Groups["visibility"].Value;
            string newSyntax = "";

            foreach (string varName in varList) {
                if (varName.Contains ("=")) {
                    // add the type beetween the varName and the equal sign
                    if (type != "")
                    	newSyntax += _visibility+"var "+varName.Replace ("=", ": "+type+" =").Trim ()+";"+EOL;
                    else
                    	newSyntax += _visibility+"var "+varName.Trim ()+";"+EOL;
                }
                else {
                	if (type != "")
                    	newSyntax += _visibility+"var "+varName.Trim ()+": "+type+";"+EOL;
                    else
                    	newSyntax += _visibility+"var "+varName.Trim ()+";"+EOL;
                }
            }

            convertedCode = convertedCode.Replace (aDeclaration.Value, newSyntax);
		}


		//--------------------
		
		// GENERIC COLLECTIONS

		// add a dot between the name of a generic collection and the opening chevron
		// List<String>   =>   List.<Sring>
        patterns.Add ( genericCollections+optWS+"<" );
        replacements.Add ( "$1$2.<" );

		// Add a whitespace between two closing chevron   Dictionary.<string, List<string> > 
		patterns.Add ( "("+genericCollections+optWS+"\\."+optWS+"<"+commonCharsWithSpace+")>>" );
		replacements.Add ( "$1> >" );

		DoReplacements ();
	}


	// ----------------------------------------------------------------------------------

	/// <summary>
	/// Properties declaration have never been more simple in UnityScript !
	/// </summary>
	void Properties () {
		// with default getter and/or setter

		// prop Name: type; // getter+setter
		// prop Name: type get; // only getter (Read only property)
		// prop Name: type set; // only setter
		pattern ="(prop|property)"+oblWS+"(?<name>"+commonName+")("+optWS+":"+optWS+"(?<type>"+commonChars+"))?("+oblWS+"(?<getOrSet>get|set))?"+optWS+";";	
		List<Match> allProps = ReverseMatches (convertedCode, pattern);
 		 		
 		foreach (Match aProp in allProps) {
			string name = aProp.Groups["name"].Value;
			string type = aProp.Groups["type"].Value;
			string getOrSet = aProp.Groups["getOrSet"].Value;
			string newSyntax = "";

			if (getOrSet == "get") {
				newSyntax = BuildGetter (name, type, "");
			}
			else if (getOrSet == "set") {
				newSyntax = BuildSetter (name, type, "");
			}
			else if (getOrSet == "") {
				newSyntax = BuildGetter (name, type, "");
				newSyntax += BuildSetter (name, type, "");
			}

			convertedCode =  convertedCode.Replace (aProp.Value, newSyntax);
		}


		// with custom getter and/or setter
		// C# like syntax
		pattern ="(prop|property)"+oblWS+"(?<name>"+commonName+")("+optWS+":"+optWS+"(?<type>"+commonChars+"))?("+optWS+"{)";	
		allProps = ReverseMatches (convertedCode, pattern);
 		 		
 		foreach (Match aProp in allProps) {
 			Block propBlock = new Block (aProp, convertedCode);

			string name = aProp.Groups["name"].Value;
			string type = aProp.Groups["type"].Value;
			string newSyntax = "";

			// looking for a getter
			pattern = "get"+optWS+"({|}|set)";
			Match getterMatch = Regex.Match (propBlock.text, pattern);

			if (getterMatch.Success) {
				string end = getterMatch.Groups[2].Value;
				
				if (end == "{") { // a custom getter
					// get the block and use it as the content of the US's getter function
					Block getterBlock = new Block (getterMatch, propBlock.text);
					newSyntax += BuildGetter (name, type, getterBlock.text);
				}
				else if (end == "}" || end == "set") // default getter
					newSyntax += BuildGetter (name, type, "");
			}


			// looking for a setter
			pattern = "set"+optWS+"({|}|get)";
			Match setterMatch = Regex.Match (propBlock.text, pattern);

			if (setterMatch.Success) {
				string end = setterMatch.Groups[2].Value;
				
				if (end == "{") { // a custom setter
					// get the block and use it as the content of the US's getter function
					Block setterBlock = new Block (setterMatch, propBlock.text);
					newSyntax += BuildSetter (name, type, setterBlock.text);
				}
				else if (end == "}" || end == "get") // end of the property block or getter
					newSyntax += BuildSetter (name, type, "");
			}


			// remove the old declaration
			convertedCode =  convertedCode.Replace (aProp.Value.TrimEnd ('{'), ""); 

			// now replace all the block itself by the new syntax
			convertedCode =  convertedCode.Replace (propBlock.text, newSyntax);
		}
	}

	//--------------------

	string GetFirstLetterLowercase (string input) {
		string output = input[0].ToString ().ToLower ();
		output += input.Substring (1);
		return output;
	}

	//--------------------

	string BuildGetter (string name, string type, string blockContent) {
		if (type != "")
			type = ": "+type;

		if (blockContent == "")
			blockContent = "{ return "+GetFirstLetterLowercase (name)+"; }";

		return "function get "+name+"()"+type+" "+blockContent+EOL;
	}


	string BuildSetter (string name, string type, string blockContent) {
		if (type != "")
			type = ": "+type;

		if (blockContent == "")
			blockContent =  "{ "+GetFirstLetterLowercase (name)+" = value; }";

		return "function set "+name+"(value"+type+") "+blockContent+EOL;
	}


	// ----------------------------------------------------------------------------------

	/// <summary>
	/// Handle conversion of classes-related stuffs
	/// </summary>
	void Classes () {
		// STRUCTS

		// strcut keyword just lie in C#
        // in US, the way to define struct is to makes a public class inherits from System.ValueType
        patterns.Add ( "struct"+oblWS+commonName+optWS+"{" );
        replacements.Add ( "class $2 extends System.ValueType {" );

        DoReplacements ();


        //--------------------

		// loop the classes declarations in the file
        pattern = "class"+oblWS+"(?<blockName>"+commonName+")"+
        "("+oblWS+"extends"+oblWS+"(?<parentClassName>"+commonName+"))?"+
        "("+oblWS+"implements"+oblWS+commonName+"("+optWS+","+optWS+commonName+")*)?"+
        optWS+"{";
        List<Match> allClasses = ReverseMatches (convertedCode, pattern);
        
        foreach (Match aClass in allClasses) {
			Block classBlock = new Block (aClass, convertedCode);

			if (classBlock.isEmpty)
				continue;

			
			if (aClass.Groups["parentClassName"].Value != "") { // this class inherits from another one
				patterns.Add ("parent("+optWS+"\\.)");
				replacements.Add ("super$1");
			}


			// parent constructor call with parameters
			patterns.Add ("parent(("+optWS+"\\(.*\\))"+optWS+";)");
			replacements.Add ("super$1");

			// parent constructor call without parameters
			patterns.Add ("parent("+optWS+";)");
			replacements.Add ("super()$1");

			// constructor declaration with parameters
			patterns.Add ("constructor("+optWS+"\\(.*\\)"+optWS+"{)");
			replacements.Add ("public function "+classBlock.name+"$1");

			// constructor declaration without parenthesis
			patterns.Add ("constructor("+optWS+"{)");
			replacements.Add ("public function "+classBlock.name+"()$1");


			// constructor call with parameters
			patterns.Add ("constructor("+optWS+"\\(.*\\)"+optWS+";)");
			replacements.Add (classBlock.name+"$1");

			// constructor call without parenthesis
			patterns.Add ("constructor("+optWS+";)");
			replacements.Add (classBlock.name+"()$1");


			classBlock.newText = DoReplacements (classBlock.text);
			convertedCode = convertedCode.Replace (classBlock.text, classBlock.newText);
		} // end looping through classes in that file
	}


	// ----------------------------------------------------------------------------------

	/// <summary>
	/// Handle for/foreach loops conversion
	/// </summary>
	void Loops () {
		// you can always use "for" or "foreach"

		// generic for(;;) loops

			// foreach Name from [from] to [to] step [step] {}
			// => for (var Name=[from]; Name<=[to] ; Name+=[step])
			// Can also set the type of the variable and default the from to 0 and the step to +1
			// equality sign will always be <=

			pattern = "(for|foreach)("+optWS+"\\("+optWS+"|"+oblWS+")(?<varKeyword>var"+oblWS+")?(?<varName>"+commonName+")(?<varType>"+optWS+":"+optWS+commonChars+")?"+
			"("+oblWS+"from"+oblWS+"(?<from>"+commonName+"))?"+
			"("+oblWS+"to"+oblWS+"(?<to>"+commonName+"))"+
			"("+oblWS+"step"+oblWS+"(?<step>"+commonName+"))?"+
			"("+optWS+"\\))?"+
			"(?<end>"+optWS+"{)?";
			List<Match> allLoops = ReverseMatches (convertedCode, pattern);

			foreach (Match aLoop in allLoops) {
				string varName = aLoop.Groups["varName"].Value;
				string varType = aLoop.Groups["varType"].Value;
				string from = aLoop.Groups["from"].Value;
				string to = aLoop.Groups["to"].Value;
				string step = aLoop.Groups["step"].Value;
				string end = aLoop.Groups["end"].Value;

				if (varType == "") varType = ": int";
				if (from == "") from = "0";
				if (step == "") step = "1";

				string equalityOperator = GetEqualityOperator (varType.TrimStart (':'), from, to);	
				string newLoop = "for (var "+varName+varType+" = "+from+"; "+varName+equalityOperator+to+"; "+varName+"+="+step+")"+end;
				convertedCode = convertedCode.Replace (aLoop.Value, newLoop);
			} 


			// boo-like syntax with range operator introduced in Unity 3.x
			// for (var i: type in range(from,to,step))
			// allow without parenthesis or var keyword
			pattern = "(for|foreach)"+
			"("+optWS+"\\("+optWS+"|("+oblWS+"))"+
			"(?<varKeyword>var"+oblWS+")?(?<varName>"+commonName+")(?<varType>"+optWS+":"+optWS+commonChars+")?"+
			"(?<inRange>"+oblWS+"in"+oblWS+"(range|Range)"+optWS+"\\([0-9\\., ]+\\))"+
			"("+optWS+"\\))?"+
			"(?<end>"+optWS+"{)?";
			allLoops = ReverseMatches (convertedCode, pattern);

			foreach (Match aLoop in allLoops) {
				string varName = aLoop.Groups["varName"].Value;
				string varType = aLoop.Groups["varType"].Value;
				string inRange = aLoop.Groups["inRange"].Value;
				string end = aLoop.Groups["end"].Value;

				if (varType == "") varType = ": int";

				string newLoop = "for (var "+varName+varType+inRange+")"+end;
				convertedCode = convertedCode.Replace (aLoop.Value, newLoop);
			}


		// for(;;) loop with index-based collection or array

			// index will always be an int
			// foreach index Name in array {}
			// => for (var Name: int=0; Name<array.length; Name++) { }
			pattern = "(for|foreach)("+optWS+"\\("+optWS+"|"+oblWS+")"+
			"index"+oblWS+"(?<varName>"+commonName+")(?<value>"+optWS+"="+optWS+commonName+")?"+oblWS+"in"+oblWS+
			"(?<collection>(col|collection)"+oblWS+")?(?<arrayName>"+commonName+")("+optWS+"\\))?(?<end>"+optWS+"{)?";
			allLoops = ReverseMatches (convertedCode, pattern);

			foreach (Match aLoop in allLoops) {
				string varName = aLoop.Groups["varName"].Value;
				string value = aLoop.Groups["value"].Value;
				string arrayName = aLoop.Groups["arrayName"].Value;
				string collection = aLoop.Groups["collection"].Value;
				string end = aLoop.Groups["end"].Value;

				if (value.Trim () == "")
					value = " = 0";

				string newLoop = "for (var "+varName+": int"+value+"; "+varName+"<";

				if (collection == "")
					newLoop += arrayName+".length; "+varName+"++)";
				else
					newLoop += arrayName+".Count; "+varName+"++)";

				newLoop += end;
				convertedCode = convertedCode.Replace (aLoop.Value, newLoop);
			}


		// for(in) loop with array/collections
			
			// for Name in collection {}
			// foreach Name: type in collections {}
			// => for (var name: type in collection)
			pattern = "(for|foreach)("+optWS+"\\("+optWS+"|"+oblWS+")"+
			"(?<varKeyword>var"+oblWS+")?(?<statement>"+commonName+"("+optWS+":"+optWS+commonChars+")?"+
			oblWS+"in"+oblWS+
			"(?<arrayName>"+commonName+"))("+optWS+"\\))?(?<end>"+optWS+"{)?";
	
			allLoops = ReverseMatches (convertedCode, pattern);

			foreach (Match aLoop in allLoops) {
				string arrayName = aLoop.Groups["arrayName"].Value;
				
				if (arrayName != "range" && arrayName != "Range") {
					patterns.Add ("(for|foreach)("+optWS+"\\("+optWS+"|"+oblWS+")"+
					"(?<varKeyword>var"+oblWS+")?(?<statement>"+commonName+"("+optWS+":"+optWS+commonChars+")?"+
					oblWS+"in"+oblWS+
					"(?<arrayName>"+arrayName+"))("+optWS+"\\))?(?<end>"+optWS+"{)?");
					replacements.Add ("for (var ${statement})${end}");
				}

			}

			DoReplacements ();
	} // end of method Loop


	// ----------------------------------------------------------------------------------

	/// <summary>
	/// Resolve the equality operator to use between the from and to parameters, depending on theirs value
	/// </summary>
	string GetEqualityOperator (string varType, string from, string to) {
		string equalityOperator = "<=";

		switch (varType.Trim ()) {
			case "int": 
				int iFrom = 0, iTo;
				if (Int32.TryParse (from, out iFrom) && Int32.TryParse (to, out iTo)) {
					if (iFrom > iTo)
						equalityOperator = ">=";
				}
				break;

			case "float": 
				float fFrom = 0.0f, fTo;
				if (Single.TryParse (from, out fFrom) && Single.TryParse (to, out fTo)) {
					if (fFrom > fTo)
						equalityOperator = ">=";
				}
				break;

			case "double": 
				double dFrom = 0.0, dTo;
				if (Double.TryParse (from, out dFrom) && Double.TryParse (to, out dTo)) {
					if (dFrom > dTo)
						equalityOperator = ">=";
				}
				break;
		}

		return equalityOperator;
	}


	// ----------------------------------------------------------------------------------

	/// <summary>
	/// Condition stuffs : switch, comparison operators
	/// </summary>
	void Conditions () {
		// SWITCH

		pattern = "switch"+optWS+"(?<openingParenthesis>\\()?"+optWS+"(?<varName>"+commonName+")"+optWS+"\\)?"+optWS+"{";
		List<Match> allSwitches = ReverseMatches (convertedCode, pattern);

        foreach (Match aSwitch in allSwitches) {
	    	Block switchBlock = new Block (aSwitch, convertedCode);

	    	string op = aSwitch.Groups["openingParenthesis"].Value;
	    	string varName = aSwitch.Groups["varName"].Value;

	    	switchBlock.newDeclaration = switchBlock.declaration;

	    	if (op.Trim () == "") // no parenthesis set => must add the
	    		switchBlock.newDeclaration = switchBlock.declaration.Replace (varName, "("+varName+")");

	    	convertedCode = convertedCode.Replace (switchBlock.declaration, switchBlock.newDeclaration);

	    	patterns.Add ("if("+oblWS+commonCharsWithSpace+optWS+":)");
	    	replacements.Add ("case$1");

	    	patterns.Add ("else("+optWS+":)");
	    	replacements.Add ("default$1");

	    	patterns.Add (";("+optWS+"(case"+oblWS+"|default"+optWS+"))");
	    	replacements.Add ("; break;$1");

	    	switchBlock.newText = DoReplacements (switchBlock.text);
	    	convertedCode = convertedCode.Replace (switchBlock.text, switchBlock.newText);
	    }


	    //--------------------

		// use "and", "or", "not", "is" and "isnt"
	    // instead of "&&", "||", "!", "==" and "!="
	    // if, while, for
	    //  ternaries don't work yet

	    string[] conditions = {
	    "\\b(if|while)\\b[^{\\n]+({|\\n)",
	    "for"+optWS+"\\([^;]+;[^;]+;.+\\)",
	    //"({|}|;|\\)|:)"+optWS+"[^?]+\\?[^:]+:[^;]+;" // does not work !
		};

		string[] sourceOperators = {"and", "or", "not", "is", "isnot"};
		string[] targetOperators = {"&&",  "||", "!",   "==", "!="};

		foreach (string condPattern in conditions) {
		    List<Match> allConditions = ReverseMatches (convertedCode, condPattern);

	        foreach (Match aCondition in allConditions) {
		    	string newCond = aCondition.Value;

		    	for (int i=0; i<sourceOperators.Length; i++){
		    		patterns.Add ("(\\(|\\)| )"+sourceOperators[i]+"(\\(|\\)| )");
		    		replacements.Add ("$1"+targetOperators[i]+"$2");
		    	}

		    	newCond = DoReplacements (newCond);

		    	convertedCode = convertedCode.Replace (aCondition.Value, newCond);
		    }
		}


		//--------------------

		// (name isoneof value1, value2)
		// (name == value1 || name == value2)
		pattern = "(?<varName>"+commonName+")"+oblWS+"(?<equality>isoneof|isnotoneof)"+oblWS+
		"(?<valueList>"+commonCharsWithSpace+optWS+"(,"+optWS+commonCharsWithSpace+optWS+")+)";
		List<Match> allStatements = ReverseMatches (convertedCode, pattern);

		foreach (Match aCondition in allStatements) {
			// split the varlist using the coma
            string[] valueList = aCondition.Groups["valueList"].Value.Split (',');
            string varName = aCondition.Groups["varName"].Value;
            string equality = aCondition.Groups["equality"].Value;
            string _operator = "";
            string newSyntax = "";

            if (equality == "isoneof") {
            	equality = " == ";
            	_operator = " || ";
            }
            else {
            	equality = " != ";
            	_operator = " && ";
            }

            foreach (string value in valueList) {
            	newSyntax += varName+equality+value+_operator;
            }

            newSyntax = newSyntax.Substring (0, newSyntax.Length-4);
            convertedCode = convertedCode.Replace (aCondition.Value, newSyntax);
		}

	} // end of method Conditions()
} // end of class SweetScriptToUnityScript
