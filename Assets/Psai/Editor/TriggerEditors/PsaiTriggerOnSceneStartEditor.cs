using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiTriggerOnSceneStart))]
public class PsaiTriggerOnSceneStartEditor : PsaiOneShotTriggerEditor
{
    public override void OnInspectorGUI()
    {
        PsaiTriggerOnSceneStart trigger = target as PsaiTriggerOnSceneStart;
        InspectorGuiThemeSelection(trigger);
        InspectorIntensity(trigger);
    }
}
