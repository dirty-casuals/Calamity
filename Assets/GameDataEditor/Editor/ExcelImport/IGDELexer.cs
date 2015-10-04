using System;
using System.Collections.Generic;

namespace GameDataEditor
{
    public interface IGDELexer
    {
        void AddDefinition(GDETokenDefinition tokenDefinition);
        IEnumerable<GDEToken> Tokenize(string source);
    }
}
