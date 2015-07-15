using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using psai.Editor;
using System.Xml.Serialization;

namespace psai.Editor
{
    [Serializable]
    public class Group : PsaiMusicEntity, ICloneable
    {
        [NonSerialized]  // hopefully helps to prevent the serialization error message in Unity 4.5
        private List<Segment> m_segments = new List<Segment>();

        [NonSerialized] // hopefully helps to prevent the serialization error message in Unity 4.5
        private HashSet<Segment> _manualBridgeSnippetsOfTargetGroups = new HashSet<Segment>();        // the compatible manual Bridge Segments of other (target) groups

        [NonSerialized] // hopefully helps to prevent the serialization error message in Unity 4.5
        private HashSet<Group> _manuallyBlockedGroups = new HashSet<Group>();

        [NonSerialized] // hopefully helps to prevent the serialization error message in Unity 4.5
        private HashSet<Group> _manuallyLinkedGroups = new HashSet<Group>();

        private string _description = "";

        #region Properties

        public List<Segment> Segments
        {
            get { return m_segments; }
            set { m_segments = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value;}
        }

        // unique id used for serialization
        public int Serialization_Id
        {
            get;
            set;
        }

        public List<int> Serialization_ManuallyBlockedGroupIds
        {
            get;
            set;
        }

        public List<int> Serialization_ManuallyLinkedGroupIds
        {
            get;
            set;
        }

        public List<int> Serialization_ManualBridgeSegmentIds
        {
            get;
            set;
        }

        [XmlIgnoreAttribute()]
        public HashSet<Segment> ManualBridgeSnippetsOfTargetGroups
        {
            get { return _manualBridgeSnippetsOfTargetGroups; }
            set { _manualBridgeSnippetsOfTargetGroups = value; }
        }

        [XmlIgnoreAttribute()]
        public HashSet<Group> ManuallyBlockedGroups
        {
            set { _manuallyBlockedGroups = value; }
            get { return _manuallyBlockedGroups; }
        }

        [XmlIgnoreAttribute()]
        public HashSet<Group> ManuallyLinkedGroups
        {
            set { _manuallyLinkedGroups = value; }
            get { return _manuallyLinkedGroups; }
        }

        [XmlIgnoreAttribute()]
        public Theme Theme
        {
            set;
            get;
        }

        #endregion


        public override string GetClassString()
        {
            return "Group";
        }


        #region ctors

        public Group()
        {
            this.Name = "new_group";
        }

        public Group(Theme parentTheme, string name)
            : this()
        {
            this.Theme = parentTheme;
            this.Name = name;
        }


        public Group(Theme parentTheme)
            : this()
        {
            this.Theme = parentTheme;
        }
        #endregion

        ~Group()
        {

        }

        public void AddSegment(Segment snippet)
        {
            AddSnippet_internal(snippet, -1);       // special case: -1 to enqueue at end of list
        }

        public void AddSegment(Segment snippet, int index)
        {
            AddSnippet_internal(snippet, index);
        }

        private void AddSnippet_internal(Segment snippet, int insertIndex)
        {           
            snippet.Group = this;

            if (this.Theme != null)
            {
                snippet.ThemeId = this.Theme.Id;
            }

            if (insertIndex < 0  || insertIndex >= m_segments.Count)
            {
                m_segments.Add(snippet);
            }
            else
            {
                m_segments.Insert(insertIndex, snippet);
            }
        }

        public void RemoveSegment(Segment snippet)
        {
            this.m_segments.Remove(snippet);
        }

        public bool HasAtLeastOneBridgeSegmentToTargetGroup(Group targetGroup)
        {
            // 1. check for manual bridge Segments
            foreach (Segment bridgeSnippet in _manualBridgeSnippetsOfTargetGroups)
            {
                if (bridgeSnippet.Group == targetGroup)
                    return true;
            }

            // 2. check for Automatic Bridge Segments
            return (targetGroup.ContainsAtLeastOneAutomaticBridgeSegment());
        }

        public bool ContainsAtLeastOneManualBridgeSegmentForSourceGroup(Group sourceGroup)
        {
            foreach (Segment bridgeSnippet in sourceGroup.ManualBridgeSnippetsOfTargetGroups)
            {
                if (bridgeSnippet.Group == this)
                    return true;
            }

            return false;
        }

        public bool ContainsAtLeastOneAutomaticBridgeSegment()
        {
            foreach (Segment snippet in Segments)
            {
                if (snippet.IsAutomaticBridgeSegment)
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return "Group '" + this.Name + "' (" + this.Theme.Name + ")";
        }

        public override CompatibilitySetting GetCompatibilitySetting(PsaiMusicEntity targetEntity)
        {
            if (targetEntity is Group)
            {
                if (ManuallyBlockedGroups.Contains((Group)targetEntity))
                {
                    return CompatibilitySetting.blocked;
                }
                else if (ManuallyLinkedGroups.Contains((Group)targetEntity))
                {
                    return CompatibilitySetting.allowed;
                }
            }
            return CompatibilitySetting.neutral;
        }

        public override CompatibilityType GetCompatibilityType(PsaiMusicEntity targetEntity, out CompatibilityReason reason)
        {

            if (targetEntity is Group)
            {
                Group targetGroup = (Group)targetEntity;

                Theme sourceTheme = this.Theme;
                Theme targetTheme = targetGroup.Theme;                

                if (sourceTheme.GetCompatibilityType(targetTheme, out reason) == CompatibilityType.logically_impossible)
                {
                    return CompatibilityType.logically_impossible;
                }
                else
                {
                    if (ManuallyBlockedGroups.Contains(targetGroup))
                    {
                        reason = CompatibilityReason.manual_setting_within_same_hierarchy;
                        return CompatibilityType.blocked_manually;
                    }
                    else if (ManuallyLinkedGroups.Contains(targetGroup))
                    {
                        reason = CompatibilityReason.manual_setting_within_same_hierarchy;
                        return CompatibilityType.allowed_manually;
                    }
                    else
                    {
                        CompatibilityType themeCompatibility = Theme.GetCompatibilityType(targetGroup.Theme, out reason);
                        {

                            switch (themeCompatibility)
                            {
                                case CompatibilityType.blocked_manually:
                                    {
                                        reason = CompatibilityReason.manual_setting_of_parent_entity;
                                        return CompatibilityType.blocked_implicitly;
                                    }


                                case CompatibilityType.allowed_manually:
                                    {
                                        reason = CompatibilityReason.manual_setting_of_parent_entity;
                                        return CompatibilityType.allowed_implicitly;
                                    }

                                default:
                                    reason = CompatibilityReason.inherited_from_parent_hierarchy;
                                    return themeCompatibility;
                            }
                        }

                    }
                }
            }

            reason = CompatibilityReason.not_set;
            return CompatibilityType.undefined;
        }


        public void SetAsParentGroupForAllSegments()
        {
            #if DEBUG
                Console.WriteLine("Group::SetAsParentGroupForAllSegments() this=" + this.Name + "  hashCode=" + this.GetHashCode());
            #endif

            foreach (Segment snippet in Segments)
            {
                snippet.Group = this;
                
                #if DEBUG
                    Console.WriteLine("...set for Segment " + snippet.Name + "   " + snippet.GetHashCode());
                #endif
            }
        }

        public override PsaiMusicEntity GetParent()
        {
            return this.Theme;
        }


        public override List<PsaiMusicEntity> GetChildren()
        {
            List<PsaiMusicEntity> childEntities = new List<PsaiMusicEntity>();
            for (int i = 0; i < m_segments.Count; i++)
            {
                childEntities.Add(m_segments[i]);
            }
            return childEntities;
        }


        public override int GetIndexPositionWithinParentEntity(PsaiProject parentProject)
        {
            return this.Theme.Groups.IndexOf(this);
        }

        public override object Clone()
        {
            Group newGroup = (Group)this.MemberwiseClone();
            newGroup.Segments = new List<Segment>();
            newGroup.ManualBridgeSnippetsOfTargetGroups = new HashSet<Segment>();
            newGroup.ManuallyBlockedGroups = new HashSet<Group>();
            newGroup.ManuallyLinkedGroups = new HashSet<Group>();

            foreach (Segment snippet in Segments)
            {
                newGroup.AddSegment((Segment)snippet.Clone());
            }

            newGroup.ManuallyBlockedGroups = new HashSet<Group>();
            newGroup.ManuallyLinkedGroups = new HashSet<Group>();

            foreach (Group blockedGroup in this.ManuallyBlockedGroups)
                newGroup.ManuallyBlockedGroups.Add(blockedGroup);

            foreach (Group linkedGroup in this.ManuallyLinkedGroups)
                newGroup.ManuallyLinkedGroups.Add(linkedGroup);

            return newGroup;
        }

        public override PsaiMusicEntity ShallowCopy()
        {
            Group newGroup = (Group)this.MemberwiseClone();

            foreach (Segment snippet in Segments)
            {
                snippet.Group = newGroup;
            }

            return newGroup;
        }
    }
}
