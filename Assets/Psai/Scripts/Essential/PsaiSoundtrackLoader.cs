//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;
using psai.net;

[System.Serializable]
public class PsaiSoundtrackLoader : MonoBehaviour 
{
    /// <summary>
    /// The the path of your binary soundtrack file as exported by the Psai Editor.
    /// </summary>
    /// <remarks>
    /// Please note that all soundtracks must be located in sub folders of your Unity project's 'Assets/Resources' folder. So e.g. if you exported your soundtrack to 'MyUnityProject/Assets/Resources/MySoundtrack/soundtrack.bytes' , set this variable to 'MySoundtrack/soundtrack.bytes' .
    /// </remarks>
    public string pathToSoundtrackFileWithinResourcesFolder;


    public void Start()
    {
        LoadSoundtrack();
    }


    private void LoadSoundtrack()
    {      
        if (pathToSoundtrackFileWithinResourcesFolder == null || pathToSoundtrackFileWithinResourcesFolder.Length == 0)
        {
            Debug.LogError("No path to psai soundtrack file set!");
        }
        else
        {
            PsaiResult psaiResult = PsaiResult.error_file;

            if (pathToSoundtrackFileWithinResourcesFolder.EndsWith(".xml"))
            {
                psaiResult = PsaiCore.Instance.LoadSoundtrackFromProjectFile(pathToSoundtrackFileWithinResourcesFolder);
            }
            else if (pathToSoundtrackFileWithinResourcesFolder.EndsWith(".bytes"))
            {
                Debug.LogWarning("Binary psai soundtrack files are deprecated in psai 1.6.0! Instead, you can now load psai Project files directly by xml. To do so, rename your 'MyProject.psai' file to 'MyProject.xml' . Please make sure that there is no 'MyProject.bytes' or 'MyProject.psai' file in the same directory, as this might cause problems at runtime due to Unity ignoring file extensions.");
                psaiResult = PsaiCore.Instance.LoadSoundtrack(pathToSoundtrackFileWithinResourcesFolder);
            }

            
            if (psaiResult != PsaiResult.OK)
            {
                Debug.LogError("Failed to load psai soundtrack from path '" + pathToSoundtrackFileWithinResourcesFolder + "' Please make sure the file is located within your Assets/Resources/ folder. [ psai Version: " + PsaiCore.Instance.GetVersion() + "]");
            }
        }
    }
} 

