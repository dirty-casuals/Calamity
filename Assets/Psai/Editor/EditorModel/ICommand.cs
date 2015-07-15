using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace psai.Editor
{
    internal interface ICommand
    {
        void Execute();
        void Undo();
        string ToString();
    }
}
