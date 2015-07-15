using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using psai.Editor;
using System.Xml.Serialization;



/************************************************************************/
/* The Theme class reflects the parameters as presented in the Editor.
 * ProtoBuf_Theme reflects the data representation as serialized to the .pcb file.
/************************************************************************/

using psai.net;

namespace psai.Editor
{
    [Serializable]
    public class Theme : PsaiMusicEntity, ICloneable
    {        
        internal static float PLAYCOUNT_VS_RANDOM_WEIGHTING_IF_PLAYCOUNT_PREFERRED = 0.8f;

	    private static readonly string DEFAULT_NAME = "new_theme";
        private static readonly int DEFAULT_PRIORITY = 1;
        private static readonly int DEFAULT_REST_SECONDS_MIN = 30;
        private static readonly int DEFAULT_REST_SECONDS_MAX = 60;
        private static readonly int DEFAULT_FADEOUT_MS = 20;
        private static readonly int DEFAULT_THEME_DURATION_SECONDS = 60;
        private static readonly float DEFAULT_INTENSITY_AFTER_REST = 0.5f;
        private static readonly int DEFAULT_THEME_DURATION_SECONDS_AFTER_REST = 40;
        private static readonly float DEFAULT_WEIGHTING_COMPATIBILITY = 0.5f;
        private static readonly float DEFAULT_WEIGHTING_INTENSITY = 0.5f;
        private static readonly float DEFAULT_WEIGHTING_LOW_PLAYCOUNT_VS_RANDOM = 0.0f;
        private static readonly int DEFAULT_THEMETYPEINT = 1;

        private List<Group> _groups;
        private HashSet<Theme> _manuallyBlockedThemes = new HashSet<Theme>();

        private float _intensityAfterRest;

        public static bool ConvertPlaycountVsRandomWeightingToBooleanPlaycountPreferred(float weightingPlaycountVsRandom)
        {
            return (weightingPlaycountVsRandom >= PLAYCOUNT_VS_RANDOM_WEIGHTING_IF_PLAYCOUNT_PREFERRED);
        }


        public override string GetClassString()
        {

            if (this.ThemeTypeInt == (int)ThemeType.highlightLayer)
            {
                return "Highlight Layer";
            }
            else
            {
                return "Theme";
            }
            
        }

        public override List<PsaiMusicEntity> GetChildren()
        {
            List<PsaiMusicEntity> childEntities = new List<PsaiMusicEntity>();
            for (int i = 0; i < Groups.Count; i++)
            {
                childEntities.Add(_groups[i]);
            }
            return childEntities;
        }

        #region Properties
        private int _id;
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                SetAsParentThemeForAllGroupsAndSegments();
            }
        }

        public string Description
        {
            get;
            set;
        }

        public int ThemeTypeInt
        {
            get;
            set;
        }

        public List<int> Serialization_ManuallyBlockedThemeIds
        {
            get;
            set;
        }


        [XmlIgnoreAttribute()]
        public HashSet<Theme> ManuallyBlockedTargetThemes
        {
            private set { _manuallyBlockedThemes = value; }
            get { return _manuallyBlockedThemes; }
        }

        public float IntensityAfterRest
        {
            get { return _intensityAfterRest; }

            set
            {
                _intensityAfterRest = value;
                if (_intensityAfterRest <= 0.0f)
                {
                    _intensityAfterRest = 0.01f;        // zero is not allowed, 1% is minimum
                }
            }
        }

        public int MusicPhaseSecondsAfterRest
        {
            get;
            set;
        }

        public int MusicPhaseSecondsGeneral
        {
            get;
            set;
        }

        public int RestSecondsMin
        {
            get;
            set;
        }

        public int RestSecondsMax
        {
            get;
            set;
        }

        public int FadeoutMs
        {
            get;
            set;
        }

        public int Priority          
        {
            get;
            set;
        }


        public float WeightingSwitchGroups
        {
            get;
            set;
        }

        public float WeightingIntensityVsVariance
        {
            get;
            set;
        }

        public float WeightingLowPlaycountVsRandom
        {
            get;
            set;
        }

        public List<Group> Groups
        {
            get { return _groups; }
            set { _groups = value; }        // has to be internal for the XmlSerializer to work
        }
        #endregion

        #region ctors
        public Theme()
        {
            Initialize();
        }

        public Theme(int id)
        {
            Initialize();
            this.Id = id;
        }

        public Theme(int id, string name)
        {
            Initialize();
            this.Id = id;
            this.Name = name;
        }
        #endregion


        public override PsaiMusicEntity GetParent()
        {
            return null;
        }

        private void Initialize()
        {
            Name = DEFAULT_NAME;
            ThemeTypeInt = DEFAULT_THEMETYPEINT;
            IntensityAfterRest = DEFAULT_INTENSITY_AFTER_REST;
            MusicPhaseSecondsAfterRest= DEFAULT_THEME_DURATION_SECONDS;
            MusicPhaseSecondsGeneral  = DEFAULT_THEME_DURATION_SECONDS_AFTER_REST;
            WeightingSwitchGroups     = DEFAULT_WEIGHTING_COMPATIBILITY;
            WeightingIntensityVsVariance = DEFAULT_WEIGHTING_INTENSITY;
            WeightingLowPlaycountVsRandom = DEFAULT_WEIGHTING_LOW_PLAYCOUNT_VS_RANDOM;
            Priority                  = DEFAULT_PRIORITY;
            RestSecondsMin            = DEFAULT_REST_SECONDS_MIN;
            RestSecondsMax            = DEFAULT_REST_SECONDS_MAX;
            FadeoutMs                 = DEFAULT_FADEOUT_MS;

            _groups = new List<Group>();

            //Group defaultGroup = new Group(this, "default group");        // adding the default group upon instanciation causes problems when deserializing a group.
            //AddGroup(defaultGroup);
        }

        public override string ToString()
        {
            return "Theme '" + Name + "'";
        }

        public bool AddGroup(Group groupToAdd)
        {

            // the group name has to be unique
            foreach (Group group in _groups)
            {
                if (group.Name.Equals(groupToAdd.Name))
                    return false;
            }

            _groups.Add(groupToAdd);

            HashSet<Segment> allSnippets = GetSegmentsOfAllGroups();
            foreach (Segment snippet in allSnippets)
            {
                snippet.ThemeId = this.Id;
            }

            return true;
        }

        public void DeleteGroup(Group group)
        {
            if (group != _groups[0])    // the default group cannot be deleted
            {
                _groups.Remove(group);
            }
        }

        public HashSet<Segment> GetSegmentsOfAllGroups()
        {
            HashSet<Segment> snippets = new HashSet<Segment>();
            foreach (Group group in _groups)
            {
                foreach (Segment snippet in group.Segments)
                {
                    snippets.Add(snippet);
                }
            }
            return snippets;
        }

        /// <summary>
        /// Returns a HashSet of all the filepaths (relative to the project dir) of the AudioData used by this Theme.
        /// </summary>
        /// <returns></returns>
        public HashSet<string> GetAudioDataRelativeFilePathsUsedByThisTheme()
        {
            HashSet<string> result = new HashSet<string>();
            HashSet<Segment> segments = GetSegmentsOfAllGroups();

            foreach (Segment segment in segments)
            {
                if (!result.Contains(segment.AudioData.FilePathRelativeToProjectDir))
                {
                    result.Add(segment.AudioData.FilePathRelativeToProjectDir);
                }
            }
            return result;
        }

        public override CompatibilitySetting GetCompatibilitySetting(PsaiMusicEntity targetEntity)
        {
            if (targetEntity is Theme)
            {
                if (ManuallyBlockedTargetThemes.Contains((Theme)targetEntity))
                {
                    return CompatibilitySetting.blocked;
                }
            }
            return CompatibilitySetting.neutral;
        }


        public override CompatibilityType GetCompatibilityType(PsaiMusicEntity targetEntity, out CompatibilityReason reason)
        {
            if (targetEntity is Theme)
            {
                Theme targetTheme = targetEntity as Theme;
                psai.net.ThemeInterruptionBehavior interruptionBehavior = psai.net.Theme.GetThemeInterruptionBehavior((psai.net.ThemeType)this.ThemeTypeInt, (psai.net.ThemeType)targetTheme.ThemeTypeInt);

                if (psai.net.Theme.ThemeInterruptionBehaviorRequiresEvaluationOfSegmentCompatibilities(interruptionBehavior))
                {
                    if (ManuallyBlockedTargetThemes.Contains(targetEntity as Theme))
                    {
                        reason = CompatibilityReason.manual_setting_within_same_hierarchy;
                        return CompatibilityType.blocked_manually;
                    }
                    else
                    {
                        reason = CompatibilityReason.default_behavior_of_psai;
                        return CompatibilityType.allowed_implicitly;
                    }                    
                }
                else
                {
                    if (interruptionBehavior == psai.net.ThemeInterruptionBehavior.never)
                    {
                        reason = CompatibilityReason.target_theme_will_never_interrupt_source;
                        return CompatibilityType.logically_impossible;
                    }
                }                
            }

            reason = CompatibilityReason.not_set;
            return CompatibilityType.undefined;            
        }

        public override int GetIndexPositionWithinParentEntity(PsaiProject parentProject)
        {            
            return parentProject.Themes.IndexOf(this);
        }


        public override bool PropertyDifferencesAffectCompatibilities(PsaiMusicEntity otherEntity)
        {
            if (otherEntity is Theme)
            {
                Theme otherTheme = otherEntity as Theme;

                if (this.ThemeTypeInt != otherTheme.ThemeTypeInt)
                {
                    return true;
                }
            }

            return false;
        }


        public void SetAsParentThemeForAllGroupsAndSegments()
        {
            foreach (Group group in Groups)
            {
                group.Theme = this;
            }

            foreach (Segment snippet in GetSegmentsOfAllGroups())
            {
                snippet.ThemeId = this.Id;
            }
        }


        public psai.net.Theme CreatePsaiDotNetVersion()
        {
            psai.net.Theme netTheme = new psai.net.Theme();
            netTheme.id = this.Id;
            netTheme.Name = this.Name;
            netTheme.themeType = (psai.net.ThemeType)this.ThemeTypeInt;
            netTheme.intensityAfterRest = this.IntensityAfterRest;
            netTheme.musicDurationGeneral = this.MusicPhaseSecondsGeneral;
            netTheme.musicDurationAfterRest = this.MusicPhaseSecondsAfterRest;
            netTheme.restSecondsMin = this.RestSecondsMin;
            netTheme.restSecondsMax = this.RestSecondsMax;
            netTheme.priority = this.Priority;
            netTheme.weightings.switchGroups = this.WeightingSwitchGroups;
            netTheme.weightings.intensityVsVariety = this.WeightingIntensityVsVariance;
            netTheme.weightings.lowPlaycountVsRandom = this.WeightingLowPlaycountVsRandom;
            return netTheme;
        }

        public ProtoBuf_Theme CreateProtoBuf()
        {
            ProtoBuf_Theme pbTheme = new ProtoBuf_Theme();
            pbTheme.id = this.Id;
            pbTheme.name = this.Name;
            pbTheme.themeType = this.ThemeTypeInt;
            pbTheme.intensityAfterRest = this.IntensityAfterRest;
            pbTheme.musicPhaseSecondsGeneral = this.MusicPhaseSecondsGeneral;
            pbTheme.musicPhaseSecondsAfterRest = this.MusicPhaseSecondsAfterRest;
            pbTheme.restSecondsMin = this.RestSecondsMin;
            pbTheme.restSecondsMax = this.RestSecondsMax;
            pbTheme.priority = this.Priority;
            pbTheme.weightingSwitchGroups = this.WeightingSwitchGroups;     
            pbTheme.weightingIntensityVsVariety = this.WeightingIntensityVsVariance;
            pbTheme.weightingPlaycountVsRandom = this.WeightingLowPlaycountVsRandom;
            return pbTheme;
        }


        #region statics
        public static Theme getTestTheme1()
        {
            Theme theme = new Theme(1, "Forest");
            theme.ThemeTypeInt = 1;
            Group groupStreicher = new Group(theme, "wald_streicher");
            Group groupChoir = new Group(theme, "wald_choir");
            Segment snippetWaldStreicher1 = new Segment(101, "wald_streicher_1", (int)SegmentSuitability.start, 0.4f);
            Segment snippetWaldStreicher2 = new Segment(102, "wald_streicher_2", (int)SegmentSuitability.middle, 0.4f);
            Segment snippetWaldStreicher3 = new Segment(103, "wald_streicher_3", (int)SegmentSuitability.middle, 0.6f);
            Segment snippetWaldStreicher4 = new Segment(104, "wald_streicher_4", (int)SegmentSuitability.end, 0.6f);
            Segment snippetWaldStreicher5 = new Segment(105, "wald_streicher_5", (int)SegmentSuitability.start, 1.0f);
            Segment snippetWaldStreicher6 = new Segment(106, "wald_streicher_6", (int)SegmentSuitability.end, 1.0f);

            Segment snippetWaldChoir1 = new Segment(111, "wald_choir_1", (int)SegmentSuitability.start, 0.4f);
            Segment snippetWaldChoir2 = new Segment(112, "wald_choir_2", (int)SegmentSuitability.middle, 0.4f);
            Segment snippetWaldChoir3 = new Segment(113, "wald_choir_3", (int)SegmentSuitability.middle, 0.6f);
            Segment snippetWaldChoir4 = new Segment(114, "wald_choir_4", (int)SegmentSuitability.start, 0.6f);
            Segment snippetWaldChoir5 = new Segment(115, "wald_choir_5", (int)SegmentSuitability.end, 1.0f);
            Segment snippetWaldChoir6 = new Segment(116, "wald_choir_6", (int)SegmentSuitability.end, 1.0f);

            groupStreicher.AddSegment(snippetWaldStreicher1);
            groupStreicher.AddSegment(snippetWaldStreicher2);
            groupStreicher.AddSegment(snippetWaldStreicher3);
            groupStreicher.AddSegment(snippetWaldStreicher4);
            groupStreicher.AddSegment(snippetWaldStreicher5);
            groupStreicher.AddSegment(snippetWaldStreicher6);

            groupChoir.AddSegment(snippetWaldChoir1);
            groupChoir.AddSegment(snippetWaldChoir2);
            groupChoir.AddSegment(snippetWaldChoir3);
            groupChoir.AddSegment(snippetWaldChoir4);
            groupChoir.AddSegment(snippetWaldChoir5);
            groupChoir.AddSegment(snippetWaldChoir6);

            theme.AddGroup(groupStreicher);
            theme.AddGroup(groupChoir);

            return theme;
        }

        public static Theme getTestTheme2()
        {
            Theme theme = new Theme(2, "Cave");
            theme.ThemeTypeInt = 1;
            Group groupStreicher = new Group(theme, "cave horns");
            Group groupChoir = new Group(theme, "cave choir");
            Segment snippetCaveHorns1 = new Segment(201, "cave_horns_1", (int)SegmentSuitability.start, 0.4f);
            Segment snippetCaveHorns2 = new Segment(202, "cave_horns_2", (int)SegmentSuitability.middle, 0.4f);
            Segment snippetCaveHorns3 = new Segment(203, "cave_horns_3", (int)SegmentSuitability.middle, 0.6f);
            Segment snippetCaveHorns4 = new Segment(204, "cave_horns_4", (int)SegmentSuitability.middle, 0.6f);
            Segment snippetCaveHorns5 = new Segment(205, "cave_horns_5", (int)SegmentSuitability.middle, 1.0f);
            Segment snippetCaveHorns6 = new Segment(206, "cave_horns_6", (int)SegmentSuitability.end, 1.0f);

            Segment snippetCaveChoir1 = new Segment(211, "cave_choir_1", (int)SegmentSuitability.start, 0.4f);
            Segment snippetCaveChoir2 = new Segment(212, "cave_choir_2", (int)SegmentSuitability.middle, 0.4f);
            Segment snippetCaveChoir3 = new Segment(213, "cave_choir_3", (int)SegmentSuitability.middle, 0.6f);
            Segment snippetCaveChoir4 = new Segment(214, "cave_choir_4", (int)SegmentSuitability.middle, 0.6f);
            Segment snippetCaveChoir5 = new Segment(215, "cave_choir_5", (int)SegmentSuitability.middle, 1.0f);
            Segment snippetCaveChoir6 = new Segment(216, "cave_choir_6", (int)SegmentSuitability.end, 1.0f);

            groupStreicher.AddSegment(snippetCaveHorns1);
            groupStreicher.AddSegment(snippetCaveHorns2);
            groupStreicher.AddSegment(snippetCaveHorns3);
            groupStreicher.AddSegment(snippetCaveHorns4);
            groupStreicher.AddSegment(snippetCaveHorns5);
            groupStreicher.AddSegment(snippetCaveHorns6);

            groupChoir.AddSegment(snippetCaveChoir1);
            groupChoir.AddSegment(snippetCaveChoir2);
            groupChoir.AddSegment(snippetCaveChoir3);
            groupChoir.AddSegment(snippetCaveChoir4);
            groupChoir.AddSegment(snippetCaveChoir5);
            groupChoir.AddSegment(snippetCaveChoir6);

            theme.AddGroup(groupStreicher);
            theme.AddGroup(groupChoir);

            return theme;
        }
        #endregion


        #region ICloneable Members


        // clones the groups and snippets, but not the blocked/linked themes ! (for it must be sure that each theme exists already)
        public override object Clone()
        {
            Theme newTheme = (Theme) this.MemberwiseClone();
            newTheme.Groups = new List<Group>();
            newTheme._manuallyBlockedThemes = new HashSet<Theme>();

            foreach(Group group in Groups)
            {
                newTheme.AddGroup((Group)group.Clone());
            }

            foreach (Theme blockedTheme in this._manuallyBlockedThemes)
                newTheme._manuallyBlockedThemes.Add(blockedTheme);
            
            return newTheme;
        }

        public override PsaiMusicEntity ShallowCopy()
        {
            Theme newTheme = (Theme) this.MemberwiseClone();

            /*
            foreach(Group group in newTheme.Groups)
            {
                group.Theme = newTheme;
            }
            */

            return newTheme;
        }

        #endregion
    }
}
