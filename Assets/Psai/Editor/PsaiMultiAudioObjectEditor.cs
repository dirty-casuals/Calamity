using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;

public class PsaiMultiAudioObjectEditor : EditorWindow
{

#if !(UNITY_4_3 || UNITY_4_4 ||UNITY_4_5 || UNITY_4_6)
    public static bool ApplyPsaiStandardSettingsToAudioClipAtPath(string path)
    {
        return true;
    }
#else

    static readonly int COMPRESSION_KBPS_MIN = 45;
    static readonly int COMPRESSION_KBPS_MAX = 500;
    static readonly AudioImporterFormat PSAI_DEFAULT_FORMAT = AudioImporterFormat.Compressed;
    static readonly AudioImporterLoadType PSAI_DEFAULT_LOADTYPE = AudioImporterLoadType.StreamFromDisc;
    static readonly bool PSAI_DEFAULT_MONO = false;
    static readonly bool PSAI_DEFAULT_3D = false;
    static readonly bool PSAI_DEFAULT_CREATE_WRAPPERS = true;

    Object[] _selectedAudioClips = null;

    bool _applyClicked;
    bool _scanningInitiated;

    bool _usePsaiDefaults = true;
    int _compression = COMPRESSION_KBPS_MAX;
    bool _3dSound;
    bool _forceToMono;
    AudioImporterFormat _format;
    AudioImporterLoadType _loadType;

    bool _createWrapperPrefabs;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Assets/psai Multi Audio Object Editor")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        EditorWindow.GetWindow(typeof(PsaiMultiAudioObjectEditor));
    }


    public static bool ApplyPsaiStandardSettingsToAudioClipAtPath(string path)
    {
        //Debug.Log("ApplyPsaiStandardSettingsToAudioClipAtPath() path=" + path);

        if (!File.Exists(path))
        {
            Debug.LogError("Error! Could not find the following file: " + path);
            return false;
        }
        else
        {
            AudioImporter ai = AssetImporter.GetAtPath(path) as AudioImporter;

            if (ai != null)
            {
                ai.format = PSAI_DEFAULT_FORMAT;
                ai.loadType = PSAI_DEFAULT_LOADTYPE;
                ai.forceToMono = PSAI_DEFAULT_MONO;
                ai.threeD = PSAI_DEFAULT_3D;
                ai.compressionBitrate = COMPRESSION_KBPS_MAX * 1000;
                AssetDatabase.ImportAsset(path);
                return true;
            }
            else
            {
                Debug.LogError("Error! Found the file, but could not start the AudioImporter on file at path '" + path + "' . Please make sure this is a valid audio file.");
                return false;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        _applyClicked = false;
        _scanningInitiated = false;
        SetPsaiDefaultValues();
    }


    void SetPsaiDefaultValues()
    {
        _3dSound = PSAI_DEFAULT_3D;
        _forceToMono = PSAI_DEFAULT_MONO;
        _format = PSAI_DEFAULT_FORMAT;
        _loadType = PSAI_DEFAULT_LOADTYPE;

        _createWrapperPrefabs = PSAI_DEFAULT_CREATE_WRAPPERS;
    }

    void OnSelectionChange()
    {               
    }

    void OnGUI()
    {
        if (_scanningInitiated)
        {            
            GUILayout.Label("PLEASE WAIT! Scanning selection...", EditorStyles.boldLabel);
            return;
        }

        GUILayout.Label("psai Multi Audio Object Editor", EditorStyles.boldLabel);

        _usePsaiDefaults = EditorGUILayout.Toggle("use psai settings", _usePsaiDefaults);
        if (_usePsaiDefaults)
        {
            GUI.enabled = false;
            SetPsaiDefaultValues();
        }

        _format = (AudioImporterFormat)EditorGUILayout.EnumPopup("Audio Format", _format);
        _3dSound = EditorGUILayout.Toggle("3D Sound", _3dSound);
        _forceToMono = EditorGUILayout.Toggle("Force to mono", _forceToMono);
        _loadType = (AudioImporterLoadType)EditorGUILayout.EnumPopup("Load type", _loadType);

        _createWrapperPrefabs = EditorGUILayout.Toggle("use Wrappers", _createWrapperPrefabs);

        GUI.enabled = true;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Compression (kbps)");
        _compression = EditorGUILayout.IntSlider(_compression, COMPRESSION_KBPS_MIN, COMPRESSION_KBPS_MAX);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Apply", GUILayout.Width(position.width / 2), GUILayout.Height(40)))
        {
            _applyClicked = true;
        }
    }

    void Update()
    {
        if (_applyClicked)
        {
            _applyClicked = false;
            _scanningInitiated = true;
            Repaint();
        }
        else if (_scanningInitiated)
        {
            _selectedAudioClips = GetSelectedAudio();
            _scanningInitiated = false;
            Repaint();
  
            if (_selectedAudioClips != null && _selectedAudioClips.Length > 0)
            {
                if (EditorUtility.DisplayDialog("Apply Audio Settings?", "Your selection (including subfolders) contains " + _selectedAudioClips.Length.ToString() + " Audio Objects. Are you sure you wish to apply?", "OK", "Cancel") == true)
                {
                    ApplySettingsToSelectedAudioClips();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Empty Selection!", "There are no Audio Objects selected in your Project View. Please select them directly or their containing parent folder, then click 'Apply' again.", "OK");
            }
        }
    }

    static Object[] GetSelectedAudio()
    {
        Object[] result = Selection.GetFiltered(typeof(AudioClip), SelectionMode.DeepAssets);
        return result;
    }

    void ApplySettingsToSelectedAudioClips()
    {       
        foreach (AudioClip audioClip in _selectedAudioClips)
        {
            string path = AssetDatabase.GetAssetPath(audioClip);
            AudioImporter ai = AssetImporter.GetAtPath(path) as AudioImporter;

            ai.format = _format;
            ai.loadType = _loadType;
            ai.forceToMono = _forceToMono;
            ai.threeD = _3dSound;
            ai.compressionBitrate = _compression * 1000;

            AssetDatabase.ImportAsset(path);

            string pathOfAudioClip = AssetDatabase.GetAssetPath(audioClip.GetInstanceID());
            string prefabPath = Path.GetDirectoryName(pathOfAudioClip) + "/" + audioClip.name + "_go.prefab";

            if (_createWrapperPrefabs)
            {
                GameObject goWrapper = new GameObject();
                goWrapper.name = audioClip.name;
                PsaiAudioClipWrapper coWrapper = goWrapper.AddComponent<PsaiAudioClipWrapper>();
                coWrapper._audioClip = audioClip;                                
                PrefabUtility.CreatePrefab(prefabPath, goWrapper);
                GameObject.DestroyImmediate(goWrapper);
            }
            else
            {
                if (File.Exists(prefabPath))
                {
                    
                    bool delSuccess = FileUtil.DeleteFileOrDirectory(prefabPath);
                    if (delSuccess)
                    {
                        Debug.Log("successfully deleted Wrapper prefab " + prefabPath);
                    }
                    else
                    {
                        Debug.LogWarning("Wrapper prefab " + prefabPath + " could not be deleted. Please delete it manually in the Project window to save some disk space. It's not harmful though.");
                    }
                    
                }
            }
        }
        AssetDatabase.Refresh();
        EditorApplication.RepaintProjectWindow();
    }
#endif
}
