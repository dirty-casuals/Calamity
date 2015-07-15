using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace psai.Editor
{

    // generic class that represents a Command for changing any property of any PsaiMusicEntity.
    // This is done by saving a clone of the original Entity and copying its values to another
    // instance of PsaiMusicEntity.
    class CommandChangePsaiEntityProperty : ICommand
    {
        private PsaiMusicEntity _originalEntityCopy = null;
        public PsaiMusicEntity _originalEntityReference = null;
        private PsaiMusicEntity _changedEntity = null;
        public string _descriptionOfChange;
        private bool _changeAffectsCompatibilites;

        public CommandChangePsaiEntityProperty(PsaiMusicEntity changedEntity, ref PsaiMusicEntity originalEntity, string descriptionOfChange = null)
        {
            _originalEntityReference = originalEntity;
            _originalEntityCopy = originalEntity.ShallowCopy();
            _changedEntity = changedEntity;
            _descriptionOfChange = descriptionOfChange;
            _changeAffectsCompatibilites = changedEntity.PropertyDifferencesAffectCompatibilities(originalEntity);
        }


        #region ICommand Members

        public void Execute()
        {
#if DEBUG
            Console.WriteLine("CommandChangePsaiEntityProperty::Execute() _changedEntity=" + _changedEntity.Name + "   _originalEntityRef=" + _originalEntityReference + "  _originalEntitiyRef.GetHashCode()=" + _originalEntityReference.GetHashCode());
#endif
            PnxHelperz.CopyTo(_changedEntity, _originalEntityReference);
            
            // special case: upon change of theme id, set internal themeId of each snippet
            if (_changedEntity is Theme && ((Theme)_changedEntity).Id != ((Theme)_originalEntityCopy).Id)
            {
                foreach (Segment snippet in ((Theme)_originalEntityReference).GetSegmentsOfAllGroups())
                {
                    snippet.ThemeId = ((Theme)_changedEntity).Id;
                }
            }

            EventArgs_PsaiEntityPropertiesChanged e = new EventArgs_PsaiEntityPropertiesChanged(_originalEntityReference, _changeAffectsCompatibilites);
            e.DescriptionOfChange = _descriptionOfChange;

            EditorModel.Instance.RaiseEvent_PsaiEntityPropertyChanged(e);
        }

        public void Undo()
        {
            PnxHelperz.CopyTo(_originalEntityCopy, _originalEntityReference);

            // special case: upon change of theme id, set internal themeId of each snippet
            if (_changedEntity is Theme)
            {
                Theme changedTheme = _changedEntity as Theme;
                Theme originalThemeCopy = _originalEntityCopy as Theme;
                if (changedTheme.Id != originalThemeCopy.Id)
                {
                    foreach (Segment snippet in ((Theme)_originalEntityReference).GetSegmentsOfAllGroups())
                    {
                        snippet.ThemeId = originalThemeCopy.Id;
                    }
                }
            }

            EventArgs_PsaiEntityPropertiesChanged e = new EventArgs_PsaiEntityPropertiesChanged(_originalEntityReference, _changeAffectsCompatibilites);
            e.DescriptionOfChange = " Undo " + this._descriptionOfChange;
            EditorModel.Instance.RaiseEvent_PsaiEntityPropertyChanged(e);
        }

        public override string ToString()
        {
            string s = "edit of ";
            s += _changedEntity.GetClassString();
            s += " ";
            s += _changedEntity.Name;
            return s;
        }

        #endregion
    }
}
