using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace psai.Editor
{
    class CommandChangeCompatibility : ICommand
    {
        private PsaiMusicEntity _sourceEntity;
        private PsaiMusicEntity _targetEntity;
        private CompatibilitySetting _newSetting;
        private CompatibilitySetting _oldSetting;

        public CommandChangeCompatibility(PsaiMusicEntity sourceEntity, PsaiMusicEntity targetEntity, CompatibilitySetting setting)
        {
            _sourceEntity = sourceEntity;
            _targetEntity = targetEntity;
            _newSetting = setting;

            _oldSetting = sourceEntity.GetCompatibilitySetting(targetEntity);
        }


        private void ApplyCompatibilitySetting(CompatibilitySetting setting)
        {
            switch (setting)
            {
                case CompatibilitySetting.neutral:
                    {
                        if (_sourceEntity is Segment && _targetEntity is Segment)
                        {
                            Segment sourceSnippet = (Segment)_sourceEntity;
                            Segment targetSnippet = (Segment)_targetEntity;

                            if (sourceSnippet.ManuallyBlockedSnippets.Contains(targetSnippet))
                            {
                                sourceSnippet.ManuallyBlockedSnippets.Remove(targetSnippet);
                            }
                            if (sourceSnippet.ManuallyLinkedSnippets.Contains(targetSnippet))
                            {
                                sourceSnippet.ManuallyLinkedSnippets.Remove(targetSnippet);
                            }
                        }
                        else if (_sourceEntity is Group && _targetEntity is Group)
                        {
                            Group sourceGroup = (Group)_sourceEntity;
                            Group targetGroup = (Group)_targetEntity;

                            if (sourceGroup.ManuallyBlockedGroups.Contains(targetGroup))
                            {
                                sourceGroup.ManuallyBlockedGroups.Remove(targetGroup);
                            }
                            if (sourceGroup.ManuallyLinkedGroups.Contains(targetGroup))
                            {
                                sourceGroup.ManuallyLinkedGroups.Remove(targetGroup);
                            }
                        }
                        else if (_sourceEntity is Theme && _targetEntity is Theme)
                        {
                            Theme sourceTheme = (Theme)_sourceEntity;
                            Theme targetTheme = (Theme)_targetEntity;

                            if (sourceTheme.ManuallyBlockedTargetThemes.Contains(targetTheme))
                            {
                                sourceTheme.ManuallyBlockedTargetThemes.Remove(targetTheme);
                            }
                        }
                    }
                    break;

                case CompatibilitySetting.allowed:
                    {
                        if (_sourceEntity is Segment && _targetEntity is Segment)
                        {
                            Segment sourceSnippet = (Segment)_sourceEntity;
                            Segment targetSnippet = (Segment)_targetEntity;

                            if (!sourceSnippet.ManuallyLinkedSnippets.Contains(targetSnippet))
                            {
                                sourceSnippet.ManuallyLinkedSnippets.Add(targetSnippet);
                            }
                            if (sourceSnippet.ManuallyBlockedSnippets.Contains(targetSnippet))
                            {
                                sourceSnippet.ManuallyBlockedSnippets.Remove(targetSnippet);
                            }
                        }
                        else if (_sourceEntity is Group && _targetEntity is Group)
                        {
                            Group sourceGroup = (Group)_sourceEntity;
                            Group targetGroup = (Group)_targetEntity;

                            if (!sourceGroup.ManuallyLinkedGroups.Contains(targetGroup))
                            {
                                sourceGroup.ManuallyLinkedGroups.Add(targetGroup);
                            }
                            if (sourceGroup.ManuallyBlockedGroups.Contains(targetGroup))
                            {
                                sourceGroup.ManuallyBlockedGroups.Remove(targetGroup);
                            }
                        }
                    }
                    break;

                case CompatibilitySetting.blocked:
                    {
                        if (_sourceEntity is Segment && _targetEntity is Segment)
                        {
                            Segment sourceSnippet = (Segment)_sourceEntity;
                            Segment targetSnippet = (Segment)_targetEntity;

                            if (!sourceSnippet.ManuallyBlockedSnippets.Contains(targetSnippet))
                            {
                                sourceSnippet.ManuallyBlockedSnippets.Add(targetSnippet);
                            }
                            if (sourceSnippet.ManuallyLinkedSnippets.Contains(targetSnippet))
                            {
                                sourceSnippet.ManuallyLinkedSnippets.Remove(targetSnippet);
                            }
                        }
                        else if (_sourceEntity is Group && _targetEntity is Group)
                        {
                            Group sourceGroup = (Group)_sourceEntity;
                            Group targetGroup = (Group)_targetEntity;

                            if (!sourceGroup.ManuallyBlockedGroups.Contains(targetGroup))
                            {
                                sourceGroup.ManuallyBlockedGroups.Add(targetGroup);
                            }
                            if (sourceGroup.ManuallyLinkedGroups.Contains(targetGroup))
                            {
                                sourceGroup.ManuallyLinkedGroups.Remove(targetGroup);
                            }
                        }
                        else if (_sourceEntity is Theme && _targetEntity is Theme)
                        {
                            Theme sourceTheme = (Theme)_sourceEntity;
                            Theme targetTheme = (Theme)_targetEntity;

                            if (!sourceTheme.ManuallyBlockedTargetThemes.Contains(targetTheme))
                            {
                                sourceTheme.ManuallyBlockedTargetThemes.Add(targetTheme);
                            }
                        }

                    }
                    break;
            }
        }

        public void Execute()
        {
            ApplyCompatibilitySetting(_newSetting);
            EventArgs_CompatibilitySettingChanged e = new EventArgs_CompatibilitySettingChanged(_sourceEntity, _targetEntity);
            EditorModel.Instance.RaiseEvent_CompatibilitySettingChanged(e);            
        }

        public void Undo()
        {
            ApplyCompatibilitySetting(_oldSetting);
            EventArgs_CompatibilitySettingChanged e = new EventArgs_CompatibilitySettingChanged(_sourceEntity, _targetEntity);
            EditorModel.Instance.RaiseEvent_CompatibilitySettingChanged(e);
        }

    }
}
