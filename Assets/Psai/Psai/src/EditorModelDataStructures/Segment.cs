using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using psai.Editor;
using System.Xml.Serialization;

using psai.net;

namespace psai.Editor
{

    [Serializable]
    public class Segment : PsaiMusicEntity, ICloneable
    {
        private static int DEFAULT_SNIPPET_TYPES = ((int)SegmentSuitability.start | (int)SegmentSuitability.middle);
        private static float DEFAULT_INTENSITY = 0.5f;

        private static float COMPATIBILITY_PERCENTAGE_SAME_GROUP = 1.0f;
        private static float COMPATIBILITY_PERCENTAGE_OTHER_GROUP = 0.5f;
    	
	    private float _intensity;

        // the _compatibleSnippetsIds Dictionary is only used for import and export
        [NonSerialized]     // hopefully helps to prevent the serialization error message in Unity 4.5
        private Dictionary<Int32, float> _compatibleSnippetsIds = new Dictionary<Int32, float>();			// complete Map of compatible Segments
        //--

        [NonSerialized]   // hopefully helps to prevent the serialization error message in Unity 4.5
        private HashSet<Segment> _manuallyLinkedSnippets = new HashSet<Segment>();
        [NonSerialized]   // hopefully helps to prevent the serialization error message in Unity 4.5
        private HashSet<Segment> _manuallyBlockedSnippets = new HashSet<Segment>();

        #region Properties

        public int Id
        {
            get;
            set;
        }


        public bool IsAutomaticBridgeSegment
        {
            get;
            set;
        }

        public float Intensity
        {
            get { return _intensity; }
            set { 
                    if (value >= 0.0f && value <= 1.0f)
                        _intensity = value; 
                }
        }

        public bool IsUsableAtStart
        {
            get;
            set;
        }

        public bool IsUsableInMiddle
        {
            get;
            set;

            /*
            get { return ((int)(this.SnippetTypeBitfield & (int)SegmentSuitability.middle) > 0); }
            set
            {
                if (value == true)
                {
                    SetSnippetTypeFlag(SegmentSuitability.middle);
                }
                else
                {
                    ClearSnippetTypeFlag(SegmentSuitability.middle);
                }

            }
             */
        }

        public bool IsUsableAtEnd
        {
            get;
            set;

            /*
            get { return ((int)(this.SnippetTypeBitfield & (int)SegmentSuitability.end) > 0); }
            set
            {
                if (value == true)
                {
                    SetSnippetTypeFlag(SegmentSuitability.end);
                }
                else
                {
                    ClearSnippetTypeFlag(SegmentSuitability.end);
                }
            } 
             */
        }

        public AudioData AudioData
        {
            get;
            set;
        }

        // Redirecting properties to this.AudioData.
        // Watch out, we need these in conjunction with "segment.GetPropertyInfo()" !
        public bool CalculatePostAndPrebeatLengthBasedOnBeats
        {
            get { return this.AudioData.CalculatePostAndPrebeatLengthBasedOnBeats; }
            set { this.AudioData.CalculatePostAndPrebeatLengthBasedOnBeats = value; }
        }
        public int PreBeatLengthInSamples
        {
            get { return this.AudioData.PreBeatLengthInSamples; }
            set { this.AudioData.PreBeatLengthInSamples = value; }
        }
        public int PostBeatLengthInSamples
        {
            get { return this.AudioData.PostBeatLengthInSamples; }
            set { this.AudioData.PostBeatLengthInSamples = value; }
        }
        public float PreBeats
        {
            get { return this.AudioData.PreBeats; }
            set { this.AudioData.PreBeats = value; }
        }
        public float PostBeats
        {
            get { return this.AudioData.PostBeats; }
            set { this.AudioData.PostBeats = value; }
        }
        public float Bpm
        {
            get { return this.AudioData.Bpm; }
            set { this.AudioData.Bpm = value; }
        }
        public int SampleRate
        {
            get { return this.AudioData.SampleRate; }
            set { this.AudioData.SampleRate = value; }
        }
        public int TotalLengthInSamples
        {
            get { return this.AudioData.TotalLengthInSamples; }
            //set { this.AudioData.GetTotalLengthInSamples() = value; }
        }
        public int BitsPerSample
        {
            get { return AudioData.BitsPerSample; }
            set { AudioData.BitsPerSample = value; }
        }
        // ----------- end of redirecting properties to this.AudioData------------


  

        // id of the parent Theme
        public int ThemeId
        {
            get;
            set;
        }

        public List<int> Serialization_ManuallyBlockedSegmentIds
        {
            get;
            set;
        }

        public List<int> Serialization_ManuallyLinkedSegmentIds
        {
            get;
            set;
        }

        // usually this allowed_implicitly
        public CompatibilityType DefaultCompatibiltyAsFollower
        {
            get;
            set;
        }

        public override List<PsaiMusicEntity> GetChildren()
        {
            return null;
        }


        // the parent Group
        [XmlIgnoreAttribute()]
        public Group Group
        {
            get;
            set;
        }

        [XmlIgnoreAttribute()]
        public HashSet<Segment> ManuallyLinkedSnippets
        {
            set { _manuallyLinkedSnippets = value; }
            get { return _manuallyLinkedSnippets; }
        }

        [XmlIgnoreAttribute()]
        public HashSet<Segment> ManuallyBlockedSnippets
        {
            set { _manuallyBlockedSnippets = value; }
            get { return _manuallyBlockedSnippets; }
        }

        [XmlIgnoreAttribute()]
        public Dictionary<Int32, float> CompatibleSnippetsIds
        {
            get { return _compatibleSnippetsIds; }
        }

        #endregion

        public override string GetClassString()
        {
            return "Segment";
        }

        #region ctors
        public Segment()
        {
            init();
        }

        public Segment(int id, String name, int snippetTypes, float intensity)
        {
            init();
            Id = id;

            SetStartMiddleEndPropertiesFromBitfield(snippetTypes);

            Name = name;
            Intensity = intensity;
        }

        // creates a snippet along with its new AudioData object based on the filename
        public Segment(int id, AudioData audioData)
        {
            init();
            this.AudioData = audioData;
            Id = id;
            this.Name = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetFileName(audioData.FilePathRelativeToProjectDir));
        }
        #endregion

        private void init()
        {
            Id = 1;
            Name = "new segment";
            DefaultCompatibiltyAsFollower = CompatibilityType.allowed_implicitly;
            SetStartMiddleEndPropertiesFromBitfield(DEFAULT_SNIPPET_TYPES);
            Intensity = DEFAULT_INTENSITY;
            this.AudioData = new AudioData();
        }

        public override object Clone()
        {
            Segment newSnippet = (Segment) this.MemberwiseClone();
            newSnippet.AudioData = (AudioData)this.AudioData.Clone();
            newSnippet.ManuallyBlockedSnippets = new HashSet<Segment>();
            newSnippet.ManuallyLinkedSnippets = new HashSet<Segment>();

            foreach (Segment blockedSnippet in this._manuallyBlockedSnippets)
                newSnippet._manuallyBlockedSnippets.Add(blockedSnippet);

            foreach (Segment linkedSnippet in this._manuallyLinkedSnippets)
                newSnippet._manuallyLinkedSnippets.Add(linkedSnippet);

            return newSnippet;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Segment '");
            sb.Append(Name);
            sb.Append("'");

            return sb.ToString();
        }
    

        public bool AddCompatibleSnippet(Segment snippet, float compatibility)
	    {
		    if (snippet != null && compatibility >= 0.0f && compatibility <= 1.0f)
		    {
			    _compatibleSnippetsIds[snippet.Id] = compatibility;
                //Console.WriteLine("Snippet.addCompatibleSnippet() {0} is now a compatible follower to {1} , compatibility={2} ", targetSnippet.Id, this.Id, compatibility);
			    return true;
		    }
		    return false;
	    }

        public override bool PropertyDifferencesAffectCompatibilities(PsaiMusicEntity otherEntity)
        {
            if (otherEntity is Segment)
            {
                Segment otherSnippet = otherEntity as Segment;

                if (this.IsUsableAtStart.Equals(otherSnippet.IsUsableAtStart) == false)
                {
                    return true;
                }
                if (this.IsUsableInMiddle.Equals(otherSnippet.IsUsableInMiddle) == false)
                {
                    return true;
                }
                if (this.IsUsableAtEnd.Equals(otherSnippet.IsUsableAtEnd) == false)
                {
                    return true;
                }

                if (this.IsAutomaticBridgeSegment.Equals(otherSnippet.IsAutomaticBridgeSegment) == false)
                {
                    return true;
                }

                if (this.DefaultCompatibiltyAsFollower.Equals(otherSnippet.DefaultCompatibiltyAsFollower) == false)
                {
                    return true;
                }
            }

            return false;
        }

        // This method will create the HashSet of compatibleSnippets based on the ManuallyBlocked / ManuallyLinked entities.
        // The CompatibleSnippets HashSet is only needed for import, or to build the .pcb file. 
        public void BuildCompatibleSegmentsSet(PsaiProject project)
        {
            HashSet<Segment> allSegments = project.GetSegmentsOfAllThemes();

            CompatibleSnippetsIds.Clear();
            foreach (Segment targetSegment in allSegments)
            {
                bool compatible = false;
                CompatibilityReason reason = CompatibilityReason.not_set;
                CompatibilityType compatibilityType = this.GetCompatibilityType(targetSegment, out reason);

                switch (compatibilityType)
                {
                    /*
                    case CompatibilityType.allowed_always:
                        compatible = true;
                        break;
                    */

                    case CompatibilityType.allowed_implicitly:
                        compatible = true;
                        break;

                    case CompatibilityType.allowed_manually:
                        compatible = true;
                        break;

                    default:
                        compatible = false;
                        break;
                }

                if (compatible)
                {
                    float compatibility;
                    if (targetSegment.Group == this.Group)
                        compatibility = COMPATIBILITY_PERCENTAGE_SAME_GROUP;
                    else
                        compatibility = COMPATIBILITY_PERCENTAGE_OTHER_GROUP;

                    this.AddCompatibleSnippet(targetSegment, compatibility);
                }             
            }
        }

        public void SetStartMiddleEndPropertiesFromBitfield(int bitfield)
        {            
            this.IsUsableAtStart = ReadOutSegmentSuitabilityFlag(bitfield, SegmentSuitability.start);
            this.IsUsableInMiddle = ReadOutSegmentSuitabilityFlag(bitfield, SegmentSuitability.middle);
            this.IsUsableAtEnd = ReadOutSegmentSuitabilityFlag(bitfield, SegmentSuitability.end);
            
            // we cannot determine if "bridge" flag means "auto bridge" or manual bridge for a certain group, so we leave it out completely
        }

        
        public int CreateSegmentSuitabilityBitfield(PsaiProject parentProject)
        {
            int bitfield = 0;

            if (this.IsAutomaticBridgeSegment || this.IsBridgeSnippetToAnyGroup(parentProject))
            {
                SetSegmentSuitabilityFlag(ref bitfield, SegmentSuitability.bridge);
            }

            if (this.IsUsableAtStart)
            {
                SetSegmentSuitabilityFlag(ref bitfield, SegmentSuitability.start);
            }

            if (this.IsUsableInMiddle)
            {
                SetSegmentSuitabilityFlag(ref bitfield, SegmentSuitability.middle);
            }

            if (this.IsUsableAtEnd)
            {
                SetSegmentSuitabilityFlag(ref bitfield, SegmentSuitability.end);
            }

            return bitfield;
        }


        
        public psai.net.Segment CreatePsaiDotNetVersion(PsaiProject parentProject)
        {
            psai.net.Segment netSegment = new psai.net.Segment();


            netSegment.audioData = this.AudioData.CreatePsaiDotNetVersion();
            netSegment.Id = this.Id;
            netSegment.Intensity = this.Intensity;
            netSegment.SnippetTypeBitfield = this.CreateSegmentSuitabilityBitfield(parentProject);
            netSegment.ThemeId = this.ThemeId;
            netSegment.Name = this.Name;

            for (int i = 0; i < this.CompatibleSnippetsIds.Count; i++)
            {
                int snippetId = this.CompatibleSnippetsIds.Keys.ElementAt(i);
                netSegment.Followers.Add(new Follower(snippetId, CompatibleSnippetsIds[snippetId]));
            }

            return netSegment;
        }
        

        public ProtoBuf_Snippet CreateProtoBuf(PsaiProject parentProject)
        {
            ProtoBuf_Snippet pbSnippet = new ProtoBuf_Snippet();
           
            pbSnippet.audio_data = this.AudioData.CreateProtoBuf();             
            pbSnippet.id = this.Id;
            pbSnippet.intensity = this.Intensity;
            pbSnippet.snippet_type = this.CreateSegmentSuitabilityBitfield(parentProject);
            pbSnippet.theme_id = this.ThemeId;
            pbSnippet.name = this.Name;

            for (int i = 0; i < this.CompatibleSnippetsIds.Count; i++)
            {
                int snippetId = this.CompatibleSnippetsIds.Keys.ElementAt(i);
                pbSnippet.follower_id.Add(snippetId);
                pbSnippet.follower_compatibility.Add(this.CompatibleSnippetsIds[snippetId]);
            }

            return pbSnippet;
        }



        public bool HasOnlyStartSuitability()
        {
            return (this.IsUsableAtStart && !this.IsUsableInMiddle && !this.IsUsableAtEnd);
        }

        public bool HasOnlyMiddleSuitability()
        {
            return (!this.IsUsableAtStart && this.IsUsableInMiddle && !this.IsUsableAtEnd);
        }

        public bool HasOnlyEndSuitability()
        {
            return (!this.IsUsableAtStart && !this.IsUsableInMiddle && this.IsUsableAtEnd);
        }


        public static bool ReadOutSegmentSuitabilityFlag(int bitfield, SegmentSuitability suitability)
        {
            return ((int)(bitfield & (int)suitability) > 0);
        }

        public static void SetSegmentSuitabilityFlag(ref int bitfield, SegmentSuitability snippetType)
        {
            int argAsBitmask = (int)snippetType;
            bitfield = bitfield | argAsBitmask;
        }

        public static void ClearSegmentSuitabilityFlag(ref int bitfield, SegmentSuitability snippetType)
        {
            int argAsBitmask = (int)snippetType;
            bitfield = bitfield & ~argAsBitmask;
        }

        /*
        public void ToggleSnippetTypeFlag(SegmentSuitability snippetType)
        {

            #if DEBUG
                Console.WriteLine("ToggleSnippetTypeFlag() " + snippetType);
                Console.WriteLine("before: start=" + IsUsableAtStart + "  middle=" + IsUsableInMiddle + "  end=" + IsUsableAtEnd);
            #endif

            if (this.IsUsableAs(snippetType))
            {
                // remove the flag
                this.ClearSnippetTypeFlag(snippetType);                
            }
            else
            {
                 //set the flag
                this.SetSnippetTypeFlag(snippetType);
            }

            #if DEBUG
                Console.WriteLine("after: start=" + IsUsableAtStart + "  middle=" + IsUsableInMiddle + "  end=" + IsUsableAtEnd);
            #endif
        }
        */

        public bool IsBridgeSnippetToAnyGroup(PsaiProject project)
        {
            List<Group> groups = null;
            return (this.IsAutomaticBridgeSegment || project.CheckIfSnippetIsManualBridgeSnippetToAnyGroup(this, false, out groups));
        }

        public bool IsManualBridgeSnippetForAnyGroup(PsaiProject project)
        {
            List<Group> groups = null;
            return project.CheckIfSnippetIsManualBridgeSnippetToAnyGroup(this, false, out groups);
        }


        public bool IsManualBridgeSegmentForSourceGroup(Group sourceGroup)
        {
            return (sourceGroup.ManualBridgeSnippetsOfTargetGroups.Contains(this));
        }

        public override CompatibilitySetting GetCompatibilitySetting(PsaiMusicEntity targetEntity)
        {
            if (targetEntity is Segment)
            {
                if (ManuallyBlockedSnippets.Contains((Segment)targetEntity))
                {
                    return CompatibilitySetting.blocked;
                }
                else if (ManuallyLinkedSnippets.Contains((Segment)targetEntity))
                {
                    return CompatibilitySetting.allowed;
                }
            }
            return CompatibilitySetting.neutral;
        }


        // this method returns the compatibility type of this Snippet to a following Snippet, based
        // on the compatibility flowchart in the "doc_internal" folder of this project.
        public override CompatibilityType GetCompatibilityType(PsaiMusicEntity targetEntity, out CompatibilityReason reason)
        {
            reason = CompatibilityReason.not_set;

            if (targetEntity is Segment)
            {
                Segment targetSegment = (Segment)targetEntity;
                Group sourceGroup = this.Group;
                Group targetGroup = targetSegment.Group;


                if (sourceGroup.GetCompatibilityType(targetGroup, out reason) == CompatibilityType.logically_impossible)
                {
                    return CompatibilityType.logically_impossible;
                }
                else
                {
                    // step 1
                    // a pure END-Segment can never follow another pure END-Segment
                    if (this.HasOnlyEndSuitability())
                    {
                        if (targetSegment.HasOnlyEndSuitability())
                        {
                            reason = CompatibilityReason.target_segment_and_source_segment_are_both_only_usable_at_end;
                            return CompatibilityType.logically_impossible;
                        }
                    }


                    // step 2                    
                    // transitioning to a pure END-Segment (which is no BridgeSnippet) of some other Group will never happen
                    if (    sourceGroup != targetGroup 
                            && targetSegment.HasOnlyEndSuitability()
                            && !targetSegment.IsAutomaticBridgeSegment 
                            && !targetSegment.IsManualBridgeSegmentForSourceGroup(sourceGroup)
                        )
                    {
                        reason = CompatibilityReason.target_segment_is_of_a_different_group_and_is_only_usable_at_end;
                        return CompatibilityType.logically_impossible;
                    }
                    

                    // step 3
                    // transitioning to a pure Bridge-Snippet within the same Group is not allowed
                    if ( sourceGroup == targetGroup)
                    {
                        if ( !targetSegment.IsUsableInMiddle && !targetSegment.IsUsableAtEnd
                            && (targetSegment.IsAutomaticBridgeSegment || targetSegment.IsManualBridgeSegmentForSourceGroup(sourceGroup)))
                        {
                            reason = CompatibilityReason.target_segment_is_a_pure_bridge_segment_within_the_same_group;
                            return CompatibilityType.blocked_implicitly;
                        }

                    }


                    // step 4
                    if (ManuallyLinkedSnippets.Contains(targetSegment))
                    {
                        reason = CompatibilityReason.manual_setting_within_same_hierarchy;
                        return CompatibilityType.allowed_manually;
                    }

                    // step 5
                    if (ManuallyBlockedSnippets.Contains(targetSegment))
                    {
                        reason = CompatibilityReason.manual_setting_within_same_hierarchy;
                        return CompatibilityType.blocked_manually;
                    }


                    // step 6
                    // In case of group-transitions: Is there a BridgeSnippet in the targetGroup for the SourceGroup? Then block the TargetSnippet implicitly, if it is no BridgSnippet from the SourceGroup to TargetGroup.
                    // Otherwise allow.
                    if (sourceGroup != null && sourceGroup != targetGroup)
                    {
                        //if (EditorModel.Instance.Project.CheckIfThereIsAtLeastOneBridgeSnippetFromSourceGroupToTargetGroup(sourceGroup, targetGroup))
                        if (targetGroup.ContainsAtLeastOneAutomaticBridgeSegment() || targetGroup.ContainsAtLeastOneManualBridgeSegmentForSourceGroup(sourceGroup))
                        {
                            if (targetSegment.IsManualBridgeSegmentForSourceGroup(sourceGroup))
                            {
                                reason = CompatibilityReason.target_segment_is_a_manual_bridge_segment_for_the_source_group;
                                return CompatibilityType.allowed_manually;
                            }

                            if (targetSegment.IsAutomaticBridgeSegment)
                            {
                                reason = CompatibilityReason.target_segment_is_an_automatic_bridge_segment;
                                return CompatibilityType.allowed_implicitly;
                            }

                            // block all non-bridge-snippets:
                            reason = CompatibilityReason.target_group_contains_at_least_one_bridge_segment;
                            return CompatibilityType.blocked_implicitly;
                        }
                    }


                    // step
                    // Anything is implicitly allowed after a pure END-Segment
                    if (this.HasOnlyEndSuitability())
                    {
                        reason = CompatibilityReason.anything_may_be_played_after_a_pure_end_segment;
                        return CompatibilityType.allowed_implicitly;
                    }


                    // step 
                    // if the default compatibility of the targetSegment is allowed_implicitly, continue evaluation by regarding the parent group
                    if (targetSegment.DefaultCompatibiltyAsFollower == CompatibilityType.allowed_implicitly)
                    {
                        CompatibilityType groupCompatibility = this.Group.GetCompatibilityType(targetSegment.Group, out reason);
                        {
                            switch (groupCompatibility)
                            {
                                case CompatibilityType.blocked_manually:
                                    reason = CompatibilityReason.manual_setting_of_parent_entity;
                                    return CompatibilityType.blocked_implicitly;

                                case CompatibilityType.blocked_implicitly:
                                    return CompatibilityType.blocked_implicitly;

                                case CompatibilityType.allowed_manually:
                                    reason = CompatibilityReason.manual_setting_of_parent_entity;
                                    return CompatibilityType.allowed_implicitly;

                                default:
                                    reason = CompatibilityReason.inherited_from_parent_hierarchy;
                                    return CompatibilityType.allowed_implicitly;
                            }
                        }
                    }
                    else
                    {
                        reason = CompatibilityReason.default_compatibility_of_the_target_segment_as_a_follower;
                        return targetSegment.DefaultCompatibiltyAsFollower;
                    }
                }           
            }

            return CompatibilityType.undefined;                       
        }

        public override PsaiMusicEntity GetParent()
        {
            return this.Group;
        }


        public override int GetIndexPositionWithinParentEntity(PsaiProject parentProject)
        {
            return this.Group.Segments.IndexOf(this);
        }

        /*
        public bool Equals(Snippet other)
        {            
            return (this.Id == other.Id && this.Name.Equals(other.Name) && this.Intensity.Equals(other.Intensity) && this.SnippetType.Equals(other.SnippetType));
        }
         */

        #region statics
        public static Segment GetExampleSnippet1()
        {
            Segment snippet = new Segment();
            snippet.Name = "snippet1";
            snippet.Intensity = 0.5f;
            return snippet;
        }

        public static Segment GetExampleSnippet2()
        {
            Segment snippet = new Segment();
            snippet.Name = "snippet2";
            snippet.Intensity = 0.6f;
            return snippet;
        }

        public static Segment GetExampleSnippet3()
        {
            Segment snippet = new Segment();
            snippet.Name = "snippet3";
            snippet.Intensity = 0.7f;
            return snippet;
        }

        public static Segment GetExampleSnippet4()
        {
            Segment snippet = new Segment();
            snippet.Name = "snippet4";
            snippet.Intensity = 0.7f;
            return snippet;
        }
        #endregion

        #region ICloneable Members

        #endregion
    }
}
