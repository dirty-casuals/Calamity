using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PsaiTriggerOnButtonDown), true)]
public class PsaiTriggerOnButtonDownEditor : PsaiSynchronizedTriggerEditor
{

    public override void OnInspectorGUI()
    {
        PsaiTriggerOnButtonDown trigger = target as PsaiTriggerOnButtonDown;

        InspectorGuiThemeSelection(trigger);

        if (trigger.fireContinuously)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.MinMaxSlider(new GUIContent("Intensity Range"), ref trigger.minimumIntensity, ref trigger.maximumIntensity, 0.1f, 1.0f);
            Color defaultColor = GUI.contentColor;
            GUI.contentColor = new Color(0.8f, 0.8f, 0.8f);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Min Intensity", trigger.minimumIntensity.ToString("F2"));
            EditorGUILayout.LabelField("Max Intensity", trigger.maximumIntensity.ToString("F2"));
            GUI.contentColor = defaultColor;
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            trigger.intensityGainPerTick = EditorGUILayout.FloatField("Intensity gain per Tick", trigger.intensityGainPerTick);
        }
        else
        {
            trigger.minimumIntensity = EditorGUILayout.Slider("fixed Intensity", trigger.minimumIntensity, 0.1f, 1.0f);
        }

        trigger.triggerKeyCode = (UnityEngine.KeyCode)EditorGUILayout.EnumPopup("Trigger Key:", trigger.triggerKeyCode);

        InspectorGuiOneShot(trigger);
        InspectorGuiContiuously(trigger);
    }

}
