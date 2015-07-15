using UnityEditor;
using UnityEngine;
using System.Collections;
using psai.net;
using System.Collections.Generic;

[CustomEditor(typeof(PsaiTriggerBase), true)]
public class PsaiTriggerBaseEditor : Editor
{
    private static PsaiSoundtrackLoader _psaiSoundtrackLoader;
    private static PsaiSoundtrackLoader PsaiSoundtrackLoader
    {
        get
        {
            if (_psaiSoundtrackLoader == null)
            {
                _psaiSoundtrackLoader = GameObject.FindObjectOfType<PsaiSoundtrackLoader>();
            }

            return _psaiSoundtrackLoader;
        }
    }

    private static Dictionary<int, ThemeInfo> _themeInfos;
    private static Dictionary<int, ThemeInfo> ThemeInfos
    {
        get
        {
            if (_themeInfos == null || _themeInfos.Count == 0)
            {
                _themeInfos = new Dictionary<int, ThemeInfo>();

                // watch out - don't create another Psai instance here, as it will flood your Psai GameObject with more PsaiAudioChannels
                if (PsaiCore.IsInstanceInitialized())
                {
                    SoundtrackInfo soundtrackInfo = PsaiCore.Instance.GetSoundtrackInfo();

                    int[] themeIds = soundtrackInfo.themeIds;
                    foreach (int themeId in themeIds)
                    {
                        ThemeInfo themeInfo = PsaiCore.Instance.GetThemeInfo(themeId);
                        _themeInfos[themeId] = themeInfo;
                    }
                }

            }
            return _themeInfos;
        }
    }


    public override void OnInspectorGUI()
    {
        PsaiTriggerBase trigger = target as PsaiTriggerBase;

        InspectorGuiThemeSelection(trigger);  
    }

    protected void InspectorGuiThemeSelection(PsaiTriggerBase trigger)
    {
        trigger.themeId = EditorGUILayout.IntField("ThemeId", trigger.themeId);
        if (trigger.themeId < 1)
        {
            trigger.themeId = 1;
        }

        if (PsaiCore.IsInstanceInitialized())
        {
            Color defaultContentColor = GUI.contentColor;
            string themeInfoString = "THEME NOT FOUND";
            GUI.contentColor = Color.red;
            if (ThemeInfos.ContainsKey(trigger.themeId))
            {
                GUI.contentColor = new Color(0, 0.85f, 0);
                ThemeInfo themeInfo = ThemeInfos[trigger.themeId];
                themeInfoString = themeInfo.name + " [" + psai.net.Theme.ThemeTypeToString(themeInfo.type) + "]";
            }
            EditorGUILayout.LabelField(" ", themeInfoString);

            GUI.contentColor = defaultContentColor;
        }
    }

}
