//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using UnityEngine;
using System.Collections;
using psai.net;

[System.Serializable]
public class PsaiSoundtrackByProjectLoader : MonoBehaviour
{
    /// <summary>
    /// The the path of your binary soundtrack file as exported by the Psai Editor.
    /// </summary>
    /// <remarks>
    /// Please note that all soundtracks must be located in sub folders of your Unity project's 'Assets/Resources' folder. So e.g. if you exported your soundtrack to 'MyUnityProject/Assets/Resources/MySoundtrack/soundtrack.bytes' , set this variable to 'MySoundtrack/soundtrack.bytes' .
    /// </remarks>
    public string PathToSoundtrackFileWithinResourcesFolder = "";


    void Start()
    {

        if (PathToSoundtrackFileWithinResourcesFolder == null || PathToSoundtrackFileWithinResourcesFolder.Length == 0)
        {
            Debug.LogError("No path to psai soundtrack file set!");
        }
        else
        {            
            PsaiResult psaiResult = PsaiCore.Instance.LoadSoundtrackFromProjectFile(PathToSoundtrackFileWithinResourcesFolder);
            if (psaiResult != PsaiResult.OK)
            {
                Debug.LogError("Failed to load psai soundtrack from path '" + PathToSoundtrackFileWithinResourcesFolder + "' Please make sure the file is located within your Assets/Resources/ folder. [ psai Version: " + PsaiCore.Instance.GetVersion() + "]");
            }
        }
    }
}


