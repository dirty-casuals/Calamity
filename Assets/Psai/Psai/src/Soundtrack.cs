//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;

namespace psai.net
{
    public class Soundtrack
    {
        public Dictionary<int, Theme> m_themes;
        public Dictionary<int, Segment> m_snippets;

        public Soundtrack()
        {
            m_themes = new Dictionary<int, Theme>();
            m_snippets = new Dictionary<int, Segment>();
        }

        public Soundtrack(psai.ProtoBuf_PsaiCoreSoundtrack pbSoundtrack) : this()
        {

            for (int i = 0; i < pbSoundtrack.themes.Count; i++)
            {
                psai.ProtoBuf_Theme pbTheme = pbSoundtrack.themes[i];
                Theme tempTheme = new Theme(pbTheme);
                m_themes[tempTheme.id] = tempTheme;
            }

            for (int i = 0; i < pbSoundtrack.snippets.Count; i++)
            {
                psai.ProtoBuf_Snippet pbSnippet = pbSoundtrack.snippets[i];
                Segment tempSnippet = new Segment(pbSnippet);

                m_snippets[tempSnippet.Id] = tempSnippet;

                Theme tmpTheme = m_themes[tempSnippet.ThemeId];
                if (tmpTheme != null)
                {
                    tmpTheme.m_segments.Add(tempSnippet);
                }
                else
                {
                    #if !(PSAI_NOLOG)
                    {
                        if (LogLevel.errors <= Logger.Instance.LogLevel)
                        {
	                        string s = "INTERNAL ERROR! could not find Theme for Theme id " + tempSnippet.ThemeId + " of snippet" + tempSnippet.Id;
	                        Logger.Instance.Log(s, LogLevel.errors);
                        }                    }
                    #endif
                }
            }
        }


        public void Clear()
        {
            m_themes.Clear();
            m_snippets.Clear();
        }

        public Theme getThemeById(int id)
        {
            if (m_themes.ContainsKey(id))
                return m_themes[id];
            else
                return null;
        }


        public Segment GetSegmentById(int id)
        {
            if (m_snippets.ContainsKey(id))
            {
                return m_snippets[id];
            }

            return null;
        }


        public SoundtrackInfo getSoundtrackInfo()
        {
            SoundtrackInfo soundtrackInfo = new SoundtrackInfo();
            soundtrackInfo.themeCount = m_themes.Count;
            soundtrackInfo.themeIds = new int[m_themes.Count];

            int i = 0;
            foreach (int themeId in m_themes.Keys)
            {
                soundtrackInfo.themeIds[i] = themeId;
                i++;
            }

            return soundtrackInfo;
        }


        public ThemeInfo getThemeInfo(int themeId)
        {
            Theme theme = getThemeById(themeId);

            if (theme != null)
            {
                ThemeInfo themeInfo = new ThemeInfo();
                themeInfo.id = theme.id;
                themeInfo.type = theme.themeType;
                themeInfo.name = theme.Name;

                // copy the snippetIds to the array
                themeInfo.segmentIds = new int[theme.m_segments.Count];
                for (int i = 0; i < theme.m_segments.Count; i++)
                {
                    themeInfo.segmentIds[i] = theme.m_segments[i].Id;
                }

                return themeInfo;
            }

            return null;
        }


        public SegmentInfo getSegmentInfo(int snippetId)
        {
            SegmentInfo segmentInfo = new SegmentInfo();

            Segment snippet = GetSegmentById(snippetId);

            if (snippet != null)
            {
                segmentInfo.id = snippet.Id;
                segmentInfo.intensity = snippet.Intensity;
                segmentInfo.segmentSuitabilitiesBitfield = snippet.SnippetTypeBitfield;
                segmentInfo.themeId = snippet.ThemeId;
                segmentInfo.playcount = snippet.Playcount;
                segmentInfo.name = snippet.Name;
                segmentInfo.fullLengthInMilliseconds = snippet.audioData.GetFullLengthInMilliseconds();
                segmentInfo.preBeatLengthInMilliseconds = snippet.audioData.GetPreBeatZoneInMilliseconds();
                segmentInfo.postBeatLengthInMilliseconds = snippet.audioData.GetPostBeatZoneInMilliseconds();
            }

            return segmentInfo;
        }



        public void UpdateMaxPreBeatMsOfCompatibleMiddleOrBridgeSnippets()
        {
            // update the maxPreBeatMsOfCompatibleSnippetsWithinSameTheme:
            foreach (Segment tmpSnippet in m_snippets.Values)
            {
                tmpSnippet.MaxPreBeatMsOfCompatibleSnippetsWithinSameTheme = 0;
                int nachfolgerCount = tmpSnippet.Followers.Count;

                for (int i = 0; i < nachfolgerCount; i++)
                {
                    int id = tmpSnippet.Followers[i].snippetId;
                    Segment tempFollowerSnippet = GetSegmentById(id);

                    if (
                         tempFollowerSnippet != null &&
                        ((tempFollowerSnippet.SnippetTypeBitfield & ((int)SegmentSuitability.middle | (int)SegmentSuitability.bridge)) > 0)
                        )
                    {
                        int preEnterMillis = tempFollowerSnippet.audioData.GetPreBeatZoneInMilliseconds();
                        if (tmpSnippet.MaxPreBeatMsOfCompatibleSnippetsWithinSameTheme < preEnterMillis)
                            tmpSnippet.MaxPreBeatMsOfCompatibleSnippetsWithinSameTheme = preEnterMillis;
                    }
                }
            }
        }

        /* Creates the indirectionToEnd and indirectionToTheme-References for all Segments throughout all Themes
        */
        public void BuildAllIndirectionSequences()
        {
            foreach (Theme theme in m_themes.Values)
            {
                theme.BuildSequencesToEndSegmentForAllSnippets();

                foreach (Theme targetTheme in m_themes.Values)
                {
                    if (theme != targetTheme && targetTheme.themeType != ThemeType.highlightLayer)
                    {
                        ThemeInterruptionBehavior themeInterruptionBehavior = Theme.GetThemeInterruptionBehavior(theme.themeType, targetTheme.themeType);
                        if (Theme.ThemeInterruptionBehaviorRequiresEvaluationOfSegmentCompatibilities(themeInterruptionBehavior))
                        {
                            theme.BuildSequencesToTargetThemeForAllSegments(this, targetTheme);
                        }
                    }                                       
                }
            }
        }
    }
}
