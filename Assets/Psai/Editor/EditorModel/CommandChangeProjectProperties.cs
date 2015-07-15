using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace psai.Editor
{

    class CommandChangeProjectProperties : ICommand
    {
        private ProjectProperties _newProjectProperties;
        private ProjectProperties _oldProjectProperties;

        public CommandChangeProjectProperties(ProjectProperties newProperties)
        {
            _newProjectProperties = newProperties;
        }


        #region ICommand Members

        public void Execute()
        {
            _oldProjectProperties = EditorModel.Instance.ProjectProperties;
            EditorModel.Instance.ProjectProperties = _newProjectProperties;

            EditorModel.Instance.RaiseEvent_ProjectPropertiesChanged();
        }

        public void Undo()
        {
            EditorModel.Instance.ProjectProperties = _oldProjectProperties;

            EditorModel.Instance.RaiseEvent_ProjectPropertiesChanged();
        }

        public override string ToString()
        {
            string s = "change Project Properties";
            return s;
        }

        #endregion
    }
}
