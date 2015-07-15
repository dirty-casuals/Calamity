using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace psai.Editor
{


    // this is used for Commands and describes the individual Compatibility as set by the User or the Application (implicit)
    [Serializable]
    public enum CompatibilitySetting
    {
        neutral = 0,
        blocked,
        allowed
    }

    // this describes the type of compatibility resulting from the combination of CompatibilitySetting, psai playback-logic and
    // transition hierarchy
    [Serializable]
    public enum CompatibilityType
    {
        undefined = 0,
        allowed_implicitly,
        allowed_manually,
        blocked_implicitly,
        blocked_manually,
        logically_impossible                  // this transition will never occur in the psai replay logic
        //allowed_always                          // this transition is always allowed by the playback logic and cannot be changed
    }

    // This is used for displaying information in the GUI why a certain transition is blocked or allowed.
    public enum CompatibilityReason
    {
        not_set = 0,
        //target_theme_will_immediately_interrupt_source,
        target_theme_will_never_interrupt_source,
        manual_setting_within_same_hierarchy,
        manual_setting_of_parent_entity,
        inherited_from_parent_hierarchy,
        target_segment_and_source_segment_are_both_only_usable_at_end,
        target_segment_is_of_a_different_group_and_is_only_usable_at_end,
        target_segment_is_a_pure_bridge_segment_within_the_same_group,
        target_segment_is_a_manual_bridge_segment_for_the_source_group,
        target_segment_is_an_automatic_bridge_segment,
        target_group_contains_at_least_one_bridge_segment,
        anything_may_be_played_after_a_pure_end_segment,
        default_behavior_of_psai,
        default_compatibility_of_the_target_segment_as_a_follower
    }    




    // Parent Class of Theme, Group and Segment    
    [Serializable]
    public abstract class PsaiMusicEntity : ICloneable
    {

        #region Properties

        public string Name
        {
            get;
            set;
        }
        #endregion

        public abstract string GetClassString();
        public abstract psai.Editor.CompatibilitySetting GetCompatibilitySetting(PsaiMusicEntity targetEntity);
        public abstract psai.Editor.CompatibilityType GetCompatibilityType(PsaiMusicEntity targetEntity, out CompatibilityReason reason);
        public abstract psai.Editor.PsaiMusicEntity GetParent();
        public abstract List<psai.Editor.PsaiMusicEntity> GetChildren();
        public abstract int GetIndexPositionWithinParentEntity(PsaiProject parentProject);
        public virtual System.Object Clone()
        {        
            return this.MemberwiseClone();
        }

        // copies all members without cloning the children.
        public virtual PsaiMusicEntity ShallowCopy()
        {
            return (PsaiMusicEntity)this.MemberwiseClone();
        }


        // for each Subclass, implement this method to be able to tell if a given property change of an entity affects the
        // compatibilities to other Entities, so that the TargetView needs to be redrawn.
        public virtual bool PropertyDifferencesAffectCompatibilities(PsaiMusicEntity otherEntity)
        {
            return false;
        }


        public Theme GetTheme()
        {
            PsaiMusicEntity entityToCheck = this;
            Theme theme;
            do
            {
                theme = entityToCheck as Theme;
                entityToCheck = entityToCheck.GetParent();
            } while (theme == null && entityToCheck != null);

            return theme;
        }
    }
}
