using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using psai.net;

[CustomEditor(typeof(PsaiTriggerWhenInRange), true)]
public class PsaiTriggerWhenInRangeEditor : PsaiColliderBasedTriggerEditor
{
    public override void OnInspectorGUI()
    {
        PsaiTriggerWhenInRange trigger = (PsaiTriggerWhenInRange)target;
        InspectorGuiThemeSelection(trigger);

        if (trigger.fireContinuously)
        {
            trigger.scaleIntensityByDistance = EditorGUILayout.Toggle("scale Intensity by distance", trigger.scaleIntensityByDistance);
        } 

        if (trigger.scaleIntensityByDistance && trigger.fireContinuously)
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
        }
        else
        {
            trigger.minimumIntensity = EditorGUILayout.Slider("fixed Intensity", trigger.minimumIntensity, 0.1f, 1.0f);
        }

        EditorGUILayout.Separator();

        float lastRadius = trigger.triggerRadius;
        trigger.triggerRadius = EditorGUILayout.FloatField("Trigger Radius", trigger.triggerRadius);
        if (trigger.triggerRadius < 0)
        {
            trigger.triggerRadius = 0;
        }
        if (lastRadius != trigger.triggerRadius)
        {
            EditorUtility.SetDirty(trigger);    // this fixes OnDrawGizmos() not being called while we drag to change the value
        }

        InspectorGuiOneShot(trigger);   
        InspectorGuiContiuously(trigger);
        InspectorGuiPlayerCollider(trigger);
    }
}
