using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace psai.Editor
{

    internal abstract class CommandDeletePsaiEntity : ICommand
    {
        protected PsaiMusicEntity _entityToDelete = null;
        protected int _indexPositionWithinParent;
        protected PsaiProject _psaiProject;
        protected abstract void SaveAllOccurrencesOfEntityWithinProject();
        protected abstract void ReconstructAllOccurrencesOfEntityWithinProject();

        public void Execute()
        {
            SaveAllOccurrencesOfEntityWithinProject();
            EditorModel.Instance.Project.DeleteMusicEntity(_entityToDelete);
            EventArgs_PsaiEntityDeleted e = new EventArgs_PsaiEntityDeleted(_entityToDelete);
            EditorModel.Instance.RaiseEvent_PsaiEntityDeleted(e);
        }

        public void Undo()
        {
            ReconstructAllOccurrencesOfEntityWithinProject();
            EditorModel.Instance.RaiseEvent_PsaiEntityAdded(new EventArgs_PsaiEntityAdded(_entityToDelete));
        }

        public override string ToString()
        {
            string s = "delete ";
            s += _entityToDelete.GetClassString();
            s += " ";
            s += _entityToDelete.Name;
            return s;
        }
    }


    internal class CommandDeleteSegment : CommandDeletePsaiEntity
    {
        Segment _segmentToDelete;
        List<Group> _groupsThisSegmentWasABridgeSegmentTo = new List<Group>();
        List<Segment> _segmentsThisSegmentWasManuallyBlocked = new List<Segment>();
        List<Segment> _segmentsThisSegmentWasManuallyLinked = new List<Segment>();

        internal CommandDeleteSegment(PsaiProject project, Segment segmentToDelete)
        {
            _psaiProject = project;
            _entityToDelete = segmentToDelete;
            _segmentToDelete = segmentToDelete;
        }

        protected override void SaveAllOccurrencesOfEntityWithinProject()
        {
            _groupsThisSegmentWasABridgeSegmentTo.Clear();
            _segmentsThisSegmentWasManuallyBlocked.Clear();
            _segmentsThisSegmentWasManuallyLinked.Clear();
            _indexPositionWithinParent = _segmentToDelete.Group.Segments.IndexOf(_segmentToDelete);

            HashSet<Group> allGroups = _psaiProject.GetGroupsOfAllThemes();
            foreach (Group group in allGroups)
            {
                if (group.ManualBridgeSnippetsOfTargetGroups.Contains(_segmentToDelete))
                {
                    _groupsThisSegmentWasABridgeSegmentTo.Add(group);
                }
            }

            HashSet<Segment> allSnippets = _psaiProject.GetSegmentsOfAllThemes();
            foreach (Segment snippet in allSnippets)
            {
                if (snippet.ManuallyBlockedSnippets.Contains(_segmentToDelete))
                {
                    _segmentsThisSegmentWasManuallyBlocked.Add(snippet);
                }

                if (snippet.ManuallyLinkedSnippets.Contains(_segmentToDelete))
                {
                    _segmentsThisSegmentWasManuallyLinked.Add(snippet);
                }
            }
        }

        protected override void ReconstructAllOccurrencesOfEntityWithinProject()
        {
            ((Group)_segmentToDelete.GetParent()).AddSegment(_segmentToDelete, _indexPositionWithinParent);

            foreach (Group group in _groupsThisSegmentWasABridgeSegmentTo)
            {
                group.ManualBridgeSnippetsOfTargetGroups.Add(_segmentToDelete);
            }

            foreach (Segment segment in _segmentsThisSegmentWasManuallyBlocked)
            {
                segment.ManuallyBlockedSnippets.Add(_segmentToDelete);
            }

            foreach (Segment segment in _segmentsThisSegmentWasManuallyLinked)
            {
                segment.ManuallyLinkedSnippets.Add(_segmentToDelete);
            }
        }
    }

    internal class CommandDeleteGroup : CommandDeletePsaiEntity
    {
        Group _groupToDelete;        
        List<Group> _groupsThisGroupWasManuallyBlocked = new List<Group>();
        List<Group> _groupsThisGroupWasManuallyLinked = new List<Group>();

        internal CommandDeleteGroup(PsaiProject project, Group groupToDelete)
        {
            _psaiProject = project;
            _entityToDelete = groupToDelete;
            _groupToDelete = groupToDelete;
        }

        protected override void SaveAllOccurrencesOfEntityWithinProject()
        {
            _groupsThisGroupWasManuallyBlocked.Clear();
            _groupsThisGroupWasManuallyLinked.Clear();
            _indexPositionWithinParent = _groupToDelete.GetTheme().Groups.IndexOf(_groupToDelete);

            HashSet<Group> allGroups = _psaiProject.GetGroupsOfAllThemes();
            foreach (Group group in allGroups)
            {
                if (group.ManuallyBlockedGroups.Contains(_groupToDelete))
                {
                    _groupsThisGroupWasManuallyBlocked.Add(_groupToDelete);
                }

                if (group.ManuallyLinkedGroups.Contains(_groupToDelete))
                {
                    _groupsThisGroupWasManuallyLinked.Add(_groupToDelete);
                }

            }
        }

        protected override void ReconstructAllOccurrencesOfEntityWithinProject()
        {
            Theme parentTheme = _groupToDelete.GetTheme();
            
            if (_indexPositionWithinParent < parentTheme.Groups.Count)
            {
                parentTheme.Groups.Insert(_indexPositionWithinParent, _groupToDelete);
            }
            else
            {
                parentTheme.AddGroup(_groupToDelete);
            }
            
            foreach (Group group in _groupsThisGroupWasManuallyBlocked)
            {
                group.ManuallyBlockedGroups.Add(_groupToDelete);
            }

            foreach (Group group in _groupsThisGroupWasManuallyLinked)
            {
                group.ManuallyLinkedGroups.Add(_groupToDelete);
            }
        }
    }


    internal class CommandDeleteTheme : CommandDeletePsaiEntity
    {
        Theme _themeToDelete;
        List<Theme> _themesThisThemeWasManuallyBlocked = new List<Theme>();

        internal CommandDeleteTheme(PsaiProject psaiProject, Theme themeToDelete)
        {
            _psaiProject = psaiProject;
            _entityToDelete = themeToDelete;
            _themeToDelete = themeToDelete;
        }

        protected override void SaveAllOccurrencesOfEntityWithinProject()
        {
            _themesThisThemeWasManuallyBlocked.Clear();
            _indexPositionWithinParent = _psaiProject.Themes.IndexOf(_themeToDelete);
            foreach (Theme theme in _psaiProject.Themes)
            {
                if (theme.ManuallyBlockedTargetThemes.Contains(_themeToDelete))
                {
                    _themesThisThemeWasManuallyBlocked.Add(_themeToDelete);
                }
            }
        }

        protected override void ReconstructAllOccurrencesOfEntityWithinProject()
        {
            if (_indexPositionWithinParent < _psaiProject.Themes.Count)
            {
                _psaiProject.Themes.Insert(_indexPositionWithinParent, _themeToDelete);
            }
            else
            {
                _psaiProject.AddPsaiMusicEntity(_themeToDelete);
            }

            foreach (Theme theme in _themesThisThemeWasManuallyBlocked)
            {
                theme.ManuallyBlockedTargetThemes.Add(_themeToDelete);
            }

        }
    }
}

