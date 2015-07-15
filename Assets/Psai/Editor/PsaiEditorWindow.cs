using UnityEngine;
using UnityEditor;

using psai.Editor;
using System.Collections.Generic;
using System.IO;

public class PsaiEditorWindow : EditorWindow
{
    public static readonly int BUTTON_MARGIN = 10;
    public static int LINE_HEIGHT = 20;
    //private static readonly string DEFAULT_PATH_TO_PSAI_PROJECT = "./Assets/Resources/PsaiDemoSoundtrack/PsaiDemoSoundtrack.psai";
    //private static readonly string DEFAULT_PATH_TO_SOUNDTRACK_FILE = "./Assets/Resources/PsaiDemoSoundtrack/soundtrack.bytes";

    //public string _pathToPsaiProject = DEFAULT_PATH_TO_PSAI_PROJECT;
    public string _pathToPsaiProject = "";
    //public string _pathToBinarySoundtrackFile = DEFAULT_PATH_TO_SOUNDTRACK_FILE;
    public string _pathToBinarySoundtrackFile = "";

    private static readonly UnityEngine.Color SELECTION_COLOR = UnityEngine.Color.green;
   
    Vector2 _projectScrollPos = Vector2.zero;
    Dictionary<PsaiMusicEntity, bool> _mapFoldout = new Dictionary<PsaiMusicEntity, bool>();
    bool _projectIsLoading;    
    PsaiEntityEditor _entityEditor;
    PsaiImportThemesWindow _importThemesWindow;
    GenericMenu _fileMenu;
    GenericMenu _contextMenu;

    // Add menu to the Window menu
    [MenuItem("Window/psai Editor (Light Edition)")]
    static void Init()
    {        
        // Get existing open window or if none, make a new one:
        EditorWindow.GetWindow<PsaiEditorWindow>();
    }


    public PsaiEditorWindow()
    {
        _projectIsLoading = false;
        this.title = "Psai Editor LE";
        EditorModel.Instance.EventNewMessageToGui += HandleEvent_UnityNewMessageForGui;        
        EditorModel.Instance.PsaiEntityAddedEvent += HandleEvent_PsaiEntityAdded;
        EditorModel.Instance.EventImportCompleted += HandleEvent_ImportCompleted;
    }


    void BuildContextMenu(PsaiMusicEntity entity)
    {
        //Debug.Log("BuildContextMenu()");
        _contextMenu = new GenericMenu();
        _contextMenu.AddItem(new GUIContent("Delete " + entity.GetClassString() + " " + entity.Name), false, ContextMenuCallback_Delete, entity);
    }


    void BuildFileMenu()
    {
        _fileMenu = new GenericMenu();
        _fileMenu.AddItem(new GUIContent("New psai Soundtrack"), false, FileMenuCallback, "new");
        _fileMenu.AddItem(new GUIContent("Open psai Soundtrack"), false, FileMenuCallback, "open");
        if (EditorModel.Instance.ProjectDataChangedSinceLastSave)
        {
            _fileMenu.AddItem(new GUIContent("Save"), false, FileMenuCallback, "save");
        }
        else
        {
            _fileMenu.AddDisabledItem(new GUIContent("Save"));
        }

        if (EditorModel.Instance.Project != null)
        {
            _fileMenu.AddItem(new GUIContent("Save As"), false, FileMenuCallback, "saveas");
        }
        else
        {
            _fileMenu.AddDisabledItem(new GUIContent("Save As"));
        }


        _fileMenu.AddSeparator("");

        if (EditorModel.Instance.Project != null)
        {
            _fileMenu.AddItem(new GUIContent("Import Theme(s) from other psai Soundtrack"), false, FileMenuCallback, "import");
        }
        else
        {
            _fileMenu.AddDisabledItem(new GUIContent("Import Theme(s) from other psai Soundtrack"));
        }
        
        /*
        _fileMenu.AddSeparator("");
        if (EditorModel.Instance.Project != null && EditorModel.Instance.Project.Themes.Count > 0) 
        {
            _fileMenu.AddItem(new GUIContent("Build Soundtrack"), false, FileMenuCallback, "export");
        }
        else
        {
            _fileMenu.AddDisabledItem(new GUIContent("Build Soundtrack"));
        } 
         */
    }

    public void ShowFileCollisionDialog(string otherFilename)
    {
        EditorUtility.DisplayDialog("File collision!", "Sorry, another file of the same name (but different extension) already exists in the target folder. As this will cause problems at runtime, please choose another filename or rename/remove the file '" + otherFilename + "' from the target folder.\nThe soundtrack has not been saved.", "OK");
    }


    public void OnNewProjectClicked()
    {
        if (CheckForUnsavedChangesAndDisplayMessageBoxToDiscard())
        {
            string path = EditorUtility.SaveFilePanel("New psai Project", Application.dataPath + "/Resources/", "", "xml");
            if (path.Length != 0)
            {
                if (EditorModel.CheckIfPathIsWithinSubdirOfAssetsResources(path))
                {
                    _pathToPsaiProject = path;
                    string normalizedPath = path.Replace('/', Path.DirectorySeparatorChar);
                    Debug.Log("Creating new psai Project at path=" + normalizedPath);
                    EditorModel.Instance.NewProject(normalizedPath);
                    //EditorModel.Instance.SaveProjectData(_pathToPsaiProject);
                }
                else
                {
                    EditorUtility.DisplayDialog("Not allowed!", "Sorry, currently the psai Editor LE only supports projects which reside somewhere within the 'Assets/Resources' folder of your Unity project.", "OK");
                }
            } 
        }
    }


    /// <summary>
    /// Takes the  full file path to an Asset within a Unity project, and returns the truncated path starting with "Assets", e.g. "Assets/SomeFolder/SomeFile". Using '/' as the directory separator.
    /// </summary>
    /// <param name="fullpath"></param>
    /// <returns></returns>
    internal static string GetRelativePathOfAssetFromFullPath(string fullpath)
    {
        string fullConvertedPath = fullpath.Replace(Path.DirectorySeparatorChar, '/');
        fullConvertedPath = fullConvertedPath.Replace(Path.AltDirectorySeparatorChar, '/');

        int relativeIndex = fullConvertedPath.LastIndexOf("/Assets/");
        string result = fullConvertedPath.Substring(++relativeIndex);
        return result;
    }

    /// <summary>
    /// If we save without reimporting, Unity will not reflect the changes, although the file is correctly written.
    /// </summary>
    /// <param name="fullPathToPsaiProject"></param>
    private void SaveAndReimportProject(string fullPathToPsaiProject)
    {
        EditorModel.Instance.SaveProject(_pathToPsaiProject);
        string relativePath = GetRelativePathOfAssetFromFullPath(fullPathToPsaiProject);
        AssetDatabase.ImportAsset(relativePath);    // this way we force a cache update
        Debug.Log("psai Project saved.");
    }


    void FileMenuCallback (object obj) 
    {
        if (obj.Equals("new"))
        {
            OnNewProjectClicked();
        }                
        else if (obj.Equals("open"))
        {
            OnOpenProjectClicked();
        }
        else if (obj.Equals("save"))
        {
            SaveAndReimportProject(_pathToPsaiProject);
        }
        else if (obj.Equals("saveas"))
        {
            string path = EditorUtility.SaveFilePanel("Save psai Soundtrack", Path.GetDirectoryName(_pathToPsaiProject), "", "xml");
            if (path.Length != 0)
            {

                if (EditorModel.CheckIfFileOfTheSameNameExistsWithAnotherExtension(path, "psai"))
                {
                    string otherFilename = Path.GetFileNameWithoutExtension(path) + ".psai";
                    ShowFileCollisionDialog(otherFilename);
                }
                else
                {
                    _pathToPsaiProject = path;
                    SaveAndReimportProject(path);
                }
            }  
        }
        else if (obj.Equals("import"))
        {
            OnImportThemesFromProjectClicked();
        }
        else if (obj.Equals("export"))
        {
            //OnBuildSoundtrackClicked();
        }
	}

    void ContextMenuCallback_Delete(object obj)
    {
        if (obj != null && obj is PsaiMusicEntity)
        {
            PsaiMusicEntity entity = obj as PsaiMusicEntity;
            string entityString = entity.GetClassString() + " " + entity.Name;
            if (EditorUtility.DisplayDialog("Delete " + entityString, "You cannot undo this action. Are you sure you wish to delete " + entityString + " ?" , "Delete", "Cancel"))
            {                
                if (_entityEditor.Entity == entity)
                {             
                    _entityEditor.Entity = null;                 
                }
                EditorModel.Instance.CreateAndExecuteNewCommand_DeletePsaiEntity(EditorModel.Instance.Project, entity);               
            }

            if (entity is Theme)
            {
                if (EditorModel.CheckIfPathIsWithinSubdirOfAssetsResources(EditorModel.Instance.ProjectDir))
                {
                    Theme theme = entity as Theme;
                    if (EditorUtility.DisplayDialog("Delete audio files?", "Do you also wish to physically delete the audio files that are exclusively used by Theme " + theme.Name + " from the Resources folder? Warning, you cannot undo this Action!", "Delete", "Keep"))
                    {
                        HashSet<string> audioPathsUsedByOtherThemes = new HashSet<string>();

                        foreach (Theme tmpTheme in EditorModel.Instance.Project.Themes)
                        {
                            if (tmpTheme != theme)
                            {
                                audioPathsUsedByOtherThemes.UnionWith(tmpTheme.GetAudioDataRelativeFilePathsUsedByThisTheme());
                            }
                        }

                        HashSet<string> audioDatas = theme.GetAudioDataRelativeFilePathsUsedByThisTheme();
                        foreach (string audioPath in audioDatas)
                        {
                            if (!audioPathsUsedByOtherThemes.Contains(audioPath))
                            {
                                string path = EditorModel.Instance.ProjectDir + "/" + audioPath;

                                bool result = FileUtil.DeleteFileOrDirectory(path);
                                if (result)
                                {
                                    Debug.Log("deleted: " + path);
                                    string metaFilePath = path + ".meta";
                                    bool metaResult = FileUtil.DeleteFileOrDirectory(metaFilePath);
                                    if (!metaResult)
                                    {
                                        Debug.LogWarning("could not delete " + metaFilePath);
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning("could not delete " + path);
                                }
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("skipped deletion of {0} as it is still being used in at least one other Theme.", audioPath));
                            }
                        }
                    }
                }
            }
        }
    }


    private void OnImportThemesFromProjectClicked()
    {       
        //string directory = System.IO.Path.GetDirectoryName(_pathToPsaiProject);
        //string fileName = System.IO.Path.GetFileName(_pathToPsaiProject);        

        string path = EditorUtility.OpenFilePanel("select psai Project", Path.GetDirectoryName(_pathToPsaiProject), "xml");
        if (path.Length != 0)
        {
            psai.Editor.PsaiProject projectToImportFrom = psai.Editor.PsaiProject.LoadProjectFromXmlFile(path);

            if (projectToImportFrom != null)
            {
                _importThemesWindow = (PsaiImportThemesWindow)EditorWindow.GetWindow(typeof(PsaiImportThemesWindow), false);
                _importThemesWindow.ProjectToImportFrom = projectToImportFrom;
                _importThemesWindow.PathToOtherProject = path;
            }                    
        }  
    }

    void OnOpenProjectClicked()
    {

        if (CheckForUnsavedChangesAndDisplayMessageBoxToDiscard())
        {
            string initialDirectory = "";
            if (_pathToPsaiProject != null && _pathToPsaiProject.Length > 0)
            {
                initialDirectory = System.IO.Path.GetDirectoryName(_pathToPsaiProject);
            }
            else
            {
                initialDirectory = Application.dataPath + "/Resources/";
            }

            //Debug.Log("OpenProjectClicked() initialDirectory=" + initialDirectory);

            string path = EditorUtility.OpenFilePanel("Open psai Soundtrack", initialDirectory, "xml");
            if (path.Length != 0)
            {
                _projectIsLoading = true;
                Repaint();

                Debug.Log("Loading psai project file from path: " + path);
                EditorModel.Instance.LoadProjectData(path);
                _pathToPsaiProject = path;
                BuildCaches();

                _projectIsLoading = false;
                Repaint();
            }   
        }
    }


    void OnGUI()
    {
        if (_projectIsLoading)
        {
            GUILayout.Label("PLEASE WAIT...", EditorStyles.boldLabel);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        string loadButtonString = "File";
        if (GUILayout.Button(loadButtonString, GUILayout.MaxWidth(GUI.skin.label.CalcSize(new GUIContent(loadButtonString)).x + BUTTON_MARGIN), GUILayout.Height(LINE_HEIGHT)))
        {
            //string path = _pathToPsaiProject;
            BuildFileMenu();
            _fileMenu.DropDown(new Rect(0, LINE_HEIGHT, 0, 0));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        if (EditorModel.Instance.Project == null)
        {
            EditorGUILayout.LabelField("No psai Project loaded", EditorStyles.boldLabel);
            EditorGUILayout.Separator();
        }
        else
        {
            _projectScrollPos = EditorGUILayout.BeginScrollView(_projectScrollPos);

            string projectName = "Project " + Path.GetFileNameWithoutExtension(_pathToPsaiProject);
            EditorGUILayout.LabelField(projectName, EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            if (EditorModel.Instance.Project.Themes.Count == 0)
            {
                EditorGUILayout.LabelField("- No Themes imported yet -");
            }
            else
            {
                foreach (Theme theme in EditorModel.Instance.Project.Themes)
                {
                    // Handling events
                    // http://answers.unity3d.com/questions/684414/custom-editor-foldout-doesnt-unfold-when-clicking.html

                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.GetControlRect(true, 16f, EditorStyles.foldout);
                    Rect foldRect = GUILayoutUtility.GetLastRect();

                    if (Event.current.type == EventType.MouseUp && foldRect.Contains(Event.current.mousePosition))
                    {
                        //_mapFoldout[theme] = !_mapFoldout[theme];

                        _entityEditor = (PsaiEntityEditor)EditorWindow.GetWindow(typeof(PsaiEntityEditor), false);
                        _entityEditor.Entity = theme;

                        GUI.changed = true;
                        //Event.current.Use();
                    }

                    if (Event.current.type == EventType.ContextClick && foldRect.Contains(Event.current.mousePosition))
                    {
                        BuildContextMenu(theme);
                        _contextMenu.ShowAsContext();
                    }

                    if (_entityEditor != null && _entityEditor.Entity != null && _entityEditor.Entity == theme)
                    {
                        UnityEngine.Color oldContentColor = GUI.contentColor;
                        GUI.contentColor = SELECTION_COLOR;
                        if (_mapFoldout.ContainsKey(theme))
                        {
                            //_mapFoldout[theme] = EditorGUI.Foldout(foldRect, _mapFoldout[theme], theme.Name, EditorStyles.foldoutPreDrop);
                            _mapFoldout[theme] = EditorGUI.Foldout(foldRect, _mapFoldout[theme], theme.Name);
                        }
                        GUI.contentColor = oldContentColor;
                    }
                    else
                    {
                        if (_mapFoldout.ContainsKey(theme))
                        {
                            _mapFoldout[theme] = EditorGUI.Foldout(foldRect, _mapFoldout[theme], theme.Name);
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                    }

                    if (_mapFoldout[theme])
                    {
                        EditorGUI.indentLevel++;

                        foreach (Group group in theme.Groups)
                        {
                            EditorGUI.BeginChangeCheck();
                            foldRect = GUILayoutUtility.GetLastRect();

                            if (Event.current.type == EventType.MouseUp && foldRect.Contains(Event.current.mousePosition))
                            {
                                //_mapFoldout[group] = !_mapFoldout[group];
                                //GUI.changed = true;
                                //Event.current.Use();
                            }
                            if (_mapFoldout.ContainsKey(group))
                            {
                                _mapFoldout[group] = EditorGUILayout.Foldout(_mapFoldout[group], group.Name);
                            }

                            if (EditorGUI.EndChangeCheck())
                            {
                                //Debug.Log("Group foldout changed");
                            }

                            if (_mapFoldout[group])
                            {
                                EditorGUI.indentLevel++;

                                foreach (Segment segment in group.Segments)
                                {
                                    string segmentString = segment.ToString();
                                    //EditorGUILayout.LabelField(segmentString);

                                    if (_entityEditor != null && _entityEditor.Entity != null && _entityEditor.Entity == segment)
                                    {
                                        UnityEngine.Color oldContentColor = GUI.contentColor;
                                        GUI.contentColor = SELECTION_COLOR;
                                        EditorGUILayout.LabelField(segmentString);
                                        //EditorGUILayout.LabelField(segmentString, EditorStyles.foldoutPreDrop);
                                        GUI.contentColor = oldContentColor;
                                    }
                                    else
                                    {
                                        EditorGUILayout.LabelField(segmentString);
                                    }

                                    EditorGUI.BeginChangeCheck();
                                    //EditorGUILayout.GetControlRect(true, 16f, EditorStyles.label);                                
                                    foldRect = GUILayoutUtility.GetLastRect();

                                    if (Event.current.type == EventType.MouseUp && foldRect.Contains(Event.current.mousePosition))
                                    {
                                        _entityEditor = (PsaiEntityEditor)EditorWindow.GetWindow(typeof(PsaiEntityEditor), false);
                                        if (_entityEditor != null)
                                        {
                                            _entityEditor.Entity = segment;
                                        }

                                        GUI.changed = true;

                                        //Event.current.Use();
                                    }

                                    if (Event.current.type == EventType.ContextClick && foldRect.Contains(Event.current.mousePosition))
                                    {
                                        BuildContextMenu(theme);
                                        _contextMenu.ShowAsContext();
                                    }



                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        //Debug.Log("Segment clicked: " + segmentString);
                                    }
                                }
                                EditorGUI.indentLevel--;
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }       
            
            EditorGUILayout.EndScrollView();
        }
    }

    private void OnBuildSoundtrackClicked()
    {
        if (_entityEditor != null)
        {
            _entityEditor.CheckForChangesAndApplyToProjectIfConfirmed();
        }

        string defaultPath = "";
        if (_pathToBinarySoundtrackFile != null && _pathToBinarySoundtrackFile.Length > 0)
        {
            defaultPath = Path.GetDirectoryName(_pathToBinarySoundtrackFile);
        }
        else if (EditorModel.CheckIfPathIsWithinSubdirOfAssetsResources(_pathToPsaiProject))
        {
            defaultPath = Path.GetDirectoryName(_pathToPsaiProject);
        }
        else
        {
            defaultPath = Application.dataPath + "/Resources/";
        }

        //Debug.Log("OnBuildSoundtrackClicked() defaultPath=" + defaultPath);

        _pathToBinarySoundtrackFile =  EditorUtility.SaveFilePanel("Please choose a name for the Soundtrack file",
                                defaultPath,
								"soundtrack.xml",
								"xml"
								);

        if (_pathToBinarySoundtrackFile.Length > 0)
        {
            if (EditorModel.CheckIfPathIsWithinSubdirOfAssetsResources(_pathToBinarySoundtrackFile) == false)
            {
                EditorUtility.DisplayDialog("Not allowed!", "Sorry, but your Soundtrack file needs to reside within the 'Resources' folder of your Unity project!", "OK");
            }
            else if (EditorModel.CheckIfFileOfTheSameNameExistsWithAnotherExtension(_pathToBinarySoundtrackFile, "bytes"))
            {
                string otherFileName = Path.GetFileNameWithoutExtension(_pathToBinarySoundtrackFile) + "." + "bytes";
                ShowFileCollisionDialog(otherFileName);
            }
            else
            {                
                try
                {
                    EditorModel.Instance._project.SaveAsXmlFile(_pathToBinarySoundtrackFile);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(string.Format("could not save file: {0} \n Exception: ", _pathToBinarySoundtrackFile, ex.ToString()));
                }

                Debug.Log("Exported Soundtrack to " + _pathToBinarySoundtrackFile);

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Done!", "Export completed.", "OK");
            }

        }        

    }

    void BuildCaches()
    {
        if (EditorModel.Instance.Project != null)
        {
            _mapFoldout.Clear();
            foreach (Theme theme in EditorModel.Instance.Project.Themes)
            {
                _mapFoldout[theme] = false;
                foreach(Group group in theme.Groups)
                {
                    _mapFoldout[group] = false;
                }
            }
        }        
    }


    /// <summary>
    /// Returns true if there were no unsaved changes or the user clicked 'Discard'
    /// </summary>
    /// <returns></returns>
    bool CheckForUnsavedChangesAndDisplayMessageBoxToDiscard()
    {
        if (EditorModel.Instance.ProjectDataChangedSinceLastSave)
        {
            string text = "Your current psai Soundtrack contains unsaved changes. Do you wish to continue and discard your changes?";
            return EditorUtility.DisplayDialog("Discard changes?", text, "Discard", "Cancel");
        }
        else
        {
            return true;
        }
    }


    void OnEnable()
    {
        EditorModel.Instance.Project = null;
    }
    
    void OnDisable()
    {
        if (EditorModel.Instance.ProjectDataChangedSinceLastSave)
        {           
            if (EditorUtility.DisplayDialog("Save psai Soundtrack?", "Continuing will discard the changes you have made to your psai Soundtrack. Do you wish to save or discard your changes?", "Save", "Discard"))
            {
                SaveAndReimportProject(_pathToPsaiProject);
                
                /*
                if (EditorUtility.DisplayDialog("Build soundtrack?", "To reflect the changes made to your psai Project, you need to export your project to the binary soundtrack file loaded by the 'PsaiSoundtrackLoader' component of the 'Psai' game object in your Scene. Do you wish to export now?", "Build", "Skip"))
                {
                    this.OnBuildSoundtrackClicked();
                }
                */
            }
        }

        //Debug.Log("OnDisable() " + this.GetInstanceID());
    }

    void OnDestroy()
    {
        //Debug.Log("OnDestroy()");
    }

    void OnProjectChange()
    {
        //Debug.Log("OnProjectChange()");
    }    

    private void HandleEvent_UnityNewMessageForGui(object sender, EventArgs_NewMessageToGui e)
    {
        switch (e.MessageType)
        {

            default:
                {
                    UnityEngine.Debug.Log(e.Message);
                }
                break;
        }
    }

    private void HandleEvent_PsaiEntityAdded(object sender, EventArgs_PsaiEntityAdded e)
    {
        if (e.Entity is Theme)
        {
            BuildCaches();
        }
    }

    private void HandleEvent_ProjectLoaded(object sender, System.EventArgs e)
    {
        //Debug.Log("PsaiEditorWindow::HandleEvent_ProjectLoaded()");
    }

    private void HandleEvent_ImportCompleted(object sender, System.EventArgs e)
    {
        Debug.Log("Import complete.");
    }
}