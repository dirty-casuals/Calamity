using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(PsaiStopMusic), true)]
class PsaiStopMusicEditor : PsaiPlaybackControlEditor
{

    static string[] optionsStop = new string[] { "keep silent until next Trigger", "wake up automatically again" };
    int optionsStopSelectedIndex = 0;

    public override void OnInspectorGUI()
    {
        PsaiStopMusic trigger = target as PsaiStopMusic;

        InspectorGuiEndOrFade(trigger);

        optionsStopSelectedIndex = trigger.keepSilentUntilNextTrigger ? 0 : 1;
        optionsStopSelectedIndex = GUILayout.SelectionGrid(optionsStopSelectedIndex, optionsStop, optionsStop.Length, EditorStyles.radioButton);

        if (optionsStopSelectedIndex == 1)
        {
            // override checkbox
            EditorGUI.indentLevel++;
            trigger.overrideDefaultRestTime = EditorGUILayout.Toggle("override default rest time", trigger.overrideDefaultRestTime);

            if (trigger.overrideDefaultRestTime)
            {
                EditorGUI.indentLevel++;
                trigger.restTimeOverrideSecondsMin = EditorGUILayout.IntField("override rest seconds min", trigger.restTimeOverrideSecondsMin);
                trigger.restTimeOverrideSecondsMax = EditorGUILayout.IntField("override rest seconds max", trigger.restTimeOverrideSecondsMax);

                if (trigger.restTimeOverrideSecondsMax < trigger.restTimeOverrideSecondsMin)
                {
                    trigger.restTimeOverrideSecondsMax = trigger.restTimeOverrideSecondsMin;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        trigger.keepSilentUntilNextTrigger = (optionsStopSelectedIndex == 0);

        InspectorGuiDontExecuteIfTriggersAreFiring(trigger);
    }
}
