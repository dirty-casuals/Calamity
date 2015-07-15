using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace psai.Editor
{
    class CommandMoveSegmentsToGroup : ICommand
    {
        int _targetIndex;
        Group _targetGroup;
        Segment[] _movedSegments;
        Dictionary<Group, List<Segment>> _oldGroupContents;      // stores a copy of each Group's contents for undo()
        List<Group> _affectedGroups;

        public CommandMoveSegmentsToGroup(List<Segment> movedSegments, Group targetGroup, int targetIndex)
        {
            #if DEBUG
                Console.WriteLine("CommandMoveSegmentsToGroup()  CTOR " + this.GetHashCode() + "  move " + movedSegments.Count + " to group " + targetGroup + " at index " + targetIndex);
            #endif

            _targetGroup = targetGroup;
            _targetIndex = targetIndex;
            _oldGroupContents = new Dictionary<Group, List<Segment>>();            
            _affectedGroups = new List<Group>();
            _movedSegments = movedSegments.ToArray<Segment>();   // we need to copy the contents to make sure the List will not be cleared outside the Command, like when clearing the cut/paste-Buffer

            // create a list of all affected groups
            _affectedGroups.Add(_targetGroup);
            foreach (Segment snippet in _movedSegments)
            {
                if (!_affectedGroups.Contains(snippet.Group))
                {
                    _affectedGroups.Add(snippet.Group);
                }
            }

            string affectedGroupsString = " affectedGroups=";
            foreach(Group group in _affectedGroups)
            {
                affectedGroupsString += " ";
                affectedGroupsString +=  group.ToString();
            }

            #if DEBUG
                Console.WriteLine(affectedGroupsString);
            #endif
        }

        #region ICommand Members

        public void Execute()
        {
            #if DEBUG
                Console.WriteLine("CommandMovePsaiEntity::Execute()  _movedSegments.Count=" + _movedSegments.Length);
            #endif

            _oldGroupContents.Clear();
            
            foreach (Group group in _affectedGroups)
            {
                //Console.WriteLine("creating a copy of the Snippets for group " + group);
                List<Segment> oldSnippets = new List<Segment>();
                for (int i = 0; i < group.Segments.Count; i++)
                {
                    oldSnippets.Add(group.Segments[i]);
                    
                    #if DEBUG
                        Console.WriteLine("  - added " + group.Segments[i]);
                    #endif
                }

                _oldGroupContents[group] = oldSnippets;
            }

            foreach (Segment segment in _movedSegments)
            {
                segment.Group.RemoveSegment(segment);

                #if DEBUG
                    Console.WriteLine("  -removed Segment " + segment + " from group " + segment.Group);
                #endif
            }

            for (int i = _movedSegments.Length-1; i >= 0; i--) 
            {
                Segment snippet = _movedSegments[i];
                _targetGroup.AddSegment(snippet, _targetIndex);

                #if DEBUG
                    Console.WriteLine("  -added Segment " + snippet + " to group " + snippet.Group + " at index " + _targetIndex);
                #endif
            }

            //Console.WriteLine("_affectedGroups after Execute()");
            //Debug_PrintAffectedGroups();

            EventArgs_SegmentMovedToGroup e = new EventArgs_SegmentMovedToGroup(_affectedGroups, _movedSegments);
            EditorModel.Instance.RaiseEvent_SegmentMovedToGroup(e);
        }

        public void Undo()
        {
            //Console.WriteLine("CommandMoveSnippetToGroup::Undo() " + this.GetHashCode() + "  _snippets.Count=" + _movedSegments.Count + "   command=" + this.GetHashCode());
            
            for (int i = 0; i < _affectedGroups.Count; i++)
            {
                Group affectedGroup = _affectedGroups[i];

                #if DEBUG
                    Console.WriteLine("clearing affectedGroup " + affectedGroup);
                #endif
                //Console.WriteLine("replacing group " + oldGroup + " with the following snippets:");

                while (affectedGroup.Segments.Count > 0)
                {
                    affectedGroup.RemoveSegment(affectedGroup.Segments[0]);
                }
              
            }

            foreach (Group group in _oldGroupContents.Keys)
            {
                List<Segment> oldSegments = _oldGroupContents[group];

                for (int i = 0; i < oldSegments.Count; i++)
                    group.AddSegment(oldSegments[i]);
            }

            //Console.WriteLine("_affectedGroups after Undo()");
            //Debug_PrintAffectedGroups();

            EventArgs_SegmentMovedToGroup e = new EventArgs_SegmentMovedToGroup(_affectedGroups, _movedSegments);
            EditorModel.Instance.RaiseEvent_SegmentMovedToGroup(e);
        }

        private void Debug_PrintAffectedGroups()
        {
            #if DEBUG
                for (int g = 0; g < _affectedGroups.Count; g++)
                {                
                    Group group = _affectedGroups[g];
                    Console.WriteLine("---" + group + "---");
                    for (int i = 0; i < group.Segments.Count; i++)
                    {
                        Console.WriteLine(group.Segments[i]);
                    }
                }
            #endif
        }


        public override string ToString()
        {
            return "Move Segment(s)";
        }

        #endregion
    }
}
