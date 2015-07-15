using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace psai.Editor
{
    internal class CommandDeclareManualBridgeSegment : ICommand
    {
        Segment _snippet;
        Group _sourceGroup;
        CommandChangeCompatibility _commandChangeCompatibility;     // if there was a Group Block, we need to create a command to restore it

        internal CommandDeclareManualBridgeSegment(Segment snippet, Group sourceGroup)
        {
            _snippet = snippet;
            _sourceGroup = sourceGroup;
        }

        public void Execute()
        {
            if (!_sourceGroup.ManualBridgeSnippetsOfTargetGroups.Contains(_snippet))
            {
                _sourceGroup.ManualBridgeSnippetsOfTargetGroups.Add(_snippet);
                //_snippet.SnippetType = SnippetType.BRIDGE;

                // disable Group Block, if exists
                if (_sourceGroup.ManuallyBlockedGroups.Contains(_snippet.Group))
                {
                    _commandChangeCompatibility = new CommandChangeCompatibility(_sourceGroup, _snippet.Group, CompatibilitySetting.neutral);
                    _commandChangeCompatibility.Execute();
                }

                EventArgs_BridgeSegmentToggled e = new EventArgs_BridgeSegmentToggled(_snippet, _sourceGroup);
                EditorModel.Instance.RaiseEvent_ManualBridgeSegmentToggled(e);

            }    
        }

        public void Undo()
        {
            if (_sourceGroup.ManualBridgeSnippetsOfTargetGroups.Contains(_snippet))
            {
                if (_commandChangeCompatibility != null)
                {
                    _commandChangeCompatibility.Undo();
                }

                _sourceGroup.ManualBridgeSnippetsOfTargetGroups.Remove(_snippet);

                EventArgs_BridgeSegmentToggled e = new EventArgs_BridgeSegmentToggled(_snippet, _sourceGroup);
                EditorModel.Instance.RaiseEvent_ManualBridgeSegmentToggled(e);
            }
        }

        public override string ToString()
        {
            string s = "Declare Segment '";
            s += _snippet.Name;
            s += "' a Manual Bridge Segment to Group ";
            s += _sourceGroup;
            return s;
        }
    }


    internal class CommandRevertBridgeSnippet : ICommand
    {
        Segment _snippet;
        Group _sourceGroup;

        internal CommandRevertBridgeSnippet(Segment snippet, Group sourceGroup)
        {
            _snippet = snippet;
            _sourceGroup = sourceGroup;
        }

        public void Execute()
        {
            if (_sourceGroup.ManualBridgeSnippetsOfTargetGroups.Contains(_snippet))
            {
                _sourceGroup.ManualBridgeSnippetsOfTargetGroups.Remove(_snippet);

                EventArgs_BridgeSegmentToggled e = new EventArgs_BridgeSegmentToggled(_snippet, _sourceGroup);
                EditorModel.Instance.RaiseEvent_ManualBridgeSegmentToggled(e);
            }
        }

        public void Undo()
        {
            if (!_sourceGroup.ManualBridgeSnippetsOfTargetGroups.Contains(_snippet))
            {
                _sourceGroup.ManualBridgeSnippetsOfTargetGroups.Add(_snippet);

                EventArgs_BridgeSegmentToggled e = new EventArgs_BridgeSegmentToggled(_snippet, _sourceGroup);
                EditorModel.Instance.RaiseEvent_ManualBridgeSegmentToggled(e);
            }
        }

        public override string ToString()
        {
            string s = "Remove Bridge-Snippet '";
            s += _snippet.Name;
            s += "' from Group ";
            s += _sourceGroup;
            return s;
        }
    }
}
