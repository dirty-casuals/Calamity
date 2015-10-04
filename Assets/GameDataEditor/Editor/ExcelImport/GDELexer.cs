using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GameDataEditor
{
    public class GDELexer : IGDELexer
    {
        Regex endOfLineRegex = new Regex(@"\r\n|\r|\n", RegexOptions.Compiled);
        IList<GDETokenDefinition> tokenDefinitions = new List<GDETokenDefinition>();

        public void AddDefinition(GDETokenDefinition tokenDefinition)
        {
            tokenDefinitions.Add(tokenDefinition);
        }

        public IEnumerable<GDEToken> Tokenize(string source)
        {
            int currentIndex = 0;
            int currentLine = 1;
            int currentColumn = 0;

            while (currentIndex < source.Length)
            {
                GDETokenDefinition matchedDefinition = null;
                int matchLength = 0;

                foreach (var rule in tokenDefinitions)
                {
                    var match = rule.Regex.Match(source, currentIndex);

                    if (match.Success && (match.Index - currentIndex) == 0)
                    {
                        matchedDefinition = rule;
                        matchLength = match.Length;
                        break;
                    }
                }

                if (matchedDefinition == null)
                {
                    throw new Exception(string.Format(GDEConstants.LexerExceptionFormat, source[currentIndex], currentIndex, currentLine, currentColumn));
                }
                else
                {
                    var value = source.Substring(currentIndex, matchLength);

                    if (!matchedDefinition.IsIgnored)
                        yield return new GDEToken(matchedDefinition.Type, value, new GDETokenPosition(currentIndex, currentLine, currentColumn));

                    var endOfLineMatch = endOfLineRegex.Match(value);
                    if (endOfLineMatch.Success)
                    {
                        currentLine += 1;
                        currentColumn = value.Length - (endOfLineMatch.Index + endOfLineMatch.Length);
                    }
                    else
                    {
                        currentColumn += matchLength;
                    }

                    currentIndex += matchLength;
                }
            }

            yield return new GDEToken("(end)", null, new GDETokenPosition(currentIndex, currentLine, currentColumn));
        }
    }
}
