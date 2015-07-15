using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiOneShotTrigger), true)]
public class PsaiOneShotTriggerEditor : PsaiSynchronizedTriggerEditor
{
    public override void OnInspectorGUI()
    {
        PsaiOneShotTrigger trigger = target as PsaiOneShotTrigger;
        base.InspectorGuiThemeSelection(trigger);        
        InspectorIntensity(trigger);
        InspectorGuiOneShot(trigger);
    }


    public void InspectorIntensity(PsaiOneShotTrigger trigger)
    {
        trigger.intensity = EditorGUILayout.Slider(new GUIContent("Intensity:"), trigger.intensity, 0.1f, 1.0f);
    }
}