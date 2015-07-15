using UnityEngine;
using UnityEditor;

using psai.Editor;
using System.Collections.Generic;
using System.IO;

public class PsaiEntityEditor : EditorWindow
{
    private PsaiMusicEntity _entity;
    private PsaiMusicEntity _tmpEntity;
    private bool _deselectAnyGuiElement;
    private Vector2 _scrollPos;
    private bool _disableSelectionButton;

    private AudioClip _lastSelectedAudioClip = null;    

    PsaiEntityEditor()
    {
        this.title = "Psai Entity";
        EditorModel.Instance.PsaiProjectLoadedEvent += HandleEvent_ProjectLoaded;
    }


    public PsaiMusicEntity Entity
    {
        get
        {
            return _entity;
        }

        set
        {
            CheckForChangesAndApplyToProjectIfConfirmed();
            DeselectAnyGuiElement();
            _entity = value;
            if (_entity != null)
            {
                _tmpEntity = (PsaiMusicEntity)_entity.ShallowCopy();
            }
            _lastSelectedAudioClip = null;
            UpdateSelectionButton();
            Repaint();
        }
    }


    /// <summary>
    /// Blurs Unity focus
    /// Should be called from inside the OnGUI() handler
    /// </summary>
    private void DeselectAnyGuiElement()
    {
        _deselectAnyGuiElement = true;
    }

    public void CheckForChangesAndApplyToProjectIfConfirmed()
    {
        if (TempEntityHasBeenEdited())
        {
            if (EditorUtility.DisplayDialog("Apply changes?", "You have made changes to " + _entity.GetClassString() + " " + _entity.Name + ". Do you wish to apply them to your temporary copy of your psai Project?", "Apply", "Discard"))
            {
                ApplyChangesToPsaiProject();
            }
        }
    }

    private void ApplyChangesToPsaiProject()
    {
        PnxHelperz.CopyTo(_tmpEntity, _entity);
        EditorModel.Instance.ProjectDataChanged();
        Debug.Log("applied changes to psai Project");
    }


    void OnDisable()
    {
        CheckForChangesAndApplyToProjectIfConfirmed();
    }


    private bool TempEntityHasBeenEdited()
    {
        if (_entity != null && _tmpEntity != null)
        {
            if (_tmpEntity is Theme)
            {
                Theme tmpTheme = _tmpEntity as Theme;
                Theme theme = _entity as Theme;

                if (PnxHelperz.PublicInstancePropertiesEqual<Theme>(theme, tmpTheme, "Id", "ThemeTypeInt", 
                    "RestSecondsMin", "RestSecondsMax", "MusicPhaseSecondsGeneral", "MusicPhaseSecondsAfterRest", "IntensityAfterRest",
                    "WeightingSwitchGroups", "WeightingIntensityVsVariance", "WeightingLowPlaycountVsRandom"
                    ) == false)
                {
                    return true;
                }
            }
            else if (_tmpEntity is Segment)
            {
                Segment tmpSegment = _tmpEntity as Segment;
                Segment segment = _entity as Segment;

                if (PnxHelperz.PublicInstancePropertiesEqual<Segment>(segment, tmpSegment, "Intensity", "IsUsableAtStart", "IsUsableAtEnd", "IsUsableInMiddle") == false)
                {
                    return true;
                }
            }

        }
        return false;
    }


    void OnGUIforTheme()
    {
        Theme tmpTheme = _tmpEntity as Theme;        

        int tmpThemeId = EditorGUILayout.IntField("Id", tmpTheme.Id);

        if (tmpTheme.Id != tmpThemeId && tmpThemeId > 0)
        {
            //Debug.Log("tmpTheme.Id=" + tmpTheme.Id.ToString() + "  tmpThemeid=" + tmpThemeId);
            int initialThemeId = tmpThemeId;
            while (EditorModel.Instance.Project.CheckIfThemeIdIsInUse(tmpThemeId) == true)
            {
                if (initialThemeId > tmpTheme.Id)
                {
                    tmpThemeId++;
                }
                else
                {
                    tmpThemeId--;
                }
            }
            if (tmpThemeId == 0)
            {
                tmpThemeId = EditorModel.Instance.Project.GetNextFreeThemeId(initialThemeId);
                //Debug.Log("was 0, tmpThemeId=" + tmpThemeId);
            }

            tmpTheme.Id = tmpThemeId;
        }

        tmpTheme.ThemeTypeInt = (int)(psai.net.ThemeType)EditorGUILayout.EnumPopup("Theme Type", (psai.net.ThemeType)tmpTheme.ThemeTypeInt);
        if (tmpTheme.ThemeTypeInt == 0)
        {
            tmpTheme.ThemeTypeInt = (int)psai.net.ThemeType.basicMood;
        }

        if ((psai.net.ThemeType)tmpTheme.ThemeTypeInt == psai.net.ThemeType.highlightLayer)
        {
            GUI.enabled = false;
        }

        int themeDuration = EditorGUILayout.IntField("Theme Duration (sec.)", tmpTheme.MusicPhaseSecondsGeneral);
        if (themeDuration >= 0)
        {
            tmpTheme.MusicPhaseSecondsGeneral = themeDuration;
        }

        if ((psai.net.ThemeType)tmpTheme.ThemeTypeInt != psai.net.ThemeType.basicMood)
        {
            GUI.enabled = false;
        }

        if ((psai.net.ThemeType)tmpTheme.ThemeTypeInt != psai.net.ThemeType.basicMood)
        {
            GUI.enabled = false;
        }

        int restTimeMin = EditorGUILayout.IntField("Rest Time Min (seconds)", tmpTheme.RestSecondsMin);
        if (restTimeMin >= 0 && restTimeMin <= tmpTheme.RestSecondsMax)
        {
            tmpTheme.RestSecondsMin = restTimeMin;
        }

        int restTimeMax = EditorGUILayout.IntField("Rest Time Max (seconds)", tmpTheme.RestSecondsMax);
        if (restTimeMax >= 0 && restTimeMax >= tmpTheme.RestSecondsMin)
        {
            tmpTheme.RestSecondsMax = restTimeMax;
        }

        int themeDurationAfterRest = EditorGUILayout.IntField("Theme Dur. after Rest", tmpTheme.MusicPhaseSecondsAfterRest);
        if (themeDurationAfterRest >= 0)
        {
            tmpTheme.MusicPhaseSecondsAfterRest = themeDurationAfterRest;
        }

        tmpTheme.IntensityAfterRest = EditorGUILayout.Slider("Intensity after Rest", tmpTheme.IntensityAfterRest, 0f, 1.0f);
        GUI.enabled = true;

        EditorGUILayout.Space();
        if ((psai.net.ThemeType)tmpTheme.ThemeTypeInt == psai.net.ThemeType.highlightLayer)
        {
            GUI.enabled = false;
        }
        EditorGUILayout.LabelField("Weightings");
        tmpTheme.WeightingSwitchGroups = EditorGUILayout.Slider("Jump between Groups", tmpTheme.WeightingSwitchGroups, 0f, 1.0f);
        tmpTheme.WeightingIntensityVsVariance = EditorGUILayout.Slider("ignore Segment Intensity", tmpTheme.WeightingIntensityVsVariance, 0f, 1.0f);
        tmpTheme.WeightingLowPlaycountVsRandom = EditorGUILayout.Slider("Random Factor", tmpTheme.WeightingLowPlaycountVsRandom, 0f, 1.0f);
        GUI.enabled = true;
    }

    void OnGUIforSegment()
    {
        Segment tmpSegment = _tmpEntity as Segment;
        Segment segment = _entity as Segment;

        string selectInProjectString = "Select AudioClip";

        if (_disableSelectionButton)
        {
            GUI.enabled = false;
        }

        if (GUILayout.Button(selectInProjectString, GUILayout.MaxWidth(GUI.skin.label.CalcSize(new GUIContent(selectInProjectString)).x + PsaiEditorWindow.BUTTON_MARGIN), GUILayout.Height(PsaiEditorWindow.LINE_HEIGHT)))
        {

            PsaiEditorWindow editor = (PsaiEditorWindow)EditorWindow.GetWindow(typeof(PsaiEditorWindow), false);

            if (!editor._pathToPsaiProject.Contains("Assets/Resources/"))
            {
                Debug.LogError("Could not select the AudioClip. For editing, please move the .psai Project to a subfolder within the 'Assets/Resources' directory of your Unity Project.");
            }
            else
            {
                string path = editor._pathToPsaiProject.Substring(editor._pathToPsaiProject.LastIndexOf("Assets/Resources/"));
                path = Path.GetDirectoryName(path);
                path = path.Remove(0, "Asssets/Resources".Length);
                path += "/" + Path.GetDirectoryName(segment.AudioData.FilePathRelativeToProjectDir) + "/" + Path.GetFileNameWithoutExtension(segment.AudioData.FilePathRelativeToProjectDir);

                _lastSelectedAudioClip = (AudioClip)Resources.Load(path, typeof(AudioClip));
                if (_lastSelectedAudioClip != null)
                {
                    Selection.activeObject = _lastSelectedAudioClip;
                    UpdateSelectionButton();
                    Repaint();
                }
                else
                {
                    Debug.LogError("could not find the following AudioClip within the Resources folder: " + path);
                }
            }
        }

        GUI.enabled = true;

        EditorGUILayout.Separator();
        tmpSegment.Intensity = EditorGUILayout.Slider("Intensity", tmpSegment.Intensity, 0f, 1.0f);
    }

    void OnGUI()
    {
        if (_entity != null)
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            if (_deselectAnyGuiElement)
            {
                string dummyString = "_______Ommi_is_die_beste___";
                GUI.SetNextControlName(dummyString);
                GUI.TextField(new Rect(-100, -100, 1, 1), "");
                GUI.FocusControl(dummyString);
                _deselectAnyGuiElement = false;
            }
            string entityString = Entity == null ? " " : Entity.GetClassString() + " " + Entity.Name;
            EditorGUILayout.LabelField(entityString, EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            if (_tmpEntity is Theme)
            {
                OnGUIforTheme();
            }
            else if (_tmpEntity is Segment)
            {
                OnGUIforSegment();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            if (TempEntityHasBeenEdited() == false)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Apply", GUILayout.MaxWidth(this.position.width / 3), GUILayout.Height(PsaiEditorWindow.LINE_HEIGHT)))
            {
                ApplyChangesToPsaiProject();
            }
            if (GUILayout.Button("Discard", GUILayout.MaxWidth(this.position.width / 3), GUILayout.Height(PsaiEditorWindow.LINE_HEIGHT)))
            {
                PnxHelperz.CopyTo(_entity, _tmpEntity);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }
    }

    private void HandleEvent_ProjectLoaded(object sender, System.EventArgs e)
    {
        _entity = null;
        _lastSelectedAudioClip = null;
    }

    private void UpdateSelectionButton()
    {
        _disableSelectionButton = (Selection.activeObject != null && Selection.activeObject == _lastSelectedAudioClip);
        //Debug.Log("UpdateSelectionButton()  Selection.activeObject=" + Selection.activeObject + "  _last clip=" + _lastSelectedAudioClip + "   ->" + _disableSelectionButton);        
    }

    void OnSelectionChange()
    {
        UpdateSelectionButton();
        Repaint();
    }
}
