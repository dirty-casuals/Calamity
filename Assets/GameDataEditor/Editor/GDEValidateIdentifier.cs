using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace GameDataEditor
{
    // Modified Gist from https://gist.github.com/LordDawnhunter/5245476
    public class GDEValidateIdentifier
    {
        // C# keywords: http://msdn.microsoft.com/en-us/library/x53a06bb(v=vs.120).aspx
        static List<string> keywords = new List<string>()
        {
            "abstract",     "as",           "base",         "bool",
            "break",        "byte",         "case",         "catch",
            "char",         "checked",      "class",        "const",
            "continue",     "decimal",      "default",      "delegate",
            "do",           "double",       "else",         "enum",
            "event",        "explicit",     "extern",       "false",
            "finally",      "fixed",        "float",        "for",
            "foreach",      "goto",         "if",           "implicit",
            "in",           "int",          "interface",    "internal",
            "is",           "lock",         "long",         "namespace",
            "new",          "null",         "object",       "operator",
            "out",          "override",     "params",       "private",
            "protected",    "public",       "readonly",     "ref",
            "return",       "sbyte",        "sealed",       "short",
            "sizeof",       "stackalloc"    ,"static",      "string",
            "struct",       "switch",       "this",         "throw",
            "true",         "try",          "typeof",       "uint",
            "ulong",        "unchecked",    "unsafe",       "ushort",
            "using",        "virtual",      "void",         "volatile",
            "while"
        };

        // definition of a valid C# identifier: http://msdn.microsoft.com/en-us/library/aa664670(v=vs.71).aspx
        const string formattingCharacter = @"\p{Cf}";
        const string connectingCharacter = @"\p{Pc}";
        const string decimalDigitCharacter = @"\p{Nd}";
        const string combiningCharacter = @"\p{Mn}|\p{Mc}";
        const string letterCharacter = @"\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nl}";
        const string identifierPartCharacter = letterCharacter + "|" +
            decimalDigitCharacter + "|" +
                connectingCharacter + "|" +
                combiningCharacter + "|" +
                formattingCharacter;
        const string identifierPartCharacters = "(" + identifierPartCharacter + ")+";
        const string identifierStartCharacter = "(" + letterCharacter + "|_)";
        const string identifierOrKeyword = identifierStartCharacter + "(" +
            identifierPartCharacters + ")*";

        public static bool IsValidIdentifier(string identifier)
        {
            if (String.IsNullOrEmpty(identifier)) return false;

            var validIdentifierRegex = new Regex("^" + identifierOrKeyword + "$");
            var normalizedIdentifier = identifier.Normalize();

            // 1. check that the identifier match the validIdentifer regex and it's not a C# keyword
            if (validIdentifierRegex.IsMatch(normalizedIdentifier) && !keywords.Contains(normalizedIdentifier))
            {
                return true;
            }

            // 2. check if the identifier starts with @
            if (normalizedIdentifier.StartsWith("@") && validIdentifierRegex.IsMatch(normalizedIdentifier.Substring(1)))
            {
                return true;
            }

            // 3. it's not a valid identifier
            return false;
        }
    }
}