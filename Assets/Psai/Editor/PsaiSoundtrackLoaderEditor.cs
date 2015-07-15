using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(PsaiSoundtrackLoader))]
public class PsaiSoundtrackLoaderEditor : Editor
{
    public TextAsset _textAsset;

    private PsaiSoundtrackLoader _loader = null;

    public override void OnInspectorGUI()
    {
        _loader = target as PsaiSoundtrackLoader;
        _textAsset = EditorGUILayout.ObjectField("drop Soundtrack file here:", _textAsset, typeof(TextAsset), true) as TextAsset;

        if (_textAsset)
        {
            string path = AssetDatabase.GetAssetPath(_textAsset);

            string assetsResources = "Assets/Resources/";
            if (!path.StartsWith(assetsResources))
            {
                Debug.LogError("Failed! Your soundtrack file needs to be located within the 'Assets/Resources' folder along with your audio files. (path=" + path);
            }
            else
            {
                string subPath = path.Substring(assetsResources.Length);
                _loader.pathToSoundtrackFileWithinResourcesFolder = subPath;

                /* This is necessary to tell Unity to update the PsaiSoundtrackLoader */
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(_loader);
                }
            }
        }

        EditorGUILayout.LabelField("Path within Resources folder", _loader.pathToSoundtrackFileWithinResourcesFolder);
    }

}