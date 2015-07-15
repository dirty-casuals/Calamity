using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiGenericTrigger), true)]
class PsaiGenericTriggerEditor : PsaiSynchronizedTriggerEditor
{
    public override void OnInspectorGUI()
    {
        PsaiGenericTrigger trigger = (PsaiGenericTrigger)target;

        InspectorGuiThemeSelection(trigger);
        trigger.intensity = EditorGUILayout.Slider(new GUIContent("Intensity:"), trigger.intensity, 0.1f, 1.0f);
        InspectorGuiContiuously(trigger);
        InspectorGuiOneShot(trigger);
    }
}
