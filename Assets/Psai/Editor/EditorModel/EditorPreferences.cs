using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace psai.Editor
{
    public class EditorPreferences
    {
        private bool _showTooltips = true;
        public bool ShowToolTips
        {
            get { return _showTooltips; }
            set
            {
                _showTooltips = value;
            }
        }
    }
}
