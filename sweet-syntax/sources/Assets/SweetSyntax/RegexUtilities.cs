/// <summary>
/// RegexUtilities class
///
/// This class provide some structs and variables to help in the search and replace of text using regex
///
/// Version used by the SweetScriptToUnityScript and SweetSharpToCSharp classes
/// in the SweetSyntax extension for Unity3D.
/// Website : http://www.florent-poujol.fr/en/unity3d/sweetsyntax
/// Documentation : http://www.florent-poujol.fr/assets/unity3d/sweetsyntax/
///
///
/// Version: 1.0 (sole version that was ever released)
/// Release: september 2012
///
/// Created by Florent POUJOL
/// florent.poujol@gmail.com
/// http://www.florent-poujol.fr/en
/// Profile on Unity's forums : http://forum.unity3d.com/members/23148-Lion
/// </summary>


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class RegexUtilities {

    /// <summary>
    /// 
    /// </summary>
	public struct Block {
        public Match match; // the Match of the block's declaration

        public int startIndex;
        public int endIndex; // index of the opening and closing bracket of block in reftext
        public string refText; //

        public string name; // name of the block (Block is used only with methods or classes)
        public string type; // type if the block is a method
        public string declaration; // full block's declaration (up util the opening bracket). Usually the match's value
        public string newDeclaration;
        public string text; // text inside the block between the opening and closing bracket which are included
        public string newText;

        public bool isEmpty; // tell wether text is empty or not


        // ----------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_match">Match of the block's declaration</param>
        /// <param name="p_refText">full reference text in which to find the block</param>
        public Block (Match p_match, string p_refText) {
            match = p_match;
            refText = p_refText;

            declaration = match.Value;
            newDeclaration = "";
            
            name = match.Groups[2].Value;
            try {
                name = match.Groups["blockName"].Value;
            } catch {}

            type = "";
            try {
                type = match.Groups["blockType"].Value;
            } catch {}
            
            startIndex = match.Index + match.Length - 1;
            endIndex = 0;
            text = "";
            newText = "";
            isEmpty = true;

            endIndex = GetEndOfBlockIndex ();
            
            if (endIndex == -1)
                return;

            if (endIndex <= startIndex) {
                Debug.LogError ("RegexUtilities.Block.Block() : endIndex <= startIndex. Can't get block text. match=["+match.Value+"] startIndex=["+startIndex+"] endIndex=["+endIndex+"] refText=["+refText+"].");
                return;
            }

            text = refText.Substring (startIndex, endIndex-startIndex); // the openeing and closing brackets are included in text
            isEmpty = (text.Trim() == "");
            newText = text;
        }


        // ----------------------------------------------------------------------------------

        /// <summary>
        /// Search for the block's closing curcly bracket, given the index in refText (startIndex) of the opening bracket
        /// </summary>
        int GetEndOfBlockIndex () {
            int openedBrackets = 0;

            for (int i = startIndex; i < refText.Length; i++) {
                if (refText[i] == '{')
                    openedBrackets++;

                if (refText[i] == '}') {
                    openedBrackets--;

                    if (openedBrackets == 0)
                        return i+1;
                }
            }

            // no matching closing bracket has been found
            Debug.LogError ("RegexUtilities.Block.GetEndOfBlockIndex() : No matching closing bracket has been found ! Returning -1. match=["+match.Value+"] startIndex=["+startIndex+"] ["+
                refText[startIndex-1]+"|"+refText[startIndex]+"|"+refText[startIndex+1]+"] text=["+refText+"].");
            return -1;
        }
    } // end of struct Block


    // ----------------------------------------------------------------------------------

	// most common characters used in names. Does not match arrays or generic collections
    protected string commonName = "([A-Za-z0-9_\\.]+)";
    
    // same as common name but includes also arrays, generic collections, strings
    // usefull when looking for a type (of variable or method)
    protected string commonChars = "([A-Za-z0-9<>,'\"_\\[\\]\\.]+)"; // 
    protected string commonCharsWithSpace = "([A-Za-z0-9<>,'\"_\\[\\]\\. ]+)"; // generic collections likes dictionnaries may have a space after the come, for instance
    protected string commonCharsWithoutComma = "([A-Za-z0-9<>'\"_\\[\\]\\.]+)"; // for use with variable or type as method parameter

    protected string visibility = "(?<visibility>public|private|protected)";

    // white spaces (or new line)
    protected string optWS = "(\\s|\\n)*"; // optionnal white space
    protected string oblWS = "(\\s|\\n)+"; // obligatory white space

    protected string genericCollections = "(Comparer|Dictionary|HashSet|KeyedByTypeCollection|LinkedList|LinkedListNode|List|Queue|SortedDictionary|SortedList|SortedSet|Stack|SynchronizedCollection"+
        "|SynchronizedKeyedCollection|SynchronizedReadOnlyCollection|ISet)";
    
    // list of the patterns and corresponding replacements to be processed by DoReplacements()
    protected List<string> patterns = new List<string> ();
    protected List<string> replacements = new List<string> ();
	protected string pattern;
    protected string replacement;
	
    // end of line
    protected string EOL = "\n"; // may throw some "inconsistent line ending blabla" warnings in the console

    // tranlated code to be returned
    public string convertedCode = "";

 
    // ----------------------------------------------------------------------------------


    /// <summary>
    /// Process the patterns/replacements
    /// </summary>
    protected void DoReplacements () {
        convertedCode = DoReplacements (convertedCode);
    }

    protected string DoReplacements (string text) {
        if (patterns.Count != replacements.Count) {
            Debug.LogError ("Patterns and replacements count mismatch : patterns.Count="+patterns.Count+" replacements.Count="+replacements.Count);
            return text;
        }

        try { // some regex may throws nasty exceptions
            for (int i = 0; i < patterns.Count; i++)
                text = Regex.Replace (text, patterns[i], replacements[i]);

            patterns.Clear ();
            replacements.Clear ();
        }
        catch (System.Exception e) {
            Debug.LogError (patterns.Count+" "+replacements.Count+" "+e);
            Debug.LogWarning (text.Substring (0, 100));
        }

        return text;
    }


    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Do a Regex.Matches but return the result in the inverse order
    /// </summary>
    protected List<Match> ReverseMatches (string text, string pattern) {
        MatchCollection matches = Regex.Matches (text, pattern);
        
        Stack<Match> stack = new Stack<Match> ();
        foreach (Match match in matches)
            stack.Push (match);
        // the matches piles up in the stack, so the lastest match in matches is now the first one in stack

        return new List<Match> (stack);
    }
} // end of class RegexUtilities
