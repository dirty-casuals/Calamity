using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;

namespace psai.Editor
{
    /* represents the psai soundtrack data-structure as written to the .pcb file.
     * The binary presentation ignores the concept of Groups and manual links/blocks.
     */
    internal class PsaiBinarySoundtrack
    {
  
        List<Theme> _themes = new List<Theme>();
        List<Segment> _segments = new List<Segment>();     // all Segments of all Themes

        internal void Init()
        {
            _themes.Clear();
            _segments.Clear();
        }

        internal string Name
        {
            get;
            set;
        }

        internal string AudioFormat
        {
            get;
            set;
        }

        internal List<Theme> Themes
        {
            get { return _themes; }            
        }

        internal List<Segment> Segments
        {
            get { return _segments; }
        }


        internal psai.net.Soundtrack CreatePsaiDotNetVersion(PsaiProject parentProject)
        {
            psai.net.Soundtrack netSoundtrack = new psai.net.Soundtrack();
          
            foreach (Theme theme in Themes)
                netSoundtrack.m_themes.Add(theme.Id, theme.CreatePsaiDotNetVersion());

            foreach (Segment segment in Segments)
                netSoundtrack.m_snippets.Add(segment.Id, segment.CreatePsaiDotNetVersion(parentProject));

            return netSoundtrack;
        }

        internal ProtoBuf_PsaiCoreSoundtrack CreateProtoBuf(PsaiProject parentProject)
        {
            ProtoBuf_PsaiCoreSoundtrack pbSoundtrack = new ProtoBuf_PsaiCoreSoundtrack();

            pbSoundtrack.audioformat = AudioFormat;

            foreach (Theme theme in Themes)
                pbSoundtrack.themes.Add(theme.CreateProtoBuf());

            foreach (Segment segment in Segments)
                pbSoundtrack.snippets.Add(segment.CreateProtoBuf(parentProject));

            return pbSoundtrack;
        }
    }
}
