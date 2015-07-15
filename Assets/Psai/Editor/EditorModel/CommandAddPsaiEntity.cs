using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace psai.Editor
{

    class CommandAddPsaiEntity : ICommand
    {
        private PsaiMusicEntity _entityToAdd = null;
        protected PsaiProject _psaiProject;
        private int _targetIndex = 0;       // the index position (within the Group / Theme / Project)

        public CommandAddPsaiEntity(PsaiProject psaiProject, PsaiMusicEntity entityToAdd, int targetIndex)
        {
            _psaiProject = psaiProject;
            _entityToAdd = entityToAdd;
            _targetIndex = targetIndex;
        }


        #region ICommand Members

        public void Execute()
        {
            _psaiProject.AddPsaiMusicEntity(_entityToAdd, _targetIndex);

            EventArgs_PsaiEntityAdded e = new EventArgs_PsaiEntityAdded(_entityToAdd);
            EditorModel.Instance.RaiseEvent_PsaiEntityAdded(e);
        }

        public void Undo()
        {
            _psaiProject.DeleteMusicEntity(_entityToAdd);

            EventArgs_PsaiEntityDeleted e = new EventArgs_PsaiEntityDeleted(_entityToAdd);
            EditorModel.Instance.RaiseEvent_PsaiEntityDeleted(e);
        }

        public override string ToString()
        {
            string s = "add ";
            s += _entityToAdd.GetClassString();
            s += " ";
            s += _entityToAdd.Name;
            return s;
        }

        #endregion
    }
}