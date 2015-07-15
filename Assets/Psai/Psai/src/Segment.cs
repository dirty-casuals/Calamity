//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;


namespace psai.net
{
    public class Follower
	{
        public float compatibility;
        public int snippetId;
		//internal float score;

        public Follower()
        {}

        public Follower(int id, float compatibility)
        {
            this.snippetId = id;
            this.compatibility = compatibility;
        }
	};


    // these are bit-flags! watch out to keep in binary system when adding a value

    /// <summary>
    /// Flags that mark the suitability of a Segment for different playback position within its Theme
    /// </summary>
    public enum SegmentSuitability
    {
        /// <summary>
        /// no suitability set yet
        /// </summary>
        none = 0,

        /// <summary>
        /// suitable to start a Theme out of silence
        /// </summary>
        start = 1,

        /// <summary>
        /// suitable to be played in the middle of a Theme
        /// </summary>
        middle = 2,

        /// <summary>
        /// suitable to end its Theme and go to silence
        /// </summary>
        end = 4,

        /// <summary>
        /// this Segment shall generally be used when transitioning from other Groups to this Segment's Group
        /// </summary>
        /// <remarks>
        /// This can mean either the 'Automatic Bridge Segment' suitability was set, or the Segment has been declared a Manual Bridge Segment for one or multiple Groups
        /// </remarks>
        bridge = 8,

        /// <summary>
        /// all bits set (internal use only)
        /// </summary>
        whatever = 15
    }

    public class Segment
    {
        public AudioData audioData;

        public int Id { get; set; }
        public float Intensity { get; set; }
        public int ThemeId { get; set; }
        public string Name { get; set; }

		/* counter how often the snippet has been replayed so far */
        public int Playcount { get; set; }

        /* the size (milliseconds) of the longest prebeat-zone of all compatible snippets */
        public int MaxPreBeatMsOfCompatibleSnippetsWithinSameTheme { get; set; }

        public List<Follower> Followers { get; private set; }

        /* if no direct transition is possible, psai will use this map of compatible Segments to follow the shortest path to the given Theme */
        public Dictionary<int, Segment> MapOfNextTransitionSegmentToTheme;        // Target Theme Id, compatible Segment

        /* contains a bool flag if there is at least one compatible follower of a valid SnippetType in the given Target Theme id*/
        private Dictionary<int, bool> _mapDirectTransitionToThemeIsPossible;   // Key: target Theme Id

		private int _snippetTypeBitfield = 0;

        /* bitwise combination of SnippetType flags, that describe the field of application of this Snippet  (Start, Middle, End) */
        public int SnippetTypeBitfield
        {
            get { return _snippetTypeBitfield;}
            set { _snippetTypeBitfield = value;}
        }

        public Segment()
        {
            Followers = new List<Follower>();
            _mapDirectTransitionToThemeIsPossible = new Dictionary<int, bool>();
            MapOfNextTransitionSegmentToTheme = new Dictionary<int, Segment>();
        }

        public Segment nextSnippetToShortestEndSequence;

        public Segment(psai.ProtoBuf_Snippet pbSnippet) : this()
	    {            
		    this.Id = pbSnippet.id;
		    this.ThemeId = pbSnippet.theme_id;
		    this.Intensity = pbSnippet.intensity;
		    this.SnippetTypeBitfield = pbSnippet.snippet_type;
		    this.Name = pbSnippet.name;
		    this.audioData = new AudioData(pbSnippet.audio_data);
		
		    for (int i=0; i<pbSnippet.follower_id.Count; i++)
		    {			    			    
                Follower follower = new Follower(pbSnippet.follower_id[i], pbSnippet.follower_compatibility[i]);
                this.Followers.Add(follower);                			    
		    }
	    }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            sb.Append(" (");
            sb.Append(Id);
            sb.Append(")");
            sb.Append(" ");
            sb.Append(GetStringFromSegmentSuitabilities(this.SnippetTypeBitfield));
            sb.Append(" [");
            sb.Append(Intensity.ToString("F2"));
            sb.Append("]");
            return sb.ToString();
        }

        public bool IsUsableAs(SegmentSuitability snippetType)
        {
            return ((int)(this.SnippetTypeBitfield & (int)snippetType) > 0);
        }

        public bool IsUsableOnlyAs(SegmentSuitability snippetType)
        {
            return ((int)(this.SnippetTypeBitfield & (int)snippetType) == (int)snippetType);
        }


        private void SetSnippetTypeFlag(SegmentSuitability snippetType)
        {
            int argAsBitmask = (int)snippetType;
            this.SnippetTypeBitfield = this.SnippetTypeBitfield | argAsBitmask;
        }

        private void ClearSnippetTypeFlag(SegmentSuitability snippetType)
        {
            int argAsBitmask = (int)snippetType;
            this.SnippetTypeBitfield = this.SnippetTypeBitfield & ~argAsBitmask;
        }


        // gibt aus der Liste dasjenige Snippet als Ergebnis zurück, dessen Intensity
        // am nähesten an der eigenen liegt
        public Segment ReturnSegmentWithLowestIntensityDifference(List<Segment> argSnippets)
        {            
            float lowestIntensityDifference = 1.0f;
            Segment result = null;

            for (int i = 0; i < argSnippets.Count; i++)
            {
                Segment snippet = argSnippets[i];

                if (snippet != this)
                {
                    float diff = System.Math.Abs(snippet.Intensity - this.Intensity);

                    if (diff == 0)
                    {
                        return snippet;
                    }
                    else if (diff < lowestIntensityDifference)
                    {
                        lowestIntensityDifference = diff;
                        result = snippet;
                    }
                }
            }

            return result;
        }




        internal bool CheckIfAnyDirectOrIndirectTransitionIsPossible(Soundtrack soundtrack, int targetThemeId)
        {
            if (CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(soundtrack, targetThemeId) == true)
            {
                return true;
            }
            else
            {
                return MapOfNextTransitionSegmentToTheme.ContainsKey(targetThemeId);
            }
        }



        // returns true if the theme with the given themeId contains at least one Segment
        // that would be a compatible follower / layer of the Segment with the given snippetId.
        // Ignores the Suitabilities of both the Source and Target Segments.
        public bool CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(Soundtrack soundtrack, int targetThemeId)
        {

            if (_mapDirectTransitionToThemeIsPossible.ContainsKey(targetThemeId))
            {
                return _mapDirectTransitionToThemeIsPossible[targetThemeId];
            }
            else
            {
                foreach (Follower follower in Followers)
                {
                    Segment followerSnippet = soundtrack.GetSegmentById(follower.snippetId);

                    if (followerSnippet.ThemeId == targetThemeId)
                    {
                        _mapDirectTransitionToThemeIsPossible[targetThemeId] = true;
                        return true;
                    }                     
                }
            }
            _mapDirectTransitionToThemeIsPossible[targetThemeId] = false;
            return false;
        }



        public static string GetStringFromSegmentSuitabilities(int snippetTypeBitfield)
        {           
            StringBuilder sb = new StringBuilder(20);
            sb.Append(("[ "));

		    if (snippetTypeBitfield == 0)
		    {
			    sb.Append("NULL ");
		    }

		    if (((int)snippetTypeBitfield & (int)SegmentSuitability.start) > 0)
		    {
			    sb.Append("START ");
		    }

		    if (((int)snippetTypeBitfield & (int)SegmentSuitability.middle) > 0)
		    {
			    sb.Append("MIDDLE ");
		    }

            if (((int)snippetTypeBitfield & (int)SegmentSuitability.bridge) > 0)
            {
                sb.Append("BRIDGE ");
            }

            if (((int)snippetTypeBitfield & (int)SegmentSuitability.end) > 0)
		    {
			    sb.Append("END ");
		    }

            sb.Append("]");

		    return sb.ToString();
        }
				
    }
}
