using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace psai.Editor
{

    class CommandAddSegments : ICommand
    {
        private string[] _filenames;
        protected PsaiProject _psaiProject;
        private Group _parentGroup;
        private List<Segment> _newSegments = new List<Segment>();

        public CommandAddSegments(PsaiProject psaiProject, string[] filenames, Group parentGroup)
        {
            _psaiProject = psaiProject;
            _filenames = filenames;
            _parentGroup = parentGroup;
        }

        public void Execute()
        {
#if DEBUG
            Console.WriteLine("CommandAddSegments::Execute()");
#endif

            if (_newSegments.Count == 0)
            {
                foreach (string path in _filenames)
                {
                    int segmentId = EditorModel.Instance.GetNextFreeSnippetIdBasedOnGroup(_parentGroup);
                    AudioData audioData = EditorModel.Instance.CreateAudioData(path);
                    Segment segment = new Segment(segmentId, audioData);
              
                    string filename = EditorModel.Instance.GetPathRelativeToProjectFileBasedOnAbsolutePath(path);
                    
                    segment.ThemeId = _parentGroup.Theme.Id;
                    segment.Id = EditorModel.Instance.GetNextFreeSnippetIdBasedOnGroup(_parentGroup);
                    segment.Group = _parentGroup;

                    System.Diagnostics.Debug.Assert(_parentGroup != null, "_parentGroup is NULL");

                    //System.Diagnostics.Debug.Assert(snippetId < 0, "snippetId=" + snippetId);
                    segment.AudioData.FilePathRelativeToProjectDir = filename;
                    segment.AudioData._prebeatLengthInSamplesEnteredManually = EditorModel.Instance.Project.Properties.DefaultPrebeatLengthInSamples;
                    segment.AudioData._postbeatLengthInSamplesEnteredManually = EditorModel.Instance.Project.Properties.DefaultPostbeatLengthInSamples;
                    segment.AudioData.Bpm = EditorModel.Instance.Project.Properties.DefaultBpm;
                    segment.AudioData.PreBeats = EditorModel.Instance.Project.Properties.DefaultPrebeats;
                    segment.AudioData.PostBeats = EditorModel.Instance.Project.Properties.DefaultPostbeats;
                    segment.AudioData.CalculatePostAndPrebeatLengthBasedOnBeats = EditorModel.Instance.Project.Properties.DefaultCalculatePostAndPrebeatLengthBasedOnBeats;
                    segment.SetStartMiddleEndPropertiesFromBitfield(EditorModel.Instance.Project.Properties.DefaultSegmentSuitabilites);

                    _newSegments.Add(segment);
                }
            }

            foreach (Segment segment in _newSegments)
            {
                #if DEBUG
                    Console.WriteLine("...adding Segment " + segment.Name + "  " + segment.GetHashCode() + "  to Group " + segment.Group.Name + "   " + segment.Group.GetHashCode());
                #endif
                _psaiProject.AddPsaiMusicEntity(segment);
                EventArgs_PsaiEntityAdded e = new EventArgs_PsaiEntityAdded(segment);
                EditorModel.Instance.RaiseEvent_PsaiEntityAdded(e);
            }

        }

        public void Undo()
        {
            foreach (Segment segment in _newSegments)
            {
                segment.Group.RemoveSegment(segment);

                EventArgs_PsaiEntityDeleted e = new EventArgs_PsaiEntityDeleted(segment);
                EditorModel.Instance.RaiseEvent_PsaiEntityDeleted(e);
            }
        }

        public override string ToString()
        {
            string s = "add " + _filenames.Length + " Audiofile(s) as Segments(s)";            
            return s;
        }
    }
}