using UnityEngine;
using UnityEditor;

using psai.Editor;
using System.Collections.Generic;
using System.Text;
using System.IO;

class PsaiImportThemesWindow : EditorWindow
{
    private PsaiProject _project = null;
    Vector2 _scrollPos;
    Dictionary<Theme, bool> _mapSelectedThemes = new Dictionary<Theme, bool>();

    public string PathToOtherProject
    {
        get;
        set;
    }


    PsaiImportThemesWindow()
    {
        this.title = "Import Themes";
        EditorModel.Instance.PsaiProjectLoadedEvent += HandleEvent_ProjectLoaded;
    }

    public PsaiProject ProjectToImportFrom
    {
        get
        {
            return _project;
        }

        set
        {
            if (value != null)
            {
                _project = value;
                _mapSelectedThemes.Clear();
                foreach (Theme theme in _project.Themes)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(theme.Name);
                    sb.Append("   (" + theme.Groups.Count + "  Groups, Segments in total: " + theme.GetSegmentsOfAllGroups().Count);
                    sb.Append(" )");

                    _mapSelectedThemes[theme] = false;
                }
            }
        }        
    }


    void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        string headerString = "Select Themes to import:";
        EditorGUILayout.LabelField(headerString, EditorStyles.boldLabel);
        EditorGUILayout.Separator();

        bool importCanceled = false;
        bool importCompleted = false;

        if (_project != null)
        {
            EditorGUI.indentLevel++;

            bool atLeastOneThemeIsSelected = false;

            foreach (Theme theme in _project.Themes)
            {
                _mapSelectedThemes[theme] = EditorGUILayout.Toggle(theme.Name, _mapSelectedThemes[theme]);
                if (_mapSelectedThemes[theme] == true)
                {
                    atLeastOneThemeIsSelected = true;
                }
            }

            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = atLeastOneThemeIsSelected;
            if (GUILayout.Button("Import", GUILayout.MaxWidth(this.position.width / 3), GUILayout.Height(PsaiEditorWindow.LINE_HEIGHT)))
            {
                List<Theme> themeList = new List<Theme>();
                foreach (Theme theme in _mapSelectedThemes.Keys)
                {
                    if (_mapSelectedThemes[theme] == true)
                    {
                        themeList.Add(theme);
                    }                  
                }

                PsaiProject targetProject = EditorModel.Instance.Project;

                PsaiEditorWindow editor = (PsaiEditorWindow)EditorWindow.GetWindow(typeof(PsaiEditorWindow), false);
                string dirOfTargetProject = Path.GetDirectoryName(editor._pathToPsaiProject);
                string dirOfSourceProject = Path.GetDirectoryName(PathToOtherProject);

                if (!dirOfSourceProject.Equals(dirOfTargetProject))
                {
                    string messageText = "The audio files of the imported Themes will now be copied to your project folder\n";
                    messageText += dirOfTargetProject;
                    messageText += "\nDo you wish to proceed?";

                    if (EditorUtility.DisplayDialog("Copy audio files?", messageText, "OK", "Cancel"))
                    {
                        Debug.Log("...ready to copy " + themeList.Count + " Themes.");

                        List<string> listOfTargetPathsWritten = new List<string>();
                        bool overwriteDialogHasBeenDisplayed = false;
                        bool overwriteFiles = false;

                        foreach (Theme theme in themeList)
                        {
                            HashSet<Segment> segments = theme.GetSegmentsOfAllGroups();
                            Debug.Log("...ready to copy " + segments.Count + " Segments of Theme " + theme.Name);

                            foreach (Segment segment in segments)
                            {
                                string relativeSourcePath = segment.AudioData.FilePathRelativeToProjectDir;
                                string fullSourcePath = dirOfSourceProject + "/" + relativeSourcePath;
                                string fullTargetPath = dirOfTargetProject + "/" + relativeSourcePath;
                                fullTargetPath.Replace(Path.DirectorySeparatorChar, '/');
                                fullSourcePath.Replace(Path.DirectorySeparatorChar, '/');

                                bool fileExists = File.Exists(fullTargetPath);

                                if (!listOfTargetPathsWritten.Contains(fullTargetPath))
                                {
                                    if (!fileExists || (fileExists && overwriteDialogHasBeenDisplayed && overwriteFiles))
                                    {
                                        Debug.Log("copying '" + fullSourcePath + "' -> '" + fullTargetPath + "'");

                                        FileInfo fiSource = new FileInfo(fullSourcePath);
                                        //FileInfo fiTarget = new FileInfo(fullTargetPath);

                                        /* the following line may not work on a Mac and also works on Windows only if the files reside within the Unity Project */
                                        //FileUtil.CopyFileOrDirectory(fullSourcePath, fullTargetPath);

                                        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(fullTargetPath));

                                        fiSource.CopyTo(fullTargetPath, true);

                                        listOfTargetPathsWritten.Add(fullTargetPath);
                                    }
                                    else
                                    {
                                        if (!overwriteDialogHasBeenDisplayed)
                                        {
                                            string messageText2 = "A file already exists:\n" + fullTargetPath;
                                            messageText += "Do you wish to overwrite ALL files in the target direcory?";
                                            if (EditorUtility.DisplayDialog("Overwrite all files?", messageText2, "Overwrite all", "skip all"))
                                            {
                                                overwriteFiles = true;
                                            }
                                            overwriteDialogHasBeenDisplayed = true;
                                        }
                                        if (overwriteDialogHasBeenDisplayed && !overwriteFiles)
                                        {
                                            Debug.Log("skipped '" + segment.AudioData.FilePathRelativeToProjectDir);
                                        }
                                    }
                                }
                            }
                        }
                        AssetDatabase.Refresh();

                        // reimport with default psai import settings
                        bool couldNotApplyThePsaiAudioSettingsToAtLeastOneFile = false;
                        foreach (string tp in listOfTargetPathsWritten)
                        {
                            string assetsResources = "Assets" + "/" + "Resources";
                            int indexOfAssetsResources = tp.IndexOf(assetsResources);
                            if (indexOfAssetsResources >= 0)
                            {
                                string cutPath = tp.Remove(0, indexOfAssetsResources);
                                if (PsaiMultiAudioObjectEditor.ApplyPsaiStandardSettingsToAudioClipAtPath(cutPath) != true)
                                {
                                    Debug.LogError("ApplyPsaiStandardSettingsToAudioClipAtPath (" + cutPath + ") was false");
                                    couldNotApplyThePsaiAudioSettingsToAtLeastOneFile = false;
                                }
                            }
                            else
                            {
                                Debug.LogWarning("indexOfAssetsResources=" + indexOfAssetsResources + "  assetsResources=" + assetsResources);
                            }
                        }

                        if (couldNotApplyThePsaiAudioSettingsToAtLeastOneFile == true)
                        {
                            EditorUtility.DisplayDialog("Failed to import the Audio Files!", "Something went wrong while trying to copy and import the audio files from the source folder '" + dirOfSourceProject + "' to your target folder at '" + dirOfTargetProject + "'. Please copy the related audio files to your target psai Project folder manually, and make sure to preserve the relative directory structure in relation to the .psai project file. Then select the copied audio files in the Unity Project window, right-click and call the psai Multi Audio Object Editor to apply the import settings needed for psai.", "OK");
                        }
                    }
                    else
                    {
                        importCanceled = true;
                    }
                }

                if (!importCanceled)
                {
                    EditorModel.Instance.ImportThemesFromOtherProject(ref targetProject, ProjectToImportFrom, themeList);
                    Debug.Log("Import completed.");
                    importCompleted = true;
                }               
            }
            GUI.enabled = true;

            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(this.position.width / 3), GUILayout.Height(PsaiEditorWindow.LINE_HEIGHT)))
            {
                importCanceled = true;
            }

            EditorGUILayout.EndHorizontal();  // throws an Exception in some cases for some reasons
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndScrollView();

        if (importCanceled || importCompleted)
        {
            this.Close();
        }
    }

    void OnClose()
    {
        this._project = null;
    }

    private void HandleEvent_ProjectLoaded(object sender, System.EventArgs e)
    {
        _project = null;
    }
}
