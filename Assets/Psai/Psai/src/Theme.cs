//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace psai.net
{

    /// <summary>
    /// Defines the playback priority and general playback behavior of a Theme.
    /// </summary>
    public enum ThemeType
	{
        /// <summary>
        /// not yet initialized
        /// </summary>
		none = 0,

        /// <summary>
        /// Basic Moods are for common background music when nothing special is happening. 
        /// </summary>
        /// <remarks>
        /// Basic Moods will in turns play for some time and then again keep silent for some time, without the need for triggering it again.
        /// Use PsaiCore.StopMusic() to keep silence until any Theme is triggered again.
        /// </remarks>
		basicMood = 1,                  

        /// <summary>
        /// Basic Mood Alterations will not interrupt a Basic Mood immediately, and will return to the previous Basic Mood.
        /// </summary>
		basicMoodAlt = 2,				

        /// <summary>
        /// Dramatic Events always interrupt Basic Moods (Alterations) immediately, and will return to last Basic Mood.
        /// </summary>
		dramaticEvent = 7,

        /// <summary>
        /// Action Events interrupt Basic Moods (Alterations) immediately. Use these for battle music when the player is suddently attacked.
        /// </summary>
		action = 3,                

        /// <summary>
        /// Shock Events will interrupt Action Events immediately and will afterwards return to Theme that was interrupted.
        /// </summary>
        /// <remarks>
        /// Use Shock Events for sudden surprises during an Action Event, like when the boss enemy suddenly appears.
        /// </remarks>
		shock = 5,

        /// <summary>
        /// Highlight Layers are not really a Theme but used for short Segments that will be layered unsynchronized above the current Segment, if marked as compatible.
        /// </summary>
		highlightLayer = 6

	};


    public enum ThemeInterruptionBehavior
    {
        undefined,
        immediately,
        at_end_of_current_snippet,
        never,
        layer
    }


    public class Weighting
	{
		public float switchGroups;
		public float intensityVsVariety;
		public float lowPlaycountVsRandom;

        internal Weighting()
        {
            intensityVsVariety = 0.5f;
		    lowPlaycountVsRandom = 0.9f;
		    switchGroups = 0.5f;
        }
	};

    public class Theme
    {

        public static bool ThemeInterruptionBehaviorRequiresEvaluationOfSegmentCompatibilities(ThemeInterruptionBehavior interruptionBehavior)
        {
            return (interruptionBehavior == ThemeInterruptionBehavior.immediately || 
                    interruptionBehavior == ThemeInterruptionBehavior.at_end_of_current_snippet ||
                    interruptionBehavior == ThemeInterruptionBehavior.layer ||
                    interruptionBehavior == ThemeInterruptionBehavior.never
                    );
        }


        public static string ThemeTypeToString(ThemeType themeType)
        {
            switch (themeType)
            {
                case ThemeType.basicMood:
                    return "Basic Mood";

                case ThemeType.basicMoodAlt:
                    return "Mood Alteration";

                case ThemeType.dramaticEvent:
                    return "Dramatic Event";

                case ThemeType.action:
                    return "Action";

                case ThemeType.shock:
                    return "Shock";

                case ThemeType.highlightLayer:
                    return "Highlight Layer";
            }

            return "";
        }




        /* Returns what will happen if sourceThemeType is playing while the targetThemeType is triggered. */
        public static ThemeInterruptionBehavior GetThemeInterruptionBehavior(ThemeType sourceThemeType, ThemeType targetThemeType)
        {
		switch (sourceThemeType)
		    {
		    case ThemeType.basicMood:
			    {
				    switch (targetThemeType)
				    {
				    case ThemeType.basicMood:
					    return ThemeInterruptionBehavior.at_end_of_current_snippet;

				    case ThemeType.basicMoodAlt:
					    return ThemeInterruptionBehavior.at_end_of_current_snippet;

				    case ThemeType.dramaticEvent:
					    return ThemeInterruptionBehavior.immediately;

				    case ThemeType.action:					
					    return ThemeInterruptionBehavior.immediately;

				    case ThemeType.shock:
					    return ThemeInterruptionBehavior.immediately;

				    case ThemeType.highlightLayer:
					    return ThemeInterruptionBehavior.layer;

				    }
				    break;
			    }
			    //break;

		    case ThemeType.basicMoodAlt:
			    {
				    switch (targetThemeType)
				    {
				    case ThemeType.basicMood:
					    return ThemeInterruptionBehavior.never;

				    case ThemeType.basicMoodAlt:
					    return ThemeInterruptionBehavior.at_end_of_current_snippet;

				    case ThemeType.dramaticEvent:
					    return ThemeInterruptionBehavior.immediately;

				    case ThemeType.action:					
				    //case ThemeType.contAction:
					    return ThemeInterruptionBehavior.immediately;

				    case ThemeType.shock:
					    return ThemeInterruptionBehavior.immediately;

				    case ThemeType.highlightLayer:
					    return ThemeInterruptionBehavior.layer;

				    }
				    break;
			    }
			    //break;

		    case ThemeType.dramaticEvent:
			    {
				    switch (targetThemeType)
				    {
				    case ThemeType.basicMood:
					    return ThemeInterruptionBehavior.never;

				    case ThemeType.basicMoodAlt:
					    return ThemeInterruptionBehavior.never;

				    case ThemeType.dramaticEvent:
					    return ThemeInterruptionBehavior.at_end_of_current_snippet;

				    case ThemeType.action:					
					    return ThemeInterruptionBehavior.immediately;

				    case ThemeType.shock:
					    return ThemeInterruptionBehavior.immediately;

				    case ThemeType.highlightLayer:
					    return ThemeInterruptionBehavior.layer;

				    }
				    break;
			    }
			    //break;

		    case ThemeType.action:
			    {
				    switch (targetThemeType)
				    {
				    case ThemeType.basicMood:
					    return ThemeInterruptionBehavior.never;

				    case ThemeType.basicMoodAlt:
					    return ThemeInterruptionBehavior.never;

				    case ThemeType.dramaticEvent:
					    return ThemeInterruptionBehavior.never;

				    case ThemeType.action:					
					    return ThemeInterruptionBehavior.at_end_of_current_snippet;

				    case ThemeType.shock:
					    return ThemeInterruptionBehavior.immediately;

				    case ThemeType.highlightLayer:
					    return ThemeInterruptionBehavior.layer;

				    }
				    break;
			    }
			    //break;

		    case ThemeType.shock:
			    {
				    switch (targetThemeType)
				    {
				    case ThemeType.basicMood:
					    return ThemeInterruptionBehavior.never;

				    case ThemeType.basicMoodAlt:
					    return ThemeInterruptionBehavior.never;

				    case ThemeType.dramaticEvent:
					    return ThemeInterruptionBehavior.never;

				    case ThemeType.action:					
					    return ThemeInterruptionBehavior.never;

				    case ThemeType.shock:
					    return ThemeInterruptionBehavior.immediately;

				    case ThemeType.highlightLayer:
					    return ThemeInterruptionBehavior.layer;

				    }
				    break;
			    }
			    //break;

		    case ThemeType.highlightLayer:
			    {
				    return ThemeInterruptionBehavior.never;
			    }
			    //break;

		    }

		    return ThemeInterruptionBehavior.undefined;
        }

		public int id;
        public string Name;
        public ThemeType themeType;

        public int priority;

        public int restSecondsMax;
        public int restSecondsMin;

        public List<Segment> m_segments;

        public float intensityAfterRest;
        public int musicDurationGeneral;
        public int musicDurationAfterRest;

        public Weighting weightings;


        public Theme()
        {
            m_segments = new List<Segment>();
            weightings = new Weighting();

            this.id = -1;
            this.restSecondsMax = 0;
            this.restSecondsMin = 0;
            this.priority = 0;
            this.themeType = ThemeType.none;
            this.Name = "";
        }

        internal Theme(psai.ProtoBuf_Theme pbTheme) : this()
        {
            this.id = pbTheme.id;
            this.intensityAfterRest = pbTheme.intensityAfterRest;
            this.musicDurationAfterRest = pbTheme.musicPhaseSecondsAfterRest;
            this.musicDurationGeneral = pbTheme.musicPhaseSecondsGeneral;
            this.Name = pbTheme.name;
            this.priority = pbTheme.priority;
            this.restSecondsMin = pbTheme.restSecondsMin;
            this.restSecondsMax = pbTheme.restSecondsMax;
            this.themeType = (ThemeType)(pbTheme.themeType);

            this.weightings.intensityVsVariety = pbTheme.weightingIntensityVsVariety;
            this.weightings.lowPlaycountVsRandom = pbTheme.weightingPlaycountVsRandom;
            this.weightings.switchGroups = pbTheme.weightingSwitchGroups;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            sb.Append(" (");
            sb.Append(id);
            sb.Append(")");
            sb.Append(" [");
            sb.Append(themeType);
            sb.Append("]");

            return sb.ToString();
        }



        /* This method is responsible for linking each Segment of a Theme to an END-Snippet
        *  in the shortest path possible. If multiple compatible Segments exist, the one with the
        *  best-matching intensity will be chosen.
        *  After the algorithm has completed, each Snippet's
        *  member 'nextSnippetToShortestEndSequence' will hold the next Snippet to follow this path,
        *  or NULL if no transition is possible.
        */
        internal void BuildSequencesToEndSegmentForAllSnippets()
        {
            foreach (Segment snippet in m_segments)
            {
                snippet.nextSnippetToShortestEndSequence = null;
            }

            List<Segment> snippetsAddedInLastTier = new List<Segment>();            

            foreach (Segment snippet in m_segments)
            {
                if ((snippet.SnippetTypeBitfield & (int)SegmentSuitability.end) > 0)
                {
                    snippetsAddedInLastTier.Add(snippet);
                }
            }
           
            SetTheNextSnippetToShortestEndSequenceForAllSourceSnippetsOfTheSnippetsInThisList(snippetsAddedInLastTier.ToArray());

            #if !(PSAI_NOLOG)
            {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("BuildSequencesToEndSnippetForAllSnippets completed for Theme ");
	                sb.Append(this.Name);
	                sb.Append("\n");
	                foreach (Segment snippet in m_segments)
	                {
	                    sb.Append(snippet.Name);
	                    sb.Append(" -> ");
	
	                    if (snippet.nextSnippetToShortestEndSequence == null)
	                    {
	                        sb.Append(" null");
	                    }
	                    else
	                    {
	                        sb.Append(snippet.nextSnippetToShortestEndSequence.Name);
	                    }
	                    sb.Append("\n");
	                }
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
            }
            #endif
        }

        /** 
         * @param listOfSnippetsWithValidEndSequences - a list of Snippets that are either END-Snippets or have a valid NextSnippetToShortestEndSequence set.
         * returns the array of Snippets for which a valid NextSnippetToValidEndSequence has been set in this call.
         */
        private void SetTheNextSnippetToShortestEndSequenceForAllSourceSnippetsOfTheSnippetsInThisList(Segment[] listOfSnippetsWithValidEndSequences)
        {
            /*
            #if !(PSAI_NOLOG)
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                Logger.Instance.Log("SetTheNextSnippetToShortestEndSequenceForAllSourceSnippetsOfTheSnippetsInThisList() called, listOfSnippets.Length=" + listOfSnippetsWithValidEndSequences.Length, LogLevel.debug);
	                StringBuilder sb = new StringBuilder();
	                foreach (Snippet argSnippet in listOfSnippetsWithValidEndSequences)
	                {
	                    sb.Append(argSnippet.name);
	                    sb.Append(" intensity=");
	                    sb.Append(argSnippet.intensity);
	                    sb.Append(" nextSnippetToShEndSeq=");
	                    sb.Append(argSnippet.nextSnippetToShortestEndSequence);
	                    sb.Append("    ");
	                }
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
            #endif
            */

            Dictionary<Segment, List<Segment>> mapWaypointAlternativesForSnippet = new Dictionary<Segment, List<Segment>>();
            foreach (Segment endSnippet in listOfSnippetsWithValidEndSequences)
            {
                List<Segment> sourceSnippets = GetSetOfAllSourceSegmentsCompatibleToSegment(endSnippet, Logik.COMPATIBILITY_PERCENTAGE_SAME_GROUP, SegmentSuitability.end);

                foreach (Segment sourceSnippet in sourceSnippets)
                {
                    if (sourceSnippet.nextSnippetToShortestEndSequence == null && sourceSnippet.ThemeId == endSnippet.ThemeId)
                    {
                        if (!mapWaypointAlternativesForSnippet.ContainsKey(sourceSnippet))
                        {
                            mapWaypointAlternativesForSnippet[sourceSnippet] = new List<Segment>();
                        }
                        mapWaypointAlternativesForSnippet[sourceSnippet].Add(endSnippet);
                    }
                }
            }

            foreach (Segment snippet in mapWaypointAlternativesForSnippet.Keys)
            {
                snippet.nextSnippetToShortestEndSequence = snippet.ReturnSegmentWithLowestIntensityDifference(mapWaypointAlternativesForSnippet[snippet]);
            }

            Segment[] snippetsAdded = new Segment[mapWaypointAlternativesForSnippet.Count];
            mapWaypointAlternativesForSnippet.Keys.CopyTo(snippetsAdded, 0);

            if (snippetsAdded.Length > 0)
            {
                SetTheNextSnippetToShortestEndSequenceForAllSourceSnippetsOfTheSnippetsInThisList(snippetsAdded);
            }           
        }


        /* This method is responsible to link each Snippet of a theme to the Target Theme
        *  in the shortest path possible. If multiple compatible Segments exist, the one with the
        *  best-matching intensity will be chosen.
        *  After the algorithm has finished, each Snippet's _mapCompatibleSegmentsToTheme will hold
        *  the Segment to be played next when a Theme Transition is in progress, or NULL
        *  if either no transition is possible at all, or if a direct transition is possible.
        */
        internal void BuildSequencesToTargetThemeForAllSegments(Soundtrack soundtrack, Theme targetTheme)
        {
            foreach (Segment snippet in m_segments)
            {
                snippet.MapOfNextTransitionSegmentToTheme.Remove(targetTheme.id);
            }

            List<Segment> snippetsAddedInLastTier = new List<Segment>();
            foreach (Segment snippet in m_segments)
            {
                if (snippet.CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(soundtrack, targetTheme.id) == true)
                {
                    snippetsAddedInLastTier.Add(snippet);
                }
            }

            SetTheNextSegmentToShortestTransitionSequenceToTargetThemeForAllSourceSegmentsOfTheSegmentsInThisList(snippetsAddedInLastTier.ToArray(), soundtrack, targetTheme);

            #if !(PSAI_NOLOG)
            {
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                StringBuilder sb = new StringBuilder();
	                sb.Append("BuildSequencesToTargetThemeForAllSegments completed for Theme ");
	                sb.Append(this);
	                sb.Append(" to Theme ");
	                sb.Append(targetTheme);
	                sb.Append("\n");
	                foreach (Segment snippet in m_segments)
	                {
	                    sb.Append(snippet);
	                    sb.Append(" -> ");
	
	                    if (snippet.MapOfNextTransitionSegmentToTheme.ContainsKey(targetTheme.id) == false)
	                    {
	                        sb.Append(" DirectTransition:");
	                        sb.Append(snippet.CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(soundtrack, targetTheme.id));
	                    }
	                    else
	                    {
	                        sb.Append(snippet.MapOfNextTransitionSegmentToTheme[targetTheme.id].ToString());
	                    }
	                    sb.Append("\n");
	                }
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
            }
            #endif
        }



        private List<Segment> GetSetOfAllSourceSegmentsCompatibleToSegment(Segment targetSnippet, float minCompatibilityThreshold, SegmentSuitability doNotIncludeSegmentsWithThisSuitability)
        {
            List<Segment> sourceSnippets = new List<Segment>();

            foreach (Segment tmpSnippet in m_segments)
            {
                if (tmpSnippet.IsUsableAs(doNotIncludeSegmentsWithThisSuitability) == false)
                {
                    foreach (Follower tmpFollower in tmpSnippet.Followers)
                    {
                        if (tmpFollower.snippetId == targetSnippet.Id && tmpFollower.compatibility >= minCompatibilityThreshold)
                        {
                            sourceSnippets.Add(tmpSnippet);
                        }
                    }
                }
            }

            return sourceSnippets;
        }




        /** 
         * @param listOfSnippetsWithValidTransitionSequencesToTargetTheme - a list of Snippets that have a valid Transition-Sequence to or directly compatible followers in a given TargetTheme.         
         */
        private void SetTheNextSegmentToShortestTransitionSequenceToTargetThemeForAllSourceSegmentsOfTheSegmentsInThisList(Segment[] listOfSnippetsWithValidTransitionSequencesToTargetTheme, Soundtrack soundtrack, Theme targetTheme)
        {
            /*
            #if !(PSAI_NOLOG)
                if (LogLevel.debug <= Logger.Instance.LogLevel)
                {
	                Logger.Instance.Log("SetTheNextSegmentToShortestTransitionSequenceToTargetThemeForAllSourceSegmentsOfTheSegmentsInThisList() called, listOfSnippets.Length=" + listOfSnippetsWithValidEndSequences.Length, LogLevel.debug);
	                StringBuilder sb = new StringBuilder();
	                foreach (Snippet argSnippet in listOfSnippetsWithValidEndSequences)
	                {
	                    sb.Append(argSnippet.name);
	                    sb.Append(" intensity=");
	                    sb.Append(argSnippet.intensity);
	                    sb.Append(" nextSnippetToShEndSeq=");
	                    sb.Append(argSnippet.nextSnippetToShortestEndSequence);
	                    sb.Append("    ");
	                }
	                Logger.Instance.Log(sb.ToString(), LogLevel.debug);
                }
            #endif
            */

            Dictionary<Segment, List<Segment>> mapWaypointAlternativesForSnippet = new Dictionary<Segment, List<Segment>>();
            foreach (Segment transitionSnippet in listOfSnippetsWithValidTransitionSequencesToTargetTheme)
            {
                List<Segment> sourceSnippets = GetSetOfAllSourceSegmentsCompatibleToSegment(transitionSnippet, Logik.COMPATIBILITY_PERCENTAGE_SAME_GROUP, SegmentSuitability.none);
                sourceSnippets.Remove(transitionSnippet);

                foreach (Segment sourceSnippet in sourceSnippets)
                {
                    if (sourceSnippet.MapOfNextTransitionSegmentToTheme.ContainsKey(targetTheme.id) == false
                        && sourceSnippet.CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(soundtrack, targetTheme.id) == false
                        && sourceSnippet.ThemeId == transitionSnippet.ThemeId)
                    {
                        if (mapWaypointAlternativesForSnippet.ContainsKey(sourceSnippet) == false)
                        {
                            mapWaypointAlternativesForSnippet[sourceSnippet] = new List<Segment>();
                        }
                        mapWaypointAlternativesForSnippet[sourceSnippet].Add(transitionSnippet);
                    }
                }
            }

            foreach (Segment snippet in mapWaypointAlternativesForSnippet.Keys)
            {
                snippet.MapOfNextTransitionSegmentToTheme[targetTheme.id] = snippet.ReturnSegmentWithLowestIntensityDifference(mapWaypointAlternativesForSnippet[snippet]);
            }

            Segment[] snippetsAdded = new Segment[mapWaypointAlternativesForSnippet.Count];
            mapWaypointAlternativesForSnippet.Keys.CopyTo(snippetsAdded, 0);

            if (snippetsAdded.Length > 0)
            {
                SetTheNextSegmentToShortestTransitionSequenceToTargetThemeForAllSourceSegmentsOfTheSegmentsInThisList(snippetsAdded, soundtrack, targetTheme);
            }
        }



		internal psai.ProtoBuf_Theme CreateProtoBufTheme()
        {
	        psai.ProtoBuf_Theme pbTheme = new psai.ProtoBuf_Theme();
	        pbTheme.id = id;
	        pbTheme.name = Name;
	        pbTheme.themeType = (int)(this.themeType);
	        pbTheme.intensityAfterRest = this.intensityAfterRest;
	        pbTheme.musicPhaseSecondsGeneral = this.musicDurationGeneral;
	        pbTheme.musicPhaseSecondsAfterRest = this.musicDurationAfterRest;
	        pbTheme.restSecondsMin = this.restSecondsMin;
	        pbTheme.restSecondsMax = this.restSecondsMax;
	        return pbTheme;
        }
    }
}
