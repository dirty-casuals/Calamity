using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


[CustomEditor(typeof(PsaiPlaybackControl), true)]
class PsaiPlaybackControlEditor : Editor
{
    static string[] optionsEnd = new string[] { "by End-Segment", "by fade-out" };

    int optionEndSelectedIndex = 0;
    

    public override void OnInspectorGUI()
    {
        PsaiPlaybackControl trigger = target as PsaiPlaybackControl;

        InspectorGuiEndOrFade(trigger);
        InspectorGuiDontExecuteIfTriggersAreFiring(trigger);
    }


    public void InspectorGuiEndOrFade(PsaiPlaybackControl trigger)
    {

        optionEndSelectedIndex = trigger.immediately ? 1 : 0;
        optionEndSelectedIndex = GUILayout.SelectionGrid(optionEndSelectedIndex, optionsEnd, optionsEnd.Length, EditorStyles.radioButton);
        trigger.immediately = (optionEndSelectedIndex == 1);
        if (optionEndSelectedIndex == 1)
        {
            EditorGUI.indentLevel++;
            trigger.fadeoutSeconds = EditorGUILayout.FloatField("Fade-out Seconds", trigger.fadeoutSeconds);
            EditorGUI.indentLevel--;
        }
    }


    public void InspectorGuiDontExecuteIfTriggersAreFiring(PsaiPlaybackControl trigger)
    {
        trigger.dontExecuteIfTriggersAreFiring = EditorGUILayout.Toggle("don't execute if Triggers are currently firing", trigger.dontExecuteIfTriggersAreFiring);

        if (trigger.dontExecuteIfTriggersAreFiring)
        {
            EditorGUI.indentLevel++;
            trigger.restrictBlockToThisThemeType = (psai.net.ThemeType)EditorGUILayout.EnumPopup("...restricted to Triggers of this Type:", trigger.restrictBlockToThisThemeType);
            EditorGUI.indentLevel--;
        }
    }
}

