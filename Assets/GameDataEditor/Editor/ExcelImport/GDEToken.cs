using System;

namespace GameDataEditor
{
    public class GDEToken
    {
        public GDEToken(string type, string value, GDETokenPosition position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

        public GDETokenPosition Position { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format("Token: {{ Type: \"{0}\", Value: \"{1}\", Position: {{ Index: \"{2}\", Line: \"{3}\", Column: \"{4}\" }} }}", Type, Value, Position.Index, Position.Line, Position.Column);
        }
    }
}
