using UnityEditor;
using UnityEngine;
using psai.net;

[CustomEditor(typeof(PsaiSynchronizedTrigger), true)]
public class PsaiSynchronizedTriggerEditor : PsaiTriggerBaseEditor
{
    public override void OnInspectorGUI()
    {
        PsaiSynchronizedTrigger trigger = (PsaiSynchronizedTrigger)target;

        InspectorGuiThemeSelection(trigger);
        InspectorGuiContiuously(trigger);
        InspectorGuiOneShot(trigger);
    }

    protected void InspectorGuiOverrideDefaultSettings(PsaiSynchronizedTrigger trigger)
    {
        EditorGUILayout.Separator();
        trigger.overrideMusicDurationInSeconds = EditorGUILayout.Toggle("override default calm-down time", trigger.overrideMusicDurationInSeconds);

        if (trigger.overrideMusicDurationInSeconds)
        {
            EditorGUI.indentLevel++;
            trigger.musicDurationInSeconds = EditorGUILayout.IntField("calm down time (seconds)", trigger.musicDurationInSeconds);
            EditorGUI.indentLevel--;
        }

    }

    protected void InspectorGuiContiuously(PsaiSynchronizedTrigger trigger)
    {
        EditorGUILayout.Separator();
        trigger.fireContinuously = EditorGUILayout.Toggle("fire continuously", trigger.fireContinuously);

        if (trigger.fireContinuously && !trigger.synchronizeByPsaiCoreManager)
        {
            EditorGUI.indentLevel++;
            trigger.tickIntervalInSeconds = EditorGUILayout.FloatField("Tick Interval (seconds)", trigger.tickIntervalInSeconds);
            EditorGUI.indentLevel--;
        }
    }

    protected void InspectorGuiOneShot(PsaiSynchronizedTrigger trigger, bool showResetOnDisableOption = true)
    {
        EditorGUILayout.Separator();
        InspectorGuiOverrideDefaultSettings(trigger);

        trigger.synchronizeByPsaiCoreManager = EditorGUILayout.Toggle("synchronize with concurrent Triggers ", trigger.synchronizeByPsaiCoreManager);

        if (trigger.synchronizeByPsaiCoreManager)
        {
            EditorGUI.indentLevel++;
            trigger.addUpIntensities = EditorGUILayout.Toggle("sum-up overlapping Triggers", trigger.addUpIntensities);

            if (trigger.addUpIntensities)
            {
                EditorGUI.indentLevel++;
                trigger.limitIntensitySum = EditorGUILayout.Slider(new GUIContent("...up to this Intensity limit:"), trigger.limitIntensitySum, 0.1f, 1.0f);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        if (!trigger.fireContinuously)
        {
            trigger.interruptAnyTheme = EditorGUILayout.Toggle("force immediate interruption of all Theme Types", trigger.interruptAnyTheme);  
            trigger.deactivateAfterFiringOnce = EditorGUILayout.Toggle("deactivate after firing once", trigger.deactivateAfterFiringOnce);

            if (showResetOnDisableOption)
            {
                if (trigger.deactivateAfterFiringOnce)
                {
                    EditorGUI.indentLevel++;
                    trigger.resetHasFiredStateOnDisable = EditorGUILayout.Toggle("reset 'has fired' state on disable", trigger.resetHasFiredStateOnDisable);
                    EditorGUI.indentLevel--;
                }
            }
        }
    }
}
