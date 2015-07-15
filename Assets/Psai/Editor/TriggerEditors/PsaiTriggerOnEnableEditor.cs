using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiTriggerOnEnable), true)]
public class PsaiTriggerEditorOnEnableEditor : PsaiOneShotTriggerEditor
{
    public override void OnInspectorGUI()
    {
        PsaiTriggerOnEnable trigger = (PsaiTriggerOnEnable)target;
        InspectorGuiThemeSelection(trigger);
        InspectorIntensity(trigger);
        InspectorGuiOneShot(trigger, false);
        trigger.resetHasFiredStateOnDisable = false;
    }

}
