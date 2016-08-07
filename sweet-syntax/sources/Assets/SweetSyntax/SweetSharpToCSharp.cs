/// <summary>
/// SweetSharpToCSharp class
///
/// This class handle the conversion from the Sweet# scripting language to the regular C#.
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


public class SweetSharpToCSharp : RegexUtilities {

	/// <summary>
	/// Constructor and main method
	/// </summary>
	public SweetSharpToCSharp (string inputCode) {
		convertedCode = inputCode;

		Properties ();
        Classes ();
        Loops ();
        Conditions ();

        // NEW OBJECT INSTANCIATION

		// allow to declare a variable while instanciating a new empty instance of a class
		// name new class    =>  class name = new class ();
		patterns.Add ("(?<varName>"+commonName+")"+oblWS+"new"+oblWS+"(?<varType>"+commonCharsWithSpace+")(\\((?<params>"+commonCharsWithSpace+")\\)"+optWS+")?;");
		replacements.Add ("${varType} ${varName} = new ${varType}(${params});");
		
		DoReplacements ();

		// STRING

		// allow to use "str" instead of "string"
		// works with var declaraion with or without value, parameters, method/properties
        patterns.Add ( "\\bstr\\b(("+optWS+"\\["+optWS+"\\])?"+oblWS+commonName+optWS+"(=|;|{|,|\\(|\\)))" );
        replacements.Add ( "string$1" );

	    // with generic collections
        patterns.Add ( "((<|,)"+optWS+")\\bstr\\b(("+optWS+"\\["+optWS+"\\])?"+optWS+"(>|,))" );
        replacements.Add ( "$1string$4" );

		DoReplacements ();
	}


	// ----------------------------------------------------------------------------------

	/// <summary>
	/// Properties declaration have never been more simple !
	/// Default getter and/or setter :
	/// type Name getset; // getter+setter
	/// type Name get; // only getter (Read only property)
	/// type Name set; // only setter
	/// </summary>
	void Properties () {
		pattern = commonCharsWithSpace+oblWS+commonName+oblWS+"(?<getOrSet>\\b((getset)|get|set)\\b"+optWS+";)";
		List<Match> allProps = ReverseMatches (convertedCode, pattern);
 		
 		foreach (Match aProp in allProps) {
			string getOrSet = aProp.Groups["getOrSet"].Value;
			string newSyntax = aProp.Value;
			
			if (getOrSet.Contains ("getset"))
				newSyntax = newSyntax.Replace (getOrSet, "{ get; set; }");
			
			else if (getOrSet.Contains ("get"))
				newSyntax = newSyntax.Replace (getOrSet, "{ get; }");
			
			else if (getOrSet.Contains ("set"))
				newSyntax = newSyntax.Replace (getOrSet, "{ set; }");
			
			convertedCode =  convertedCode.Replace (aProp.Value, newSyntax);
		}

		// running the code a second time if the semi colon has ben forgotten
		pattern = commonCharsWithSpace+oblWS+commonName+oblWS+"(?<getOrSet>\\b((get"+optWS+"set)|get|set)\\b)";
		allProps = ReverseMatches (convertedCode, pattern);
 		
 		foreach (Match aProp in allProps) {
			string getOrSet = aProp.Groups["getOrSet"].Value;
			string newSyntax = aProp.Value;
			
			if (getOrSet.Contains ("getset"))
				newSyntax = newSyntax.Replace (getOrSet, "{ get; set; }");
			
			else if (getOrSet.Contains ("get"))
				newSyntax = newSyntax.Replace (getOrSet, "{ get; }");
			
			else if (getOrSet.Contains ("set"))
				newSyntax = newSyntax.Replace (getOrSet, "{ set; }");
			
			convertedCode =  convertedCode.Replace (aProp.Value, newSyntax);
		}
	}

	
	// ----------------------------------------------------------------------------------

	/// <summary>
	/// Handle conversion of classes-related stuffs
	/// </summary>
	void Classes () {
		// loop the classes declarations in the file
        pattern = "\\bclass"+oblWS+"(?<blockName>"+commonName+")"+
        "("+optWS+":"+optWS+"(?<parentClassName>"+commonName+"))?"+
        "("+optWS+","+optWS+commonName+")*"+
        optWS+"{";
        List<Match> allClasses = ReverseMatches (convertedCode, pattern);
        
        foreach (Match aClass in allClasses) {
			Block classBlock = new Block (aClass, convertedCode);

			if (classBlock.isEmpty)
				continue;
			
			if (aClass.Groups["parentClassName"].Value != "") { // this class inherits from another one
				patterns.Add ("\\bparent("+optWS+"\\.)");
				replacements.Add ("base$1");
			}


			// parent constructor call with parameters
			patterns.Add ("((:|callfirst)"+optWS+")(parent|base)("+optWS+"\\(.*\\)"+optWS+"{)");
			replacements.Add (":$3base$5");

			// parent constructor call without parameters
			patterns.Add ("((:|callfirst)"+optWS+")(parent|base)("+optWS+"{)");
			replacements.Add (":$3base()$5");


			// constructor call with parameters
			patterns.Add ("((:|callfirst)"+optWS+")(constructor|this)("+optWS+"\\(.*\\)"+optWS+"{)");
			replacements.Add (":$3this$5");

			// constructor call with parenthesis
			patterns.Add ("((:|callfirst)"+optWS+")(constructor|this)("+optWS+"{)");
			replacements.Add (":$3this()$5");


			// constructor declaration with parameters
			patterns.Add ("constructor("+optWS+"\\(.*\\)"+optWS+
			"(:"+optWS+"(base|this)"+optWS+"\\(.*\\)"+optWS+")?{)");
			replacements.Add ("public "+classBlock.name+"$1");

			// constructor declaration without parenthesis
			patterns.Add ("constructor("+optWS+"(:"+optWS+"(base|this)"+optWS+"\\(.*\\)"+optWS+")?{)");
			replacements.Add ("public "+classBlock.name+"()$1");


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
		// the parenthesis are always optionnal

		// generic for(;;) loops

			// foreach type name from [from] to [to] step [step] {}
			// => for (int name=[from]; Name<=[to] ; Name+=[step])
			// Can also default the from to 0 and the step to +1
			// equality sign will always be <= if from or to is a variable name and not a value

			pattern = "\\b(for|foreach)("+optWS+"\\("+optWS+"|"+oblWS+")(?<varType>"+commonChars+oblWS+")?(?<varName>"+commonName+")"+
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

				if (varType.Trim () == "") varType = "int ";
				if (from == "") from = "0";
				if (step == "") step = "1";

				string equalityOperator = GetEqualityOperator (varType, from, to);				
				string newLoop = "for ("+varType+varName+" = "+from+"; "+varName+equalityOperator+to+"; "+varName+"+="+step+")"+end;
				convertedCode = convertedCode.Replace (aLoop.Value, newLoop);
			}


			// boo-like syntax with range operator
			// for (int i in range(from,to,step))
			// allow without parenthesis or type
			pattern = "(for|foreach)"+
			"("+optWS+"\\("+optWS+"|("+oblWS+"))"+
			"(?<varType>"+commonChars+oblWS+")?(?<varName>"+commonName+")"+
			oblWS+"in"+oblWS+"(range|Range)"+optWS+"\\("+
			"("+optWS+"(?<from>"+commonName+"))"+
			"("+optWS+","+optWS+"(?<to>"+commonName+"))?"+
			"("+optWS+","+optWS+"(?<step>"+commonName+"))?"+
			optWS+"\\)("+optWS+"\\))?"+
			"(?<end>"+optWS+"{)?";
			allLoops = ReverseMatches (convertedCode, pattern);

			foreach (Match aLoop in allLoops) {
				string varName = aLoop.Groups["varName"].Value;
				string varType = aLoop.Groups["varType"].Value;
				string from = aLoop.Groups["from"].Value.Trim ();
				string to = aLoop.Groups["to"].Value.TrimStart (',').Trim ();
				string step = aLoop.Groups["step"].Value.TrimStart (',').Trim ();
				string end = aLoop.Groups["end"].Value;

				if (varType.Trim () == "") varType = "int ";

				if (to == "" && step == "") { // there is only one number between parenthesis, that's actually [to] and not [from]
					to = from;
					from = "0";
					step = "1";
				}

				if (step == "") step = "1";

				string equalityOperator = GetEqualityOperator (varType, from, to).TrimEnd ('='); // with range, the condition is always [from]<[to] or [from]>[to]
				string newLoop = "for ("+varType+varName+" = "+from+"; "+varName+equalityOperator+to+"; "+varName+"+="+step+")"+end;
				convertedCode = convertedCode.Replace (aLoop.Value, newLoop);
			}


		// for(;;) loop with index-based collection or array

			// index will always be an int
			// foreach index Name [= startIndex] in [coll|collection] array {}
			// => for (int name = 0; name<array.length; name++)
			pattern = "(for|foreach)("+optWS+"\\("+optWS+"|"+oblWS+")"+
			"index"+oblWS+"(?<varName>"+commonName+")(?<value>"+optWS+"="+optWS+commonName+")?"+oblWS+"in"+oblWS+
			"(?<collection>(col|collection)"+oblWS+")?(?<arrayName>"+commonName+")("+optWS+"\\))?(?<end>"+optWS+"{)?";
			allLoops = ReverseMatches (convertedCode, pattern);

			foreach (Match aLoop in allLoops) {
				string varName = aLoop.Groups["varName"].Value;
				string value = aLoop.Groups["value"].Value;
				string arrayName = aLoop.Groups["arrayName"].Value;
				string collection = aLoop.Groups["collection"].Value.Trim ();
				string end = aLoop.Groups["end"].Value;

				if (value.Trim () == "") value = " = 0";

				string newLoop = "for (int "+varName+value+"; "+varName+"<";

				if (collection == "")
					newLoop += arrayName+".Length; "+varName+"++)";
				else
					newLoop += arrayName+".Count; "+varName+"++)";

				newLoop += end;
				convertedCode = convertedCode.Replace (aLoop.Value, newLoop);
			}


		// for(in) loop with arrays/collections
			
			// foreach type Name in collections {}
			// => for (type name in collection)

			patterns.Add ("\\b(for|foreach)("+optWS+"\\("+optWS+"|"+oblWS+")(?<statement>"+commonCharsWithSpace+oblWS+commonName+oblWS+"in"+oblWS+
			commonName+")("+optWS+"\\))?(?<end>"+optWS+"{)?");
			replacements.Add ("foreach (${statement})${end}");

		DoReplacements ();
	}


	// ----------------------------------------------------------------------------------

	/// <summary>
	/// Resolve the equality operator to use between the from and to parameters, depending on theirs value
	/// </summary>
	string GetEqualityOperator (string varType, string from, string to) {
		string equalityOperator = "<=";

		switch (varType.Trim ()) {
			case "int": 
				int iFrom = 0, iTo;
				if (Int32.TryParse (from, out iFrom) && Int32.TryParse (to, out iTo)) { // check if from and to are hardcoded value and not a variable name
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
	    "(if|while)"+optWS+"\\(.+\\)",
	    "for"+optWS+"\\([^;]+;[^;]+(;.+)?\\)",
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

		    	newCond = DoReplacements(newCond);

		    	convertedCode = convertedCode.Replace (aCondition.Value, newCond);
		    }
		}

		//--------------------

		// (name isoneof value1, value2)
		// (name == value1 || name == value2)
		pattern = "(?<varName>"+commonName+")"+oblWS+"(?<equality>isoneof|isnotoneof)"+oblWS+
		"(?<valueList>"+commonCharsWithSpace+optWS+"(,"+optWS+commonCharsWithSpace+optWS+")+)";
		List<Match> allStatements = ReverseMatches (convertedCode, pattern);
		//Debug.Log (allDeclarations.Count);

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
} // end of class SweetSharpToCSharp
