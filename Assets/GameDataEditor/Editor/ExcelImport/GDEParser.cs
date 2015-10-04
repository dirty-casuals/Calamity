using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GameDataEditor
{
    public static class GDEParser
    {
        #region Token Types
        static string LEFT_BRACKET_TYPE = "(left_bracket)";
        static string RIGHT_BRACKET_TYPE = "(right_bracket)";
        static string LEFT_PAREN_TYPE = "(left_paren)";
        static string RIGHT_PAREN_TYPE = "(right_paren)";
        static string COMMA_TYPE = "(comma)";
        static string NUMBER_TYPE = "(number)";
        static string WHITE_SPACE_TYPE = "(white_space)";
        static string STRING_TYPE = "(string)";
        #endregion

        #region Token Regexs
        static string LEFT_BRACKET_REGEX = @"\[";
        static string RIGHT_BRACKET_REGEX = @"\]";
        static string LEFT_PAREN_REGEX = @"\(";
        static string RIGHT_PAREN_REGEX =  @"\)";
        static string COMMA_REGEX = @",";
        static string NUMBER_REGEX = @"[-+]?[0-9]*\.?[0-9]+";
        static string WHITE_SPACE_REGEX = @"\s+";
        static string STRING_REGEX = @"""([^\\""]|\\\\|\\"")*""";      
        #endregion

        static GDELexer lexer;

        static GDEParser()
        {
            lexer = new GDELexer();

            lexer.AddDefinition(new GDETokenDefinition(STRING_TYPE, new Regex(STRING_REGEX)));
            lexer.AddDefinition(new GDETokenDefinition(NUMBER_TYPE, new Regex(NUMBER_REGEX)));            
            
            lexer.AddDefinition(new GDETokenDefinition(LEFT_BRACKET_TYPE, new Regex(LEFT_BRACKET_REGEX)));
            lexer.AddDefinition(new GDETokenDefinition(RIGHT_BRACKET_TYPE, new Regex(RIGHT_BRACKET_REGEX)));
            
            lexer.AddDefinition(new GDETokenDefinition(LEFT_PAREN_TYPE, new Regex(LEFT_PAREN_REGEX)));
            lexer.AddDefinition(new GDETokenDefinition(RIGHT_PAREN_TYPE, new Regex(RIGHT_PAREN_REGEX)));
            
            lexer.AddDefinition(new GDETokenDefinition(COMMA_TYPE, new Regex(COMMA_REGEX), true));
            lexer.AddDefinition(new GDETokenDefinition(WHITE_SPACE_TYPE, new Regex(WHITE_SPACE_REGEX), true));
        }

        public static List<object> Parse(string expression)
        {
            List<object> results = new List<object>();
            var tokens = lexer.Tokenize(expression);

            List<object> tempList = null;
            foreach(var token in tokens)
            {
                if (token.Type.Equals(STRING_TYPE))                
                {
                    string t = token.Value.Remove(0, 1);
                    t = t.Remove(t.Length-1);
                    t = t.Replace(@"\\", @"\");
                    t = t.Replace(@"\""", @"""");
                    results.Add(t);
                }
                else if (token.Type.Equals(NUMBER_TYPE))
                    tempList.Add(token.Value);
                else if (token.Type.Equals(LEFT_PAREN_TYPE))                
                    tempList = new List<object>();
                else if (token.Type.Equals(RIGHT_PAREN_TYPE))
                    results.Add(tempList);
             }

            return results;
        }
    }
}

