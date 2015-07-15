using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Globalization;

using psai.net;

#if (PSAI_EDITOR_STANDALONE)
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
#endif

namespace psai.Editor
{

#if(!PSAI_EDITOR_STANDALONE)
    // fake implementation of BackgroundWorker as it does not exist within Unity
    public class BackgroundWorker
    {
        public void ReportProgress(int i) { }
    }
#endif

    internal delegate void ProgressBarProcessStart(string caption);
    internal delegate void ProgressBarProcessUpdate(float totalProgressInPercent);
    internal delegate void ProgressBarProcessEnd(bool success, string message);

    [Serializable]
    public class EditorModel
    {
        // Singleton
        private static EditorModel _instance;
        public static EditorModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EditorModel();
                }

                return _instance;
            }

            private set
            {
                _instance.InitCommandQueue();
            }

        }
        //---


        internal static readonly int SNIPPET_INTENSITY_MINVALUE = 1;     // in percent
        internal static readonly int SNIPPET_INTENSITY_MAXVALUE = 100;

        internal static readonly int WARNING_THRESHOLD_PREBEAT_MILLISECONDS  = 1500;
        internal static readonly int WARNING_THRESHOLD_INDIRECTION_STEPS     = 2;        //TODO: als Parameter in die Projekt-Settings?


        internal static bool CheckIfFileLiesWithinSubfolderOfDirectory(string folder, string filepath)
        {
            DirectoryInfo di1 = new DirectoryInfo(folder);
            DirectoryInfo di2 = new DirectoryInfo(filepath);
            bool isParent = false;
            while (di2.Parent != null)
            {
                if (di2.Parent.FullName == di1.FullName)
                {
                    isParent = true;
                    break;
                }
                else di2 = di2.Parent;
            }

            return isParent;
        }


        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs_SegmentMovedToGroup> SegmentMovedToGroupEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs_PsaiEntityPropertiesChanged> PsaiEntityPropertiesChangedEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs_CompatibilitySettingChanged> CompatibilitySettingChangedEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs_BridgeSegmentToggled> BridgeSegmentToggledEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs_OperationFailed> OperationFailedEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs_PsaiEntityAdded> PsaiEntityAddedEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs_PsaiEntityDeleted> PsaiEntityDeletedEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs> PsaiProjectLoadedEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs> PsaiProjectReloadedEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs> CommandQueueChangedEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs> ProjectPropertiesChangedEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs> ProjectSavedEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs> ProjectDataChangedEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs> NewProjectInitializedEvent;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs_NewMessageToGui> EventNewMessageToGui;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs_ProgressBarStart> EventProgressBarStart;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs_ProgressBarUpdate> EventProgressBarUpdate;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs_AuditOrExportResult> EventAuditCompleted;
        [field: NonSerializedAttribute()]
        public event EventHandler<EventArgs_AuditOrExportResult> EventExportCompleted;
        [field: NonSerializedAttribute()]
        public event EventHandler EventImportCompleted;
        [field: NonSerializedAttribute()]
        public event EventHandler<FileSystemEventArgs> EventFileInProjectDirChanged;
        [field: NonSerializedAttribute()]
        private List<ICommand> _commandQueue;

        [field: NonSerializedAttribute()]
        private int _commandQueueUndoIndex;   // points to the last command to undo or to redo

        [field: NonSerializedAttribute()]
        private bool _projectDataChangedSinceLastSave = false;

        [field: NonSerializedAttribute()]
        private bool _projectDataChangedSinceLastAudit = false;

        [field: NonSerializedAttribute()]
        public PsaiProject _project = new PsaiProject();

        [field: NonSerializedAttribute()]
        private EditorPreferences _preferences;

        private string _directxSdkPath = "";


#if (PSAI_EDITOR_STANDALONE)

        [field: NonSerializedAttribute()]
        private BackgroundWorker _bgWorkerProjectAudit;

        [field: NonSerializedAttribute()]
        private BackgroundWorker _bgWorkerExportToUnity;

        [field: NonSerializedAttribute()]
        private BackgroundWorker _bgWorkerImportThemes;

        [field: NonSerializedAttribute()]
        private FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();

     

#endif

        [field: NonSerializedAttribute()]
        private EventArgs_AuditOrExportResult _eventArgsOfAuditOrExportRun;

        public string ProjectDir
        {
            get
            {
                if (FullProjectPath != null && FullProjectPath.Length > 0)
                {
                    return System.IO.Path.GetDirectoryName(FullProjectPath);
                }
                return null;
            }
        }

        public PsaiProject Project
        {
            get { return _project; }
            set { _project = value; }
        }

        internal bool ExportInitiated
        {
            get;
            private set;
        }


        public EditorPreferences Preferences
        {
            get
            {
                if (_preferences == null)
                {
                    string prefPath = GetPathOfPreferencesFileAndCreateSubdirIfItDoesNotExist();
                    _preferences = DeserializeXmlPreferences(prefPath);

                    if (_preferences == null)
                    {
                        _preferences = new EditorPreferences();
                    }
                }
                return _preferences;
            }

            private set
            {
                _preferences = value;
            }
        }


        private void InitCommandQueue()
        {
            _commandQueue = new List<ICommand>();
            _commandQueueUndoIndex = -1;
            RaiseEvent_CommandQueueChanged();
        }


        // Define the event handlers. 
        private void OnFileInProjectDirChanged(object source, FileSystemEventArgs e)
        {
                        // Specify what is done when a file is changed, created, or deleted.

            if (this.Project != null)
            {
                DoUpdateAllAudioDataBasedOnFileHeaders(ProjectDir, this.Project);
                RaiseEvent_FileInProjectDirChanged(this, e);
            }
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        private void OnFileInProjectDirRenamed(object source, RenamedEventArgs e)
        {

            // TODO: scan for related Segments and add a requester to update the AudioData filename?
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }

        internal EditorPreferences DeserializeXmlPreferences(string filename)
        {
            EditorPreferences preferences = null;
            try
            {
                TextReader reader = new StreamReader(filename);
                XmlSerializer serializer = new XmlSerializer(typeof(EditorPreferences));

                preferences = (EditorPreferences)serializer.Deserialize(reader);
                reader.Close();                
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
                RaiseEvent_OperationFailed("Failed to load Preferences", ex);
            }
            return preferences;
        }

        internal string DirectXSdkPath
        {
            get
            {
                return _directxSdkPath;
            }

            set
            {
                this._directxSdkPath = value;
            }
        }


        internal ProjectProperties ProjectProperties
        {
            get { return _project.Properties; }
            set
            {
                _project.Properties = value;
            }
        }

        public bool ProjectDataChangedSinceLastSave
        {
            get { return _projectDataChangedSinceLastSave;}
            private set { _projectDataChangedSinceLastSave = value;}
        }

        public bool ProjectDataChangedSinceLastAudit
        {
            get {return _projectDataChangedSinceLastAudit;}
            private set {_projectDataChangedSinceLastAudit = value;}
        }

        /// <summary>
        /// Execute this whenever the Project data has changed.
        /// </summary>
        public void ProjectDataChanged()
        {
            ProjectDataChangedSinceLastSave = true;
            ProjectDataChangedSinceLastAudit = true;
            RaiseEvent_ProjectDataChanged();
        }


        private EditorModel()
        {
            _project = new PsaiProject();
            Init("");
        }


        private string GetPathOfPreferencesFileAndCreateSubdirIfItDoesNotExist()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "psaiEditor");

            if (!System.IO.Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string path = Path.Combine(folder, "psaiEditor.config");
            return path;
        }

        internal void Init(string pathToProjectFile)
        {
            // psaiEditorLE sets the Project to null in OnEnable()
            if (_project == null)
            {
                _project = new PsaiProject();
            }
            _project.Init();
            ProjectDataChangedSinceLastSave = false;

            if (DirectXSdkPath.Length == 0)
            {
                string sdkPath = System.Environment.GetEnvironmentVariable("DXSDK_DIR");

                if (sdkPath != null)
                {
                    DirectXSdkPath = RemoveInvalidCharsFromPath(sdkPath);
                }
            }

            FullProjectPath = pathToProjectFile;

            InitCommandQueue();

#if (PSAI_EDITOR_STANDALONE)
            _bgWorkerProjectAudit = new BackgroundWorker();
            _bgWorkerProjectAudit.DoWork += new DoWorkEventHandler(bgWorkerProjectAudit_DoWork);
            _bgWorkerProjectAudit.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerProjectAudit_RunWorkerCompleted);
            _bgWorkerProjectAudit.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);
            _bgWorkerProjectAudit.WorkerReportsProgress = true;
            _bgWorkerProjectAudit.WorkerSupportsCancellation = true;

            _bgWorkerExportToUnity = new BackgroundWorker();
            _bgWorkerExportToUnity.DoWork += new DoWorkEventHandler(bgWorkerBuildForUnity_DoWork);
            _bgWorkerExportToUnity.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerExportToUnity_RunWorkerCompleted);
            _bgWorkerExportToUnity.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);
            _bgWorkerExportToUnity.WorkerReportsProgress = true;
            _bgWorkerExportToUnity.WorkerSupportsCancellation = true;


            _bgWorkerImportThemes = new BackgroundWorker();
            _bgWorkerImportThemes.DoWork += new DoWorkEventHandler(bgWorkerCopyAudioFilesAfterImport_DoWork);
            _bgWorkerImportThemes.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerCopyFilesAfterThemeImport_RunWorkerCompleted);
            _bgWorkerImportThemes.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);
            _bgWorkerImportThemes.WorkerReportsProgress = true;
            _bgWorkerImportThemes.WorkerSupportsCancellation = false;
#endif

        }

        public void NewProject(string path)
        {
            Init(path);
            RaiseEvent_NewProjectInitialized();
        }

        private string _pathOfProjectFile = null;
        internal string FullProjectPath
        {
            get
            {
                return _pathOfProjectFile;
            }

            set
            {
                _pathOfProjectFile = value;


                #if (PSAI_EDITOR_STANDALONE)
                if (value != null && value.Length > 0)
                {
                    try                    
                    {                        
                        // you need to set the path before you set anything else. Otherwise you get a
                        // SystemArgumentException. (Path is not of a legal form)
                        //_fileSystemWatcher = new FileSystemWatcher();           // TODO: disable the last one
                        _fileSystemWatcher.Path = Path.GetDirectoryName(value);
                        _fileSystemWatcher.IncludeSubdirectories = true;
                        /* Watch for changes in LastAccess and LastWrite times, and
                           the renaming of files or directories. */
                        // _fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                        _fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security;

                        _fileSystemWatcher.Filter = "*.wav";
                        _fileSystemWatcher.Changed += new FileSystemEventHandler(OnFileInProjectDirChanged);
                        //_fileSystemWatcher.Deleted += new FileSystemEventHandler(OnChanged);
                        _fileSystemWatcher.Renamed += new RenamedEventHandler(OnFileInProjectDirRenamed);
                        _fileSystemWatcher.EnableRaisingEvents = true;
                    }
                    catch (System.Exception ex)
                    {
                        Console.Write(ex);
                    	RaiseEvent_NewMessageToGui(string.Format("Warning! The psai Editor will not be able to watch changes on the Project path {0}. If you edit or replace your audio files, you'll need to click the 'Refresh' button in the AudioData rider in the Segment Properties Panel.", value));
                    }
                    
                }
                #endif
                
                //Project.Properties.Name = Path.GetFileNameWithoutExtension(_pathOfProjectFile);
            }
        }


        internal static string RemoveInvalidCharsFromPath(string argPath)
        {
            string returnString = string.Copy(argPath);
            char[] invalidChars = Path.GetInvalidPathChars();
            int invalidIndex = -1;
            do
            {
                invalidIndex = returnString.IndexOfAny(invalidChars);
                if (invalidIndex != -1)
                {
                    returnString = returnString.Remove(invalidIndex, 1);
                }
            }
            while (invalidIndex != -1);

            return returnString;
        }

        internal AudioData CreateAudioData(string argFilename)
        {
            string path = argFilename;
            if (!File.Exists(path))
            {
                path = System.IO.Path.Combine(FullProjectPath, argFilename);
                if (!File.Exists(path))
                {
                    return null;
                }
            }

            AudioData audioData = new AudioData();

            audioData.FilePathRelativeToProjectDir = EditorModel.Instance.GetPathRelativeToProjectFileBasedOnAbsolutePath(argFilename);
            string errorMessage = "";
            bool success = audioData.DoUpdateMembersBasedOnWaveHeader(path, out errorMessage);
            if (!success)
            {
                RaiseEvent_NewMessageToGui(errorMessage);                
            }
            else
            {
                audioData.Bpm = EditorModel.Instance.Project.Properties.DefaultBpm;
                audioData.PreBeats = EditorModel.Instance.Project.Properties.DefaultPrebeats;
                audioData.PostBeats = EditorModel.Instance.Project.Properties.DefaultPostbeats;
                audioData.PreBeatLengthInSamples = EditorModel.Instance.Project.Properties.DefaultPrebeatLengthInSamples;
                audioData.PostBeatLengthInSamples = EditorModel.Instance.Project.Properties.DefaultPostbeatLengthInSamples;
            }

            return audioData;
        }


        /// <summary>
        /// Returns the normalized filepath to the audio file using the current Systems directory separator.
        /// </summary>
        /// <param name="audioData"></param>
        /// <returns></returns>
        internal string GetFullPathOfAudioFile(string projectDir, AudioData audioData)
        {
            if (projectDir != null && audioData != null && audioData.FilePathRelativeToProjectDir != null)
            {
                string path = System.IO.Path.Combine(projectDir, audioData.FilePathRelativeToProjectDirForCurrentSystem);
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }


        public void DoUpdateAllAudioDataBasedOnFileHeaders(string projectDir, PsaiProject project)
        {
            if (project != null)
            {
                HashSet<Segment> segments = project.GetSegmentsOfAllThemes();

                string errorMessage;
                foreach (Segment segment in segments)
                {
                    //if (segment.AudioData.ReadValuesFromFileHeader)
                    {
                        string fullPathToAudioFile = Path.Combine(projectDir, segment.AudioData.FilePathRelativeToProjectDirForCurrentSystem);
                        bool success = segment.AudioData.DoUpdateMembersBasedOnWaveHeader(fullPathToAudioFile, out errorMessage);
                        if (!success)
                        {
                            errorMessage += "Hit the 'Refresh' button in the Segment Properties Panel to retry reading out the audio file header information.";
                            RaiseEvent_NewMessageToGui(errorMessage);
                        }
                    }
                }
            }
        }



        internal void ChangeAllFilenamesFromOggToWav()
        {
            HashSet<Segment> allSnippets = this.Project.GetSegmentsOfAllThemes();

            int replaceCount = 0;
            foreach (Segment snippet in allSnippets)
            {
                AudioData audioData = snippet.AudioData;
                string filename = audioData.FilePathRelativeToProjectDir;
                if (filename.EndsWith(".ogg"))
                {
                    filename = filename.Replace(".ogg", ".wav");
                    replaceCount++;
                }
                audioData.FilePathRelativeToProjectDir = filename;
            }

            #if DEBUG
                Console.WriteLine("changed " + replaceCount + " filenames from .ogg to .wav . ");
            #endif

        }


        public bool SaveProject(string path)
        {
            try
            {
                _project.SaveAsXmlFile(path);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                RaiseEvent_NewMessageToGui("ERROR! could not save project file. " + ex.ToString());
            }

            ProjectDataChangedSinceLastSave = false;
            RaiseEvent_ProjectSaved();

            return true;
        }

        internal void SavePreferences()
        {
            try
            {
                string path = GetPathOfPreferencesFileAndCreateSubdirIfItDoesNotExist();
                TextWriter writer = new StreamWriter(path);
                XmlSerializer serializer = new XmlSerializer(typeof(EditorPreferences));

                serializer.Serialize(writer, Preferences);
                writer.Close();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.InnerException);
            }
        }

        public void LoadProjectData(string path)
        {

#if (PSAI_EDITOR_STANDALONE)
            _eventArgsOfAuditOrExportRun = null;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(path));
#endif
            XmlProjectLoad(path);

        }


        // pass the audioLayer string as #defined in PSAI_AUDIOPLAYBACKLAYER
        public void BuildPsaiCoreBinaryFile(string targetFilepath, string audioLayer, BackgroundWorker worker, int percentageOfTask, int percentageDone)
        {
            PsaiProject project = this.Project;

            PsaiBinarySoundtrack psaiSoundtrack = new PsaiBinarySoundtrack();

            psaiSoundtrack.AudioFormat = audioLayer;

            foreach (Theme theme in project.Themes)
                psaiSoundtrack.Themes.Add(theme);

            HashSet<Segment> segmentSet = project.GetSegmentsOfAllThemes();

            float fPercentageDone = percentageDone;
            float percentageOfSingleSubtask = (float)percentageOfTask / segmentSet.Count;


            //DoUpdateAllAudioDataBasedOnFileHeaders(project, ProjectDir);

            foreach (Segment segment in segmentSet)
            {
                if (worker != null)
                {
                    fPercentageDone += percentageOfSingleSubtask;
                    worker.ReportProgress((int)fPercentageDone);
                }

                string pathToAudioData = EditorModel.Instance.GetFullPathOfAudioFile(ProjectDir, segment.AudioData);

                AudioData audioData = segment.AudioData;
                string errorMessage = "";

                bool result = audioData.DoUpdateMembersBasedOnWaveHeader(pathToAudioData, out errorMessage);  // TODO: what happens if file was not found ?

                if (!result)
                {
                    EditorModel.Instance.RaiseEvent_NewMessageToGui("warning: could not update members based on wave header of file: " + pathToAudioData);
                }

                segment.BuildCompatibleSegmentsSet(project);
                psaiSoundtrack.Segments.Add(segment);
            }

            ProtoBuf_PsaiCoreSoundtrack pbSoundtrack = psaiSoundtrack.CreateProtoBuf(project);

            using (var file = File.Create(targetFilepath))
            {
                ProtoBuf.Serializer.Serialize(file, pbSoundtrack);
            }
        }

        /// <summary>
        /// Takes an absolute filepath and searches backwards until the directory of the PsaiProject is found. The result will contain Path.DirectorySeparatorChars instead of '/'.
        /// </summary>
        /// <param name="fullpath"></param>
        /// <returns></returns>
 
        internal string GetPathRelativeToProjectFileBasedOnAbsolutePath(string fullpath)
        {            
            string relativePath = "";
            Uri file = new Uri(fullpath);

            // Must end in a slash to indicate folder
            Uri folder = new Uri(ProjectDir + Path.DirectorySeparatorChar);

            try
            {
                Uri relativeUri = folder.MakeRelativeUri(file);
                relativePath = Uri.UnescapeDataString(relativeUri.ToString());
                relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return relativePath;
        }


        // returns the first free snippet id starting from the id of the
        // last snippet of the snippet's parent group.
        internal int GetNextFreeSnippetIdBasedOnGroup(Group group)
        {
            int idToStartSearchFrom = 1;
            Group parentGroup = group;
            if (parentGroup != null && parentGroup.Segments.Count > 0)
            {
                idToStartSearchFrom = parentGroup.Segments.ElementAt(parentGroup.Segments.Count - 1).Id;
            }
            return EditorModel.Instance.Project.GetNextFreeSnippetId(idToStartSearchFrom);
        }

        // checks each theme if there is a possible transition to any other theme at any time.
        // returns true if all themes are free of deadlocks, false otherwise
        internal bool DoAuditTransitionForAllThemes(psai.net.Soundtrack soundtrack, out Dictionary<int, Dictionary<int, int>> mapSegmentIdToTargetThemeIdsAndSteps)
        {            
            bool atLeastOneDeadlockExists = false;

            mapSegmentIdToTargetThemeIdsAndSteps = new Dictionary<int, Dictionary<int, int>>();

            foreach (psai.net.Theme sourceTheme in soundtrack.m_themes.Values)
            {
                foreach (psai.net.Theme targetTheme in soundtrack.m_themes.Values)
                {
                    Dictionary<int, int> mapSegmentIdsToSteps = new Dictionary<int, int>();
                    bool themeTransitionIsPossible = CheckIfThemeTransitionIsAlwaysPossible(soundtrack, sourceTheme, targetTheme, out mapSegmentIdsToSteps);

                    if (!themeTransitionIsPossible)
                    {
                        atLeastOneDeadlockExists = true;
                    }

                    foreach(int sourceSegmentId in mapSegmentIdsToSteps.Keys)
                    {
                        if (!mapSegmentIdToTargetThemeIdsAndSteps.ContainsKey(sourceSegmentId))
                        {
                            mapSegmentIdToTargetThemeIdsAndSteps[sourceSegmentId] = new Dictionary<int, int>();       // key: targetThemeId   value: steps
                        }
                        
                        mapSegmentIdToTargetThemeIdsAndSteps[sourceSegmentId][targetTheme.id] = mapSegmentIdsToSteps[sourceSegmentId];
                    }                    
                }
            }            
            return !atLeastOneDeadlockExists;
        }


        internal bool CheckIfThemeTransitionIsAlwaysPossible(psai.net.Soundtrack soundtrack, psai.net.Theme sourceTheme, psai.net.Theme targetTheme, out Dictionary<int, int> mapSegmentIdsToSteps)
        {
            mapSegmentIdsToSteps = new Dictionary<int, int>();      // value -1: deadlock ;  value 0: direct Transition is possible ; value x: number of indirection steps 
            bool atLeastOneDeadlockExists = false;

            if (sourceTheme.themeType == ThemeType.highlightLayer || targetTheme.themeType == ThemeType.highlightLayer)
            {
                return true;
            }
            else
            {
                ThemeInterruptionBehavior interruptionBehavior = psai.net.Theme.GetThemeInterruptionBehavior(sourceTheme.themeType, targetTheme.themeType);

                if (psai.net.Theme.ThemeInterruptionBehaviorRequiresEvaluationOfSegmentCompatibilities(interruptionBehavior))
                {
                    foreach (psai.net.Segment sourceSegment in sourceTheme.m_segments)
                    {

                        if (sourceSegment.CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(soundtrack, targetTheme.id))
                        {
                            mapSegmentIdsToSteps[sourceSegment.Id] = 0;
                        }
                        else if (sourceSegment.MapOfNextTransitionSegmentToTheme.ContainsKey(targetTheme.id))
                        {
                            int indirectionSteps = 0;
                            psai.net.Segment segmentToCheck = sourceSegment;
                            psai.net.Segment nextSegment = null;
                            do
                            {
                                nextSegment = segmentToCheck.MapOfNextTransitionSegmentToTheme[targetTheme.id];
                                segmentToCheck = nextSegment;
                                indirectionSteps++;
                            }
                            while (sourceSegment.MapOfNextTransitionSegmentToTheme.ContainsKey(targetTheme.id) && !nextSegment.CheckIfAtLeastOneDirectTransitionOrLayeringIsPossible(soundtrack, targetTheme.id));

                            mapSegmentIdsToSteps[sourceSegment.Id] = indirectionSteps;
                        }
                        else
                        {
                            mapSegmentIdsToSteps[sourceSegment.Id] = -1;
                            atLeastOneDeadlockExists = true;
                        }
                    }
                }

                return !atLeastOneDeadlockExists;
            }
        }


        public static bool CheckIfFileOfTheSameNameExistsWithAnotherExtension(string fullPath, string otherExtension)
        {
            string otherFileName = Path.GetFileNameWithoutExtension(fullPath) + "." + otherExtension;
            string otherFilePath = Path.Combine(Path.GetDirectoryName(fullPath), otherFileName);

            return (File.Exists(otherFilePath));
        }
        
        public static bool CheckIfPathIsWithinSubdirOfAssetsResources(string fullPath)
        {
            string[] folders = fullPath.Split(new char[] { '/', '\\' });

            for (int i = 0; i < folders.Length; i++)
            {
                string folder = folders[i];
                if (folder.Equals("Assets"))
                {
                    if (i < folders.Length - 1 && folders[i + 1].Equals("Resources"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }



#if (PSAI_EDITOR_STANDALONE)
        public void CopyAudioFilesOfImportedThemes(List<Theme> importedThemes, string sourceDir, string targetDir)
        {
            if (!_bgWorkerImportThemes.IsBusy)
            {
                ArgsCopyFilesToTargetDir args = new ArgsCopyFilesToTargetDir();
                args.targetDirectory = targetDir;
                args.sourceThemes = importedThemes;
                args.sourceDirectory = sourceDir;

                EventHandler<EventArgs_ProgressBarStart> handler = EventProgressBarStart;
                if (handler != null)
                {
                    handler(this, new EventArgs_ProgressBarStart("Copying Audio Files..."));
                }

                _bgWorkerImportThemes.RunWorkerAsync(args);
            }
            else
            {
                RaiseEvent_NewMessageToGui("The files are already being copied apparently..!");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="themesToImport"></param>
        /// <param name="sourceDir">the full path of the base directory from where the relative paths of the AudioData will lead to their files</param>
        /// <param name="targetDir"></param>
        /// <param name="atLeastOneCopyFailed"></param>
        /// <param name="atLeastOneFileDidNotExistInTargetFolder"></param>
        /// <param name="canceled"></param>
        /// <param name="worker"></param>
        /// <param name="percentageOfTask"></param>
        /// <param name="percentageDone"></param>
        /// <returns></returns>
        private static int CopyAudioFilesOfThemesToTargetDirectoryIfOlderOrNotExisting(List<Theme> themesToImport, string sourceDir, string targetDir, out bool atLeastOneCopyFailed, out bool atLeastOneFileDidNotExistInTargetFolder, out bool canceled, BackgroundWorker worker, int percentageOfTask, int percentageDone)
        {
            List<Segment> sourceSegments = new List<Segment>();

            foreach (Theme theme in themesToImport)
            {
                HashSet<Segment> segmentsOfTheme = theme.GetSegmentsOfAllGroups();
                sourceSegments.AddRange(segmentsOfTheme);
            }
            return CopyAudioFilesToTargetDirectoryIfOlderOrNotExisting(sourceSegments, sourceDir, targetDir, out atLeastOneCopyFailed, out atLeastOneFileDidNotExistInTargetFolder, out canceled, worker, percentageOfTask, percentageDone);
        }

    
        private static int CopyAudioFilesOfProjectToTargetDirectoryIfOlderOrNotExisting(PsaiProject project, string sourceDir, string targetDir, out bool atLeastOneCopyFailed, out bool atLeastOneFileDidNotExistInTargetFolder, out bool canceled, BackgroundWorker worker, int percentageOfTask, int percentageDone)
        {
            HashSet<Segment> allSegments = project.GetSegmentsOfAllThemes();
            List<Segment> sourceSegments = allSegments.ToList<Segment>();

            return CopyAudioFilesToTargetDirectoryIfOlderOrNotExisting(sourceSegments, sourceDir, targetDir, out atLeastOneCopyFailed, out atLeastOneFileDidNotExistInTargetFolder, out canceled, worker, percentageOfTask, percentageDone);
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="targetDir"></param>
        /// <param name="atLeastOneCopyFailed"></param>
        /// <param name="atLeastOneFileDidNotExistInTargetFolder"></param>
        /// <param name="worker"></param>
        /// <param name="percentageOfTask"></param>
        /// <param name="percentageDone"></param>
        /// <returns>filesCopiedOrUpdated</returns>
        internal static int CopyAudioFilesToTargetDirectoryIfOlderOrNotExisting(List<Segment> sourceSegments, string sourceDir, string targetDir, out bool atLeastOneCopyFailed, out bool atLeastOneFileDidNotExistInTargetFolder, out bool canceled, BackgroundWorker worker, int percentageOfTask, int percentageDone)
        {           
            int filesCopiedOrUpdated = 0;
            atLeastOneFileDidNotExistInTargetFolder = false;
            atLeastOneCopyFailed = false;
            canceled = false;

            float fPercentageDone = percentageDone;
            float percentageOfSingleCopyAction = (float)percentageOfTask / sourceSegments.Count;
            bool permissionGrantedToOverwriteAllOlderWavFilesInTargetFolder = false;
            bool permissionGrantedToOverwriteAllNewerWavFilesInTargetFolder = false;

            foreach (Segment segment in sourceSegments)
            {
                if (worker != null)
                {
                    fPercentageDone += percentageOfSingleCopyAction;
                    worker.ReportProgress((int)fPercentageDone);
                }

                string fullSourcePath = Path.Combine(sourceDir, segment.AudioData.FilePathRelativeToProjectDirForCurrentSystem);
                string fullTargetPath = Path.Combine(targetDir, segment.AudioData.FilePathRelativeToProjectDirForCurrentSystem);
                bool sourceFileExists = System.IO.File.Exists(fullSourcePath);
                bool targetFileExists = System.IO.File.Exists(fullTargetPath);
                bool doCopyAudioFile = false;

                System.DateTime lastWriteTimeOfSourceAudioFile = System.DateTime.MinValue;               
                System.DateTime lastWriteTimeOfTargetAudioFile = System.DateTime.MinValue;

                if (sourceFileExists == false)
                {
                    EditorModel.Instance.RaiseEvent_NewMessageToGui("ERROR! source audio data file was not found", MessageType.error);
                    atLeastOneCopyFailed = true;
                    return filesCopiedOrUpdated;
                }
                else
                {
                    lastWriteTimeOfSourceAudioFile = System.IO.File.GetLastWriteTime(fullSourcePath);

                    string normalizedSourcePath = fullSourcePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                    string normalizedTargetPath = fullTargetPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                    if (normalizedSourcePath.Equals(normalizedTargetPath) && sourceFileExists)
                    {
                        // source and target file are the same
                        #if DEBUG
                            Console.WriteLine("skipping " + fullSourcePath + ", target path is the same.");                        
                        #endif
                    }
                    else
                    {
                        if (targetFileExists == false)
                        {
                            doCopyAudioFile = true;
                            atLeastOneFileDidNotExistInTargetFolder = true;
                        }
                        else
                        {                        
                            lastWriteTimeOfTargetAudioFile = System.IO.File.GetLastWriteTime(fullTargetPath);

                            #if DEBUG
                                Console.WriteLine("file " + fullTargetPath + " exists. lastWriteTimeOfSourceAudioFile=" + lastWriteTimeOfSourceAudioFile + "  lastWriteTimeOfTargetAudioFile=" + lastWriteTimeOfTargetAudioFile);
                            #endif

                            if (lastWriteTimeOfSourceAudioFile < lastWriteTimeOfTargetAudioFile)
                            {
                                if (!permissionGrantedToOverwriteAllNewerWavFilesInTargetFolder)
                                {
                                    if (MessageBox.Show("A newer file '" + segment.AudioData.FilePathRelativeToProjectDir + "' already exists in the target folder. Do you wish to replace ALL newer audio files in the target directory? ", "Replace all newer wav files in Unity Project's soundtrack folder? ", MessageBoxButtons.OKCancel) == DialogResult.OK)
                                    {
                                        permissionGrantedToOverwriteAllNewerWavFilesInTargetFolder = true;
                                    }
                                    else
                                    {
                                        EditorModel.Instance.RaiseEvent_NewMessageToGui("Canceled by user.");
                                        atLeastOneCopyFailed = true;
                                        canceled = true;
                                        return filesCopiedOrUpdated;
                                    }
                                }

                                if (permissionGrantedToOverwriteAllNewerWavFilesInTargetFolder)
                                {
                                    doCopyAudioFile = true;
                                }
                            }


                            if (lastWriteTimeOfTargetAudioFile < lastWriteTimeOfSourceAudioFile)
                            {
                                if (!permissionGrantedToOverwriteAllOlderWavFilesInTargetFolder)
                                {                                    
                                    if (MessageBox.Show("An older file named '" + segment.AudioData.FilePathRelativeToProjectDir + "' already exists in the target folder. Do you wish to overwrite ALL older audio files in the target directory? ", "Replace all older files in Unity Project's folder? ", MessageBoxButtons.OKCancel) == DialogResult.OK)
                                    {
                                        permissionGrantedToOverwriteAllOlderWavFilesInTargetFolder = true;
                                    }
                                    else
                                    {
                                        EditorModel.Instance.RaiseEvent_NewMessageToGui("Canceled by user.");
                                        atLeastOneCopyFailed = true;
                                        canceled = true;
                                        return filesCopiedOrUpdated;
                                    }
                                }

                                if (permissionGrantedToOverwriteAllOlderWavFilesInTargetFolder)
                                {
                                    doCopyAudioFile = true;
                                }
                            }
                        }
                    }


                    if (targetFileExists) 
                    {
                        // check if the .wav.meta file also exists
                        string fullMetaFilePath = fullTargetPath + ".meta";
                        
                        if (System.IO.File.Exists(fullMetaFilePath) == false)
                        {
                            atLeastOneFileDidNotExistInTargetFolder = true;
                        }
                    }

                    if (doCopyAudioFile)
                    {
                        #if DEBUG
                            Console.WriteLine("copying source file '" + fullSourcePath + "' to '" + fullTargetPath);
                        #endif

                        try
                        {
                            System.IO.FileInfo file = new System.IO.FileInfo(fullTargetPath);
                            file.Directory.Create(); // If the directory already exists, this method does nothing.
                            System.IO.File.Copy(fullSourcePath, fullTargetPath, true);
                            System.IO.File.SetLastWriteTime(fullTargetPath, lastWriteTimeOfSourceAudioFile);  // if we don't override the last write time, the time of the copy action will be used
                            targetFileExists = true;
                            filesCopiedOrUpdated++;
                        }
                        catch (System.Exception ex)
                        {
                            EditorModel.Instance.RaiseEvent_NewMessageToGui("ERROR! Failed to copy file '" + fullSourcePath + "' to path '" + fullTargetPath + "'. Exception= " + ex.ToString(), MessageType.error);
                            atLeastOneCopyFailed = true;
                            return filesCopiedOrUpdated;
                        }
                    }
                }
            }

            return filesCopiedOrUpdated;
        }


        internal void DoAudit()
        {
            if (_eventArgsOfAuditOrExportRun == null)
            {
                _eventArgsOfAuditOrExportRun = new EventArgs_AuditOrExportResult();
            }

            _eventArgsOfAuditOrExportRun.Canceled = false;
            _eventArgsOfAuditOrExportRun.ErrorOccurred = false;
            _eventArgsOfAuditOrExportRun.WarningOccurred = false;

            if (Project.Themes == null || Project.Themes.Count == 0)
            {
                RaiseEvent_NewMessageToGui("No audit is needed, the Project is empty.");
            }
            else
            {
                EventHandler<EventArgs_ProgressBarStart> handler = EventProgressBarStart;
                if (handler != null)
                {
                    handler(this, new EventArgs_ProgressBarStart("Project Audit"));
                }

                if (!_bgWorkerProjectAudit.IsBusy)
                {
                    _bgWorkerProjectAudit.RunWorkerAsync(_eventArgsOfAuditOrExportRun);
                }
            }
        }

        internal void ProjectAuditCancel()
        {
            if (_bgWorkerProjectAudit.IsBusy && _bgWorkerProjectAudit.WorkerSupportsCancellation)
            {
                _bgWorkerProjectAudit.CancelAsync();
            }
        }

        private void bgWorkerCopyAudioFilesAfterImport_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            int percentageDone = 5;

            RaiseEvent_NewMessageToGui("Copying audio files...");

            ArgsCopyFilesToTargetDir args = (ArgsCopyFilesToTargetDir)e.Argument;

            List<Segment> segments = new List<Segment>();
            foreach (Theme theme in args.sourceThemes)
            {
                segments.AddRange(theme.GetSegmentsOfAllGroups());
            }

            bool atLeastOneFileDidNotExistInTargetFolder = false;
            bool atLeastOneCopyFailed = false;
            bool exportCanceled = false;
            int numberOfFilesCopiedOrUpdated = EditorModel.CopyAudioFilesToTargetDirectoryIfOlderOrNotExisting(segments, args.sourceDirectory, args.targetDirectory, out atLeastOneCopyFailed, out atLeastOneFileDidNotExistInTargetFolder, out exportCanceled, worker, 95, percentageDone);
            RaiseEvent_NewMessageToGui(string.Format("...copied {0} audio files.", numberOfFilesCopiedOrUpdated));
        }

        private void bgWorkerCopyFilesAfterThemeImport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EventHandler handler = EventImportCompleted;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void bgWorkerBuildForUnity_DoWork(object sender, DoWorkEventArgs e)
        {
            //bgWorderExportToUnityDoWork_protobuf(sender, e);
            bgWorderExportToUnityDoWork_xml(sender, e);
        }

        private void bgWorderExportToUnityDoWork_xml(Object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            int percentageDone = 5;
            int percentageBuildSoundtrackFile = 20;
            int percentageCopyAllAudioFiles = 50;

            //RaiseEvent_NewMessageToGui("Writing xml file...");

            worker.ReportProgress(percentageDone);

            string xmlFilename = Path.Combine(Path.GetDirectoryName(_eventArgsOfAuditOrExportRun.PsaiCoreBinaryFilename), Path.GetFileNameWithoutExtension(_eventArgsOfAuditOrExportRun.PsaiCoreBinaryFilename));
            xmlFilename += ".xml";

            try
            {
                _project.SaveAsXmlFile(xmlFilename);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                RaiseEvent_NewMessageToGui("ERROR! could not save soundtrack file. " + ex.ToString());
                _eventArgsOfAuditOrExportRun.ErrorOccurred = true;
            }


            RaiseEvent_NewMessageToGui("saved xml file.");

            percentageDone += percentageBuildSoundtrackFile;
            worker.ReportProgress(percentageDone);

            string psaiSoundtrackDir = System.IO.Path.GetDirectoryName(_eventArgsOfAuditOrExportRun.PsaiCoreBinaryFilename);
            string audioSubdir = psaiSoundtrackDir; //Path.Combine(psaiSoundtrackDir, "wav");

            if (EditorModel.Instance.Project.InitialExportDirectory != psaiSoundtrackDir)
            {
                EditorModel.Instance.Project.InitialExportDirectory = psaiSoundtrackDir;
                EditorModel.Instance.ProjectDataChangedSinceLastSave = true;
            }

            if (!Directory.Exists(audioSubdir))
            {
                RaiseEvent_NewMessageToGui("creating subdirectory in Resources folder...");

                try
                {
                    Directory.CreateDirectory(audioSubdir);
                }
                catch (System.Exception ex)
                {
                    RaiseEvent_NewMessageToGui(ex.ToString());
                    RaiseEvent_NewMessageToGui("ERROR! could not create the 'wav' subfolder within the target directory. Please make sure the current User has sufficient access rights.");
                    _eventArgsOfAuditOrExportRun.ErrorOccurred = true;
                }
                RaiseEvent_NewMessageToGui("...folder created.");
            }

            if (_eventArgsOfAuditOrExportRun.ErrorOccurred == false)
            {
                RaiseEvent_NewMessageToGui("copying/verifying audio files in Unity project's Resources folder...");
                bool atLeastOneFileDidNotExistInTargetFolder = false;
                bool atLeastOneCopyFailed = false;
                bool exportCanceled = false;
                int numberOfFilesCopiedOrUpdated = EditorModel.CopyAudioFilesOfProjectToTargetDirectoryIfOlderOrNotExisting(EditorModel.Instance.Project, EditorModel.Instance.ProjectDir, psaiSoundtrackDir, out atLeastOneCopyFailed, out atLeastOneFileDidNotExistInTargetFolder, out exportCanceled, worker, percentageCopyAllAudioFiles, percentageDone);
                if (!atLeastOneCopyFailed && !exportCanceled)
                {
                    percentageDone += percentageCopyAllAudioFiles;
                    worker.ReportProgress(percentageDone);

                    _eventArgsOfAuditOrExportRun.NumberOfAudioFilesCopiedToUnityProject = numberOfFilesCopiedOrUpdated;

                    RaiseEvent_NewMessageToGui("...copied or updated " + numberOfFilesCopiedOrUpdated + " audio files.");
                }
                else
                {
                    if (exportCanceled)
                    {
                        _eventArgsOfAuditOrExportRun.Canceled = true;
                    }
                    _eventArgsOfAuditOrExportRun.ErrorOccurred = true;
                }
            }

            e.Result = _eventArgsOfAuditOrExportRun;      // pass the result to the _Completed Handler

        }


        private void bgWorderExportToUnityDoWork_protobuf(Object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            int percentageDone = 5;
            int percentageBuildPsaiCoreBinaryFile = 20;
            int percentageCopyAllAudioFiles = 50;

            RaiseEvent_NewMessageToGui("Writing binary file...");

            worker.ReportProgress(percentageDone);

            EditorModel.Instance.BuildPsaiCoreBinaryFile(_eventArgsOfAuditOrExportRun.PsaiCoreBinaryFilename, "OGG", worker, percentageBuildPsaiCoreBinaryFile, percentageDone);
            RaiseEvent_NewMessageToGui("...done writing binary file...");

            percentageDone += percentageBuildPsaiCoreBinaryFile;
            worker.ReportProgress(percentageDone);

            string psaiSoundtrackDir = System.IO.Path.GetDirectoryName(_eventArgsOfAuditOrExportRun.PsaiCoreBinaryFilename);
            string audioSubdir = psaiSoundtrackDir; //Path.Combine(psaiSoundtrackDir, "wav");

            if (EditorModel.Instance.Project.InitialExportDirectory != psaiSoundtrackDir)
            {
                EditorModel.Instance.Project.InitialExportDirectory = psaiSoundtrackDir;
                EditorModel.Instance.ProjectDataChangedSinceLastSave = true;
            }

            if (!Directory.Exists(audioSubdir))
            {
                RaiseEvent_NewMessageToGui("creating subdirectory in Resources folder...");

                try
                {
                    Directory.CreateDirectory(audioSubdir);
                }
                catch (System.Exception ex)
                {
                    RaiseEvent_NewMessageToGui(ex.ToString());
                    RaiseEvent_NewMessageToGui("ERROR! could not create the 'wav' subfolder within the target directory. Please make sure the current User has sufficient access rights.");
                    _eventArgsOfAuditOrExportRun.ErrorOccurred = true;
                }
                RaiseEvent_NewMessageToGui("...folder created.");
            }

            if (_eventArgsOfAuditOrExportRun.ErrorOccurred == false)
            {
                RaiseEvent_NewMessageToGui("copying/verifying audio files in Unity project's Resources folder...");
                bool atLeastOneFileDidNotExistInTargetFolder = false;
                bool atLeastOneCopyFailed = false;
                bool exportCanceled = false;
                int numberOfFilesCopiedOrUpdated = EditorModel.CopyAudioFilesOfProjectToTargetDirectoryIfOlderOrNotExisting(EditorModel.Instance.Project, EditorModel.Instance.ProjectDir, psaiSoundtrackDir, out atLeastOneCopyFailed, out atLeastOneFileDidNotExistInTargetFolder, out exportCanceled, worker, percentageCopyAllAudioFiles, percentageDone);
                if (!atLeastOneCopyFailed && !exportCanceled)
                {
                    percentageDone += percentageCopyAllAudioFiles;
                    worker.ReportProgress(percentageDone);

                    _eventArgsOfAuditOrExportRun.NumberOfAudioFilesCopiedToUnityProject = numberOfFilesCopiedOrUpdated;

                    RaiseEvent_NewMessageToGui("...copied or updated " + numberOfFilesCopiedOrUpdated + " audio files.");
                }
                else
                {
                    if (exportCanceled)
                    {
                        _eventArgsOfAuditOrExportRun.Canceled = true;
                    }
                    _eventArgsOfAuditOrExportRun.ErrorOccurred = true;
                }
            }

            e.Result = _eventArgsOfAuditOrExportRun;      // pass the result to the _Completed Handler

        }

        private void bgWorkerProjectAudit_DoWork(object sender, DoWorkEventArgs e)
        {
            ProjectDataChangedSinceLastAudit = false;

            BackgroundWorker worker = sender as BackgroundWorker;

            bool atLeastOneFileIsMissing = false;

            //EventArgs_AuditOrExportResult result = e.Argument as EventArgs_AuditOrExportResult;

            RaiseEvent_NewMessageToGui("\n-------------------------------------------");
            RaiseEvent_NewMessageToGui("Starting Project Audit...");

            // check files
            RaiseEvent_NewMessageToGui("Checking audio files, please wait...");

            int percentageOfFileTest = 20;
            int percentageDone = 0;

            HashSet<Segment> allSegments = EditorModel.Instance.Project.GetSegmentsOfAllThemes();

            if (allSegments == null || allSegments.Count == 0)
            {
                // TODO: empty project, return immediately
            }
            else
            {
                float percentageOfIndividualFile = percentageOfFileTest / allSegments.Count;

                foreach (Segment segment in allSegments)
                {
                    string fullPath = Path.Combine(ProjectDir, segment.AudioData.FilePathRelativeToProjectDirForCurrentSystem);
                    if (!File.Exists(fullPath))
                    {
                        worker.ReportProgress((int)(percentageDone + percentageOfIndividualFile));

                        string errorString = "File not found !  '" + segment.AudioData.FilePathRelativeToProjectDir + "'  in Theme '" + segment.Group.Theme.Name + "' , Group '" + segment.Group.Name + "' , Segment '" + segment.Name + "'";
                        RaiseEvent_NewMessageToGui(errorString, MessageType.error);

                        _eventArgsOfAuditOrExportRun.entitiesThatCausedErrorInLastAudit[segment] = errorString;

                        atLeastOneFileIsMissing = true;
                        _eventArgsOfAuditOrExportRun.ErrorOccurred = true;
                    }
                    else
                    {
                        string pathToAudioData = EditorModel.Instance.GetFullPathOfAudioFile(EditorModel.Instance.ProjectDir, segment.AudioData);
                        string errorMessage = "";
                        AudioData audioData = segment.AudioData;
                        bool result = audioData.DoUpdateMembersBasedOnWaveHeader(pathToAudioData, out errorMessage);
                        if (!result)
                        {
                            RaiseEvent_NewMessageToGui(errorMessage);
                        }
                    }                    
                }

                if (!atLeastOneFileIsMissing)
                {
                    RaiseEvent_NewMessageToGui("...all files ok.");
                }

                percentageDone += percentageOfFileTest;
                worker.ReportProgress(percentageDone);

                // check for long Prebeat times
                RaiseEvent_NewMessageToGui("Checking the prebeat times of all Segments...");
                List<int> segmentsWithHighPrebeatTimes = null;
                bool auditPrebeatSuccess = EditorModel.Instance.DoAuditPrebeatTimes(out segmentsWithHighPrebeatTimes);
                if (!auditPrebeatSuccess)
                {
                    _eventArgsOfAuditOrExportRun.WarningOccurred = true;
                    foreach (int segmentId in segmentsWithHighPrebeatTimes)
                    {
                        Segment segment = EditorModel.Instance.Project.GetSnippetById(segmentId);
                        int prebeatInMs = segment.AudioData.GetMillisecondsFromSampleCount(segment.AudioData.PreBeatLengthInSamples);
                        string warningString = "WARNING! Segment " + segment.Name + " has a high Prebeat time of " + prebeatInMs + " milliseconds.";
                        RaiseEvent_NewMessageToGui("  " + warningString, MessageType.warning);

                        _eventArgsOfAuditOrExportRun.entitiesThatCausedWarningInLastAudit[segment] = warningString;
                    }
                    RaiseEvent_NewMessageToGui("");
                    RaiseEvent_NewMessageToGui("High Prebeat times will negatively affect the responsiveness of your soundtrack. The higher the longest prebeat time throughout all compatible Segments, the earlier psai needs to start the evaluation of the next Segment to play at runtime. We therefore highly recommend to keep the Prebeat times of all Segments as short as possible, like below " + EditorModel.WARNING_THRESHOLD_PREBEAT_MILLISECONDS + " milliseconds.");
                }
                else
                {
                    RaiseEvent_NewMessageToGui("...all Prebeat times are ok.");
                }
                //RaiseEvent_NewMessageToGui("");

                percentageDone = 30;
                worker.ReportProgress(percentageDone);


                // check for missing Start- and Middle- End-Segments
                RaiseEvent_NewMessageToGui("Checking all Groups for missing Start-, Middle- and End-Segments...");

                Dictionary<PsaiMusicEntity, string> errorMapEssentionSegmentSuitabilityMissing = new Dictionary<PsaiMusicEntity, string>();
                bool essentialSegmentSuitabilityIsMissingForAtLeastOneGroup = DoAuditCheckAllGroupsForStartMiddleEnd(out errorMapEssentionSegmentSuitabilityMissing);

                if (!essentialSegmentSuitabilityIsMissingForAtLeastOneGroup)
                {
                    RaiseEvent_NewMessageToGui("...ok.");
                }
                else
                {
                    _eventArgsOfAuditOrExportRun.ErrorOccurred = true;
                    foreach (PsaiMusicEntity entity in errorMapEssentionSegmentSuitabilityMissing.Keys)
                    {
                        _eventArgsOfAuditOrExportRun.entitiesThatCausedErrorInLastAudit[entity] = errorMapEssentionSegmentSuitabilityMissing[entity];
                    }


                    foreach (string s in errorMapEssentionSegmentSuitabilityMissing.Values)
                    {
                        RaiseEvent_NewMessageToGui(s, MessageType.error);
                    }
                }

                Dictionary<PsaiMusicEntity, string> errorMapNoSuitabilitySet = new Dictionary<PsaiMusicEntity, string>();
                bool segmentWithNoSuitabilitiesFound = DoAuditCheckAllSegmentsForZeroSuitability(out errorMapNoSuitabilitySet);

                if (!segmentWithNoSuitabilitiesFound)
                {
                    RaiseEvent_NewMessageToGui("...ok.");
                }
                else
                {
                    _eventArgsOfAuditOrExportRun.WarningOccurred = true;
                    foreach (PsaiMusicEntity entity in errorMapNoSuitabilitySet.Keys)
                    {
                        _eventArgsOfAuditOrExportRun.entitiesThatCausedWarningInLastAudit[entity] = errorMapNoSuitabilitySet[entity];
                    }

                    foreach (string s in errorMapNoSuitabilitySet.Values)
                    {
                        RaiseEvent_NewMessageToGui(s);
                    }
                }

                percentageDone = 40;                           
                worker.ReportProgress(percentageDone);

                int percentageIndirectionToEndTest = 20;
                float percentageIndirToEndStep = percentageIndirectionToEndTest / Project.Themes.Count;

                psai.net.Soundtrack soundtrack = Project.BuildPsaiDotNetSoundtrackFromProject();
                //RaiseEvent_NewMessageToGui("");
                RaiseEvent_NewMessageToGui("Checking for redirections to End-Segments...");

                Dictionary<int, Dictionary<int, int>> mapThemeIdsToSegmentsIdsAndIndirectionSteps = null;
                bool indirectionToEndSuccess = EditorModel.Instance.DoAuditCheckIndirectionToEndForAllThemes(soundtrack, out mapThemeIdsToSegmentsIdsAndIndirectionSteps);
                if (!indirectionToEndSuccess)
                {
                    _eventArgsOfAuditOrExportRun.ErrorOccurred = true;
                }
                else
                {
                    RaiseEvent_NewMessageToGui("...ok.");
                }

                foreach (int themeId in mapThemeIdsToSegmentsIdsAndIndirectionSteps.Keys)
                {
                    Theme theme = EditorModel.Instance.Project.GetThemeById(themeId);
                    Dictionary<int, int> singleMap = mapThemeIdsToSegmentsIdsAndIndirectionSteps[themeId];

                    foreach (int segmentId in singleMap.Keys)
                    {
                        Segment segment = EditorModel.Instance.Project.GetSnippetById(segmentId);

                        if (singleMap[segmentId] == 0)
                        {
                            // TODO: add verbose mode to log this?
                            //WriteLineToLogWindow("   Segment " + segment.Name + " is tagged as suitable to end a Theme.");
                        }
                        else if (singleMap[segmentId] == -1)
                        {

                            string errorString = "ERROR! Segment " + segment.Name + " cannot reach an End-Segment.";
                            RaiseEvent_NewMessageToGui("  " + errorString, MessageType.error);

                            _eventArgsOfAuditOrExportRun.entitiesThatCausedErrorInLastAudit[segment] = errorString;
                        }
                        else if (singleMap[segmentId] == 1)
                        {
                            // TODO: add verbose mode to log this?
                            //WriteLineToLogWindow("   Segment " + segment.Name + " can reach an End-Segment directly.");
                        }
                        else if (singleMap[segmentId] >= EditorModel.WARNING_THRESHOLD_INDIRECTION_STEPS)
                        {
                            _eventArgsOfAuditOrExportRun.WarningOccurred = true;

                            string warningString = "WARNING! Segment " + segment.Name + " needs a sequence of " + singleMap[segmentId] + " Segments to reach an End-Segment.";
                            RaiseEvent_NewMessageToGui("  " + warningString, MessageType.warning);

                            _eventArgsOfAuditOrExportRun.entitiesThatCausedWarningInLastAudit[segment] = warningString;
                        }
                    }

                    worker.ReportProgress((int)(percentageDone + percentageIndirToEndStep));

                }

                percentageDone = 60;
                worker.ReportProgress(percentageDone);

                // Check for deadends
                //RaiseEvent_NewMessageToGui("");
                RaiseEvent_NewMessageToGui("Checking Theme/Group transitions for dead-ends...");
                Dictionary<int, Dictionary<int, int>> mapSegmentIdsToTargetThemeIdsAndSteps = null;
                bool transitionAuditSuccess = EditorModel.Instance.DoAuditTransitionForAllThemes(soundtrack, out mapSegmentIdsToTargetThemeIdsAndSteps);


                percentageDone = 70;
                worker.ReportProgress(percentageDone);

                if (!transitionAuditSuccess)
                {
                    _eventArgsOfAuditOrExportRun.ErrorOccurred = true;

                    foreach (int segmentId in mapSegmentIdsToTargetThemeIdsAndSteps.Keys)
                    {
                        foreach (psai.net.Theme targetTheme in soundtrack.m_themes.Values)
                        {
                            if (mapSegmentIdsToTargetThemeIdsAndSteps[segmentId].ContainsKey(targetTheme.id))
                            {
                                int steps = mapSegmentIdsToTargetThemeIdsAndSteps[segmentId][targetTheme.id];

                                if (steps == -1)
                                {
                                    Segment segment = EditorModel.Instance.Project.GetSnippetById(segmentId);
                                    string errorString = "ERROR! No valid sequence exists from Segment " + segment.Name + " to any Segment of Theme " + targetTheme.Name;
                                    RaiseEvent_NewMessageToGui("  " + errorString, MessageType.error);

                                    _eventArgsOfAuditOrExportRun.entitiesThatCausedErrorInLastAudit[segment] = errorString;
                                }
                                else if (steps == 0)
                                {
                                    //WriteLineToLogWindow("Segment " + segmentId + " can reach Theme " + targetTheme.Name + " directly."); 
                                }
                                else
                                {
                                    if (steps >= EditorModel.WARNING_THRESHOLD_INDIRECTION_STEPS)
                                    {
                                        _eventArgsOfAuditOrExportRun.WarningOccurred = true;
                                        Segment segment = EditorModel.Instance.Project.GetSnippetById(segmentId);
                                        string warningString = "WARNING! Segment " + segment.Name + " needs a rather long indirection sequence of " + steps + " Segments for a transition to Theme " + targetTheme.Name;
                                        RaiseEvent_NewMessageToGui("  " + warningString, MessageType.warning);

                                        _eventArgsOfAuditOrExportRun.entitiesThatCausedWarningInLastAudit[segment] = warningString;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    RaiseEvent_NewMessageToGui("...ok.");
                }

                percentageDone = 90;
                worker.ReportProgress(percentageDone);                
            }

            //e.Result = _eventArgsOfAuditOrExportRun;      // pass the result to the _Completed Handler
       }


        // This event handler updates the progress. 
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            EventHandler<EventArgs_ProgressBarUpdate> handler = EventProgressBarUpdate;
            if (handler != null)
            {
                handler(this, new EventArgs_ProgressBarUpdate(e.ProgressPercentage));
            }
        }



        // this will be called both for the first and second part (writing metafiles) of the export process.

        private void bgWorkerExportToUnity_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EventArgs_AuditOrExportResult result = e.Result as EventArgs_AuditOrExportResult;

            if (e.Cancelled == true)
            {
                result.Canceled = true;
            }

            ExportInitiated = false;

            EventHandler<EventArgs_AuditOrExportResult> handler = EventExportCompleted;
            if (handler != null)
            {
                handler(this, result);
            }
        }


        // This event handler deals with the results of the background operation. 
        private void bgWorkerProjectAudit_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //_eventArgsOfAuditOrExportRun = e.Result as EventArgs_AuditOrExportResult;

            if (e.Cancelled == true)
            {
                _eventArgsOfAuditOrExportRun.Canceled = true;
                _eventArgsOfAuditOrExportRun.ResultMessage = "Audit has been canceled!";
            }
            else
            {
                if (_eventArgsOfAuditOrExportRun.ErrorOccurred)
                {
                    _eventArgsOfAuditOrExportRun.Caption = "AUDIT FAILED";
                    _eventArgsOfAuditOrExportRun.ResultMessage = "Your soundtrack contains errors. The related entities have been marked in the Source Tree. Please see the Log- and Tooltip windows for more information.";
                }
                else if (_eventArgsOfAuditOrExportRun.WarningOccurred)
                {
                    _eventArgsOfAuditOrExportRun.Caption = "WARNINGS";
                    _eventArgsOfAuditOrExportRun.ResultMessage = "Your soundtrack will generally work, but some problems have been detected. The related entities have been marked in the Source Tree. Please see the Log- and Tooltip windows for more information.";
                }
                else
                {
                    _eventArgsOfAuditOrExportRun.ResultMessage = "AUDIT SUCCEEDED";
                    _eventArgsOfAuditOrExportRun.ResultMessage = "Your soundtrack looks good!";
                }
            }

            EventHandler<EventArgs_AuditOrExportResult> handler = EventAuditCompleted;
            if (handler != null)
            {
                handler(this, _eventArgsOfAuditOrExportRun);
            }
        }


        internal void ExportSoundtrackToUnityRequested(string psaiCoreBinaryFilename)
        {
            if (_eventArgsOfAuditOrExportRun == null)
            {
                _eventArgsOfAuditOrExportRun = new EventArgs_AuditOrExportResult();
            }
            
            _eventArgsOfAuditOrExportRun.PsaiCoreBinaryFilename = psaiCoreBinaryFilename;

            ExportInitiated = true;

            if (ProjectDataChangedSinceLastAudit || (!ProjectDataChangedSinceLastAudit && _eventArgsOfAuditOrExportRun != null && _eventArgsOfAuditOrExportRun.ErrorOccurred))
            {
                DoAudit();
            }
            else
            {
                ExportSoundtrackToUnity();
            }
        }


        internal void ExportSoundtrackToUnity()
        {
            if (_eventArgsOfAuditOrExportRun != null && _eventArgsOfAuditOrExportRun.PsaiCoreBinaryFilename != null)
            {
                EventHandler<EventArgs_ProgressBarStart> handler = EventProgressBarStart;
                if (handler != null)
                {
                    handler(this, new EventArgs_ProgressBarStart("Exporting Soundtrack to Unity Project"));
                }


                if (!_bgWorkerExportToUnity.IsBusy)
                {
                    _bgWorkerExportToUnity.RunWorkerAsync();
                }
            }
            else
            {
                RaiseEvent_NewMessageToGui("INTERNAL ERROR - _eventARgsOfAuditOrExportRun or _eventArgsOfAuditOrExportRun.PsaiCoreBinaryFilename is null.  _eventArgs=" + _eventArgsOfAuditOrExportRun);
            }
        }


        internal void CancelExport()
        {
            if (_bgWorkerProjectAudit.IsBusy)
            {
                _bgWorkerProjectAudit.CancelAsync();
            }

            if (_bgWorkerExportToUnity.IsBusy)
            {
                _bgWorkerExportToUnity.CancelAsync();
            }

            ExportInitiated = false;
        }

#endif


        internal bool DoAuditCheckIndirectionToEndForAllThemes(psai.net.Soundtrack soundtrack, out Dictionary<int, Dictionary<int, int>> mapThemeIdsToSegmentsIdsToSteps)
        {
            /* key = ThemeId, value = SegmentIds, number of segments needed to transition to End ;  0 == End-Segment ; -1 = no transition to end exists. */
            mapThemeIdsToSegmentsIdsToSteps = new Dictionary<int, Dictionary<int, int>>();                   
            bool atLeastOneErrorOccurred = false;

            foreach (int themeId in soundtrack.m_themes.Keys)
            {
                Dictionary<int, int> singleResultMap = null;

                bool singleAuditResult = DoAuditCheckIndirectionToEndForTheme(soundtrack, themeId, out singleResultMap);

                mapThemeIdsToSegmentsIdsToSteps[themeId] = singleResultMap;

                if (!singleAuditResult)
                {
                    atLeastOneErrorOccurred = true;
                }
            }

            return !atLeastOneErrorOccurred;
        }


        private bool DoAuditCheckIndirectionToEndForTheme(psai.net.Soundtrack soundtrack, int themeId, out Dictionary<int, int> mapSegmentIdsToSteps)
        {
            mapSegmentIdsToSteps = new Dictionary<int, int>();   // value 0 = End-Segment, value -1 = deadend, value x = indirection steps to end
            bool atLeastOneErrorOccurred = false;

            psai.net.Theme theme = soundtrack.m_themes[themeId];

            if (theme.themeType == ThemeType.highlightLayer)
            {
                return true;
            }
            else
            {
                foreach (psai.net.Segment segment in theme.m_segments)
                {
                    int redirectionSteps = 1;

                    if (!segment.IsUsableAs(SegmentSuitability.end))
                    {
                        psai.net.Segment nextSegment = segment.nextSnippetToShortestEndSequence;
                        while (nextSegment != null && !nextSegment.IsUsableAs(SegmentSuitability.end))
                        {
                            redirectionSteps++;
                            nextSegment = nextSegment.nextSnippetToShortestEndSequence;
                        }

                        if (nextSegment == null || !nextSegment.IsUsableAs(SegmentSuitability.end))
                        {
                            mapSegmentIdsToSteps[segment.Id] = -1;
                            atLeastOneErrorOccurred = true;
                        }
                        else
                        {
                            mapSegmentIdsToSteps[segment.Id] = redirectionSteps;
                        }
                    }
                    else
                    {
                        mapSegmentIdsToSteps[segment.Id] = 0;
                    }
                }

                return !atLeastOneErrorOccurred;
            }
        }

        // returns true if the prebeat times of all Segments are short enough
        // out segmentsWithHighPrebeatTimes: a List of the Ids of all Segments that have a Prebeat-Times which exceeds the warning threshold
        internal bool DoAuditPrebeatTimes(out List<int> segmentsWithHighPrebeatTimes)
        {
            segmentsWithHighPrebeatTimes = new List<int>();

            HashSet<Segment> allSegments = Project.GetSegmentsOfAllThemes();
            foreach (Segment segment in allSegments)
            {
                int prebeatLengthInMs = segment.AudioData.GetMillisecondsFromSampleCount(segment.AudioData.PreBeatLengthInSamples);
                if (prebeatLengthInMs > this.Project.Properties.WarningThresholdPreBeatMillis)
                {
                    segmentsWithHighPrebeatTimes.Add(segment.Id);
                }
            }

            return (segmentsWithHighPrebeatTimes.Count == 0);
        }


        internal bool DoAuditCheckAllSegmentsForZeroSuitability(out Dictionary<PsaiMusicEntity, string> errorMap)
        {
            errorMap = new Dictionary<PsaiMusicEntity, string>();
            bool auditFailed = false;            

            foreach (Theme theme in Project.Themes)
            {
                if (theme.ThemeTypeInt == (int)ThemeType.highlightLayer)
                {
                    continue;
                }
                else
                {
                    foreach (Group group in theme.Groups)
                    {
                        foreach(Segment segment in group.Segments)
                        {
                            if (!segment.IsUsableAtStart && !segment.IsUsableAtEnd && !segment.IsUsableInMiddle && !segment.IsAutomaticBridgeSegment)
                            {
                                auditFailed = true;
                                StringBuilder sb = new StringBuilder();
                                sb.Append("WARNING: Segment '");
                                sb.Append(segment.Name);
                                sb.Append("' has no suitabilites set and thus will never be played. ");
                                errorMap[segment] = sb.ToString();
                            }
                        }
                    }
                }
            }
            return auditFailed;
        }


        internal bool DoAuditCheckAllGroupsForStartMiddleEnd(out Dictionary<PsaiMusicEntity, string> errorMap)
        {           
            errorMap = new Dictionary<PsaiMusicEntity, string>();
            bool auditFailed = false;            

            foreach (Theme theme in Project.Themes)
            {
                if (theme.ThemeTypeInt == (int)ThemeType.highlightLayer)
                {
                    continue;
                }
                else
                {
                    foreach (Group group in theme.Groups)
                    {
                        bool foundStart = false;
                        bool foundMiddle = false;
                        bool foundEnd = false;

                        foreach(Segment segment in group.Segments)
                        {
                            if (segment.IsUsableAtStart)
                                foundStart = true;

                            if (segment.IsUsableInMiddle)
                                foundMiddle = true;

                            if (segment.IsUsableAtEnd)
                                foundEnd = true;

                            if (foundStart && foundMiddle && foundEnd)
                            {
                                break;
                            }
                        }

                        if (!(foundStart && foundMiddle && foundEnd))
                        {
                            auditFailed = true;

                            StringBuilder sb = new StringBuilder();
                            sb.Append("ERROR: Group '");
                            sb.Append(group.Name);
                            sb.Append(" (");
                            sb.Append(theme.Name);
                            sb.Append(") ");
                            sb.Append("' is lacking a Segment suitable for ");

                            if (!foundStart)
                                sb.Append("[START] ");

                            if (!foundMiddle)
                                sb.Append("[MIDDLE] ");

                            if (!foundEnd)
                                sb.Append("[END]");

                            sb.Append("!");

                            errorMap.Add(group, sb.ToString());
                        }
                    }
                }
            }

            return auditFailed;
        }


        public void ImportThemesFromOtherProject(ref PsaiProject targetProject, PsaiProject otherProject, List<Theme> themesToImport)
        {
            int nextFreeSegmentId = Math.Max(targetProject.GetHighestSegmentId(), otherProject.GetHighestSegmentId()) + 1;

            // handle id collisions
            foreach (Theme theme in themesToImport)
            {
                // check for Theme Id collisions
                Theme themeInProjectWithSameId = targetProject.GetThemeById(theme.Id);
                if (themeInProjectWithSameId != null)
                {
                    theme.Id = targetProject.GetNextFreeThemeId(theme.Id);
                    //theme.SetAsParentThemeForAllGroupsAndSnippets();
                    EditorModel.Instance.RaiseEvent_NewMessageToGui("...Theme id of Theme " + theme.Name + " has been changed to " + theme.Id + " because of id collision with Theme " + themeInProjectWithSameId.Name);
                }

                // check for Snippet id collisions                          
                HashSet<Segment> importedSnippets = theme.GetSegmentsOfAllGroups();
                foreach (Segment importedSnippet in importedSnippets)
                {
                    if (targetProject.GetSnippetById(importedSnippet.Id) != null)
                    {
                        EditorModel.Instance.RaiseEvent_NewMessageToGui("SegmentId collision (" + importedSnippet.Id + ") -> assigning new id (" + nextFreeSegmentId + ") to Segment " + importedSnippet.Name);
                        importedSnippet.Id = nextFreeSegmentId;
                        nextFreeSegmentId++;
                    }
                }
            }

            // build a Set of all imported Theme Ids
            HashSet<int> importedThemeIds = new HashSet<int>();
            foreach (Theme importedTheme in themesToImport)
            {
                importedThemeIds.Add(importedTheme.Id);
            }

            // build a Set of all imported Segments
            HashSet<Segment> allSegmentsOfImportProject = otherProject.GetSegmentsOfAllThemes();
            HashSet<Segment> allSegmentsOfImportedThemes = new HashSet<Segment>();
            foreach (Segment importedSegment in allSegmentsOfImportProject)
            {
                if (importedThemeIds.Contains(importedSegment.ThemeId))
                {
                    allSegmentsOfImportedThemes.Add(importedSegment);
                }
            }

            // remove those Snippets from manualLinked/Blocked Lists, which have not been imported.                        
            foreach (Segment snippet in allSegmentsOfImportedThemes)
            {
                int i = 0;
                while (i < snippet.ManuallyBlockedSnippets.Count)
                {
                    if (importedThemeIds.Contains(snippet.ManuallyBlockedSnippets.ElementAt(i).ThemeId))
                    {
                        i++;
                    }
                    else
                    {
                        Segment snippetToRemove = snippet.ManuallyBlockedSnippets.ElementAt(i);
                        snippet.ManuallyBlockedSnippets.Remove(snippetToRemove);
                    }
                }


                i = 0;
                while (i < snippet.ManuallyLinkedSnippets.Count)
                {
                    if (importedThemeIds.Contains(snippet.ManuallyLinkedSnippets.ElementAt(i).ThemeId))
                    {
                        i++;
                    }
                    else
                    {
                        Segment snippetToRemove = snippet.ManuallyLinkedSnippets.ElementAt(i);
                        snippet.ManuallyLinkedSnippets.Remove(snippetToRemove);
                    }
                }

            }

            // remove those Themes from theme.ManuallyBlockedTargetThemes, which have not been imported
            foreach (Theme theme in otherProject.Themes)
            {
                int i = 0;
                while (i < theme.ManuallyBlockedTargetThemes.Count)
                {
                    if (importedThemeIds.Contains(theme.ManuallyBlockedTargetThemes.ElementAt(i).Id))
                    {
                        i++;
                    }
                    else
                    {
                        Theme themeToRemove = theme.ManuallyBlockedTargetThemes.ElementAt(i);
                        theme.ManuallyBlockedTargetThemes.Remove(themeToRemove);
                    }
                }

                // filter those Groups and Manual BridgeSnippets from imported Groups, of Themes which have not been imported
                i = 0;
                foreach (Group group in theme.Groups)
                {
                    // remove those Snippets from Group.BridgeSnippets, which have not been imported
                    i = 0;
                    while (i < group.ManualBridgeSnippetsOfTargetGroups.Count)
                    {
                        if (importedThemeIds.Contains(group.ManualBridgeSnippetsOfTargetGroups.ElementAt(i).Id))
                        {
                            i++;
                        }
                        else
                        {
                            Segment snippetToRemove = group.ManualBridgeSnippetsOfTargetGroups.ElementAt(i);
                            group.ManualBridgeSnippetsOfTargetGroups.Remove(snippetToRemove);
                        }
                    }

                    // remove ManuallyBlocked Groups
                    i = 0;
                    while (i < group.ManuallyBlockedGroups.Count)
                    {
                        if (importedThemeIds.Contains(group.ManuallyBlockedGroups.ElementAt(i).Theme.Id))
                        {
                            i++;
                        }
                        else
                        {
                            Group groupToRemove = group.ManuallyBlockedGroups.ElementAt(i);
                            group.ManuallyBlockedGroups.Remove(groupToRemove);
                        }
                    }

                    // remove ManuallyLinked Groups
                    i = 0;
                    while (i < group.ManuallyLinkedGroups.Count)
                    {
                        if (importedThemeIds.Contains(group.ManuallyLinkedGroups.ElementAt(i).Theme.Id))
                        {
                            i++;
                        }
                        else
                        {
                            Group groupToRemove = group.ManuallyLinkedGroups.ElementAt(i);
                            group.ManuallyLinkedGroups.Remove(groupToRemove);
                        }
                    }

                }
            }

            // add imported themes
            foreach (Theme theme in themesToImport)
            {
                CreateAndExecuteNewCommand_AddPsaiEntity(targetProject, theme);
            }
        }




#if (PSAI_EDITOR_STANDALONE)
        internal void BuildOggVorbis(string oggpath, DataReceivedEventHandler handler)
        {
            string filenameOggEnc = "oggenc.exe";
            string applicationStartupDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

            string oggEncPath = Path.Combine(applicationStartupDirectory, filenameOggEnc);

            if (!File.Exists(oggEncPath))
            {
                RaiseEvent_NewMessageToGui("oggenc.exe not found! oggEncPath=" + oggEncPath, MessageType.critical);
                RaiseEvent_NewMessageToGui("Ogg/Vorbis encoding failed.");
                
                return;
            }


            HashSet<Segment> allSnippets = Project.GetSegmentsOfAllThemes();

            float soundQuality = Project.Properties.ExportSoundQualityInPercent / 10.0f;       // OggVorbis: float between -1 (lowest) and 10 (highest quality)


            foreach (Segment segment in allSnippets)
            {
                // TODO: what happens if we have the same filenames within different subdirectories?
                string filename = Path.GetFileNameWithoutExtension(Path.GetFileName(segment.AudioData.FilePathRelativeToProjectDirForCurrentSystem));   
                string fullOutAudioFilepath = Path.Combine(oggpath, filename);
                fullOutAudioFilepath += ".ogg";

                bool oggFileExists = File.Exists(fullOutAudioFilepath);

                if (oggFileExists && Project.Properties.ForceFullRebuild == false)
                {
                    string message = "- Skipped encoding of " + filename + ", file already exists. Enable 'Force Full Rebuild' checkbox in the Project Properties Panel to re-encode.";
                    RaiseEvent_NewMessageToGui(message);
                }
                else if (Project.Properties.ForceFullRebuild || oggFileExists)
                {
                    string arguments = " ";
                    //arguments += "--quiet ";
                    arguments += "--quality " + soundQuality.ToString() + " ";
                    arguments += "--output ";
                    arguments += "\"";
                    arguments += fullOutAudioFilepath;
                    arguments += "\"";
                    arguments += " \"";
                    arguments += EditorModel.Instance.GetFullPathOfAudioFile(EditorModel.Instance.ProjectDir, segment.AudioData);
                    arguments += "\"";

                    //string wavDir = Path.Combine(EditorModel.Instance.PathToProjectDir, EditorModel.Instance.Project.Properties.PathToAudioData);

                    ProcessStartInfo startInfo = new ProcessStartInfo();

                    startInfo.FileName = oggEncPath;

                    startInfo.Arguments = arguments;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardError = true;

                    //if (File.Exists(Path.Combine(startInfo.WorkingDirectory, startInfo.FileName)))
                    {
                        //Console.WriteLine("oggenc.exe found");

                        try
                        {
                            Process p = new Process();
                            p.StartInfo = startInfo;
                            p.OutputDataReceived += handler;
                            p.ErrorDataReceived += handler;
                            p.Start();

                            p.BeginErrorReadLine();
                            p.BeginOutputReadLine();

                            p.WaitForExit();

                            if (p.ExitCode == 0)
                            {
                                //System.Windows.Forms.MessageBox.Show("Ogg/Vorbis encoding was successful");
                            }
                            else
                            {
                                RaiseEvent_NewMessageToGui("Ogg/Vorbis encoding failed ! Make sure that oggenc.exe and all its related .dll files are located in the following directory: " + oggEncPath, MessageType.critical);
                                return;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.InnerException.ToString());
                        }
                    }
                }
            }

            RaiseEvent_NewMessageToGui("Ogg/Vorbis encoding complete.");
        }
#endif


        /*
        internal PsaiProject DeserializeXmlProject(string filename)        
        {
            PsaiProject project = null;
            try
            {
                TextReader reader = new StreamReader(filename);
                XmlSerializer serializer = new XmlSerializer(typeof(PsaiProject));

                project = (PsaiProject)serializer.Deserialize(reader);
                reader.Close();
                project.ReconstructReferencesAfterXmlSerialization();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
                RaiseEvent_OperationFailed("Failed to load Project due to invalid .psai file.\r\n", ex);
            }

            return project;
        }
        */

        // loads a new XML Project file into the Editor
        internal void XmlProjectLoad(string filename)
        {
            _project = psai.Editor.PsaiProject.LoadProjectFromXmlFile(filename);            

            FullProjectPath = filename;

            if (_project != null)
            {
                DoUpdateAllAudioDataBasedOnFileHeaders(Path.GetDirectoryName(filename),_project);
                RaiseEvent_SoundtrackLoaded();
            }
            else
            {
                RaiseEvent_NewMessageToGui("...failed to load project due to invalid .psai file.");
            }

            #if PSAI_EDITOR_STANDALONE            
                //_fileSystemWatcher.Path = ProjectDir;        
            #endif
        }

        internal void PushCommand(ICommand command)
        {
            #if DEBUG
                Console.WriteLine("PushCommand() command=" + command + "   " + command.GetHashCode());
            #endif

            // first clear everything after the commandQueueUndoIndex. Redo will not be possible after a new
            // command has been inserted in the middle of the command stack
            if (_commandQueue.Count - 1 > _commandQueueUndoIndex)
            {
                int noOfCommandsToRemove = _commandQueue.Count - _commandQueueUndoIndex - 1;
                _commandQueue.RemoveRange(_commandQueueUndoIndex + 1, noOfCommandsToRemove);

                #if DEBUG
                    Console.WriteLine("PushCommand() commands removed=" + noOfCommandsToRemove);
                #endif
            }
            _commandQueueUndoIndex++;
            _commandQueue.Add(command);
            RaiseEvent_CommandQueueChanged();
        }

        internal bool UndoIsAvailable()
        {
            return (_commandQueue.Count > 0 && _commandQueueUndoIndex >= 0);
        }

        internal bool RedoIsAvailable()
        {
            return (_commandQueue.Count > 0 && _commandQueueUndoIndex + 1 < _commandQueue.Count);
        }

        internal string GetStringOfNextCommandToUndo()
        {
            if (UndoIsAvailable())
            {
                return _commandQueue[_commandQueueUndoIndex].ToString();
            }
            return "";
        }

        internal string GetStringOfNextCommandToRedo()
        {
            if (RedoIsAvailable())
            {
                return _commandQueue[_commandQueueUndoIndex + 1].ToString();
            }
            return "";
        }

        internal ICommand UndoLastCommand()
        {
            if (_commandQueue.Count > 0)
            {
                ICommand lastCommand = _commandQueue[_commandQueueUndoIndex];

                #if DEBUG
                    Console.WriteLine("UndoLastCommand() lastCommand=" + lastCommand.GetHashCode());
                #endif

                lastCommand.Undo();
                ProjectDataChanged();

                _commandQueueUndoIndex--;               
                
                #if DEBUG
                    Console.WriteLine("Command queue after Undo():");
                    Debug_PrintCommandQueue();
                #endif

                RaiseEvent_CommandQueueChanged();
                return lastCommand;
            }
            return null;
        }

        internal void Debug_PrintCommandQueue()
        {
            #if DEBUG
                Console.WriteLine("_commandQueue.Count=" + _commandQueue.Count + "   _commandQueueUndoIndex= " + _commandQueueUndoIndex);
                for (int i = _commandQueue.Count - 1; i >= 0; i--)
                {
                    ICommand command = _commandQueue[i];
                    Console.WriteLine(i + ". " + command.GetHashCode() + "   " + command);
                }
            #endif
        }

        internal ICommand RedoLastCommand()
        {
            if (_commandQueue.Count > _commandQueueUndoIndex + 1)
            {
                _commandQueueUndoIndex++;
                RaiseEvent_CommandQueueChanged();

                #if DEBUG
                    Console.WriteLine("command queue after Redo():");
                    Debug_PrintCommandQueue();
                #endif
                
                ICommand lastCommand = _commandQueue[_commandQueueUndoIndex];
                
                #if DEBUG
                    Console.WriteLine("Redo command " + lastCommand);
                #endif

                lastCommand.Execute();
                ProjectDataChanged();

                return lastCommand;
            }
            return null;
        }

        internal void PushAndExecuteCommand(ICommand command)
        {
            PushCommand(command);
            command.Execute();
            ProjectDataChanged();
        }

        internal void CreateAndExecuteNewCommand_AddSegments(PsaiProject project, string[] filenames, Group parentGroup)
        {
            CommandAddSegments command = new CommandAddSegments(project, filenames, parentGroup);
            PushAndExecuteCommand(command);
        }

        public void CreateAndExecuteNewCommand_DeletePsaiEntity(PsaiProject project, PsaiMusicEntity entity)
        {
            //CommandDeletePsaiEntity command = new CommandDeletePsaiEntity(entity, this.Project);
            ICommand command = null;

            if (entity is Segment)
            {
                Segment snippet = entity as Segment;
                command = new CommandDeleteSegment(project, snippet);
            }

            if (entity is Group)
            {
                Group group = entity as Group;
                command = new CommandDeleteGroup(project, group);
            }

            if (entity is Theme)
            {
                Theme theme = entity as Theme;
                command = new CommandDeleteTheme(project, theme);
            }

            PushAndExecuteCommand(command);
        }


        public void CreateAndExecuteNewCommand_AddPsaiEntity(PsaiProject project, PsaiMusicEntity newEntity)
        {
            CreateAndExecuteNewCommand_AddPsaiEntity(project, newEntity, -1);   // default behavior: add to the end
        }


        public void CreateAndExecuteNewCommand_AddPsaiEntity(PsaiProject project, PsaiMusicEntity newEntity, int targetIndex)
        {
            CommandAddPsaiEntity command = new CommandAddPsaiEntity(project, newEntity, targetIndex);
            PushAndExecuteCommand(command);
        }

        public void CreateAndExecuteNewCommand_ChangePsaiEntityProperties(PsaiMusicEntity changedEntity, PsaiMusicEntity oldEntity, string nameOfChangedProperty)
        {
            CommandChangePsaiEntityProperty command = new CommandChangePsaiEntityProperty(changedEntity, ref oldEntity, nameOfChangedProperty);
            PushAndExecuteCommand(command);
        }

        public void CreateAndExecuteNewCommand_ChangeProjectProperties(ProjectProperties changedProperties)
        {
            CommandChangeProjectProperties command = new CommandChangeProjectProperties(changedProperties);
            PushAndExecuteCommand(command);
        }

        public void CreateAndExecuteNewCommand_CompatibilityChange(PsaiMusicEntity sourceEntity, PsaiMusicEntity targetEntity, CompatibilitySetting newSetting)
        {
            CommandChangeCompatibility command = new CommandChangeCompatibility(sourceEntity, targetEntity, newSetting);
            PushAndExecuteCommand(command);
        }

        public void CreateAndExecuteNewCommand_DeclareBridgeSnippet(Segment snippet, Group sourceGroup)
        {
            CommandDeclareManualBridgeSegment command = new CommandDeclareManualBridgeSegment(snippet, sourceGroup);
            PushAndExecuteCommand(command);
        }

        public void CreateAndExecuteNewCommand_RevertBridgeSnippet(Segment snippet, Group sourceGroup)
        {
            CommandRevertBridgeSnippet command = new CommandRevertBridgeSnippet(snippet, sourceGroup);
            PushAndExecuteCommand(command);
        }

        public void CreateAndExecuteNewCommand_MoveSnippetsToGroup(List<Segment> snippets, Group group, int index)
        {
            CommandMoveSegmentsToGroup command = new CommandMoveSegmentsToGroup(snippets, group, index);
            PushAndExecuteCommand(command);
        }


        public void RaiseEvent_CompatibilitySettingChanged(EventArgs_CompatibilitySettingChanged e)
        {
            EventHandler<EventArgs_CompatibilitySettingChanged> handler = CompatibilitySettingChangedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void RaiseEvent_PsaiEntityPropertyChanged(EventArgs_PsaiEntityPropertiesChanged e)
        {
            EventHandler<EventArgs_PsaiEntityPropertiesChanged> handler = PsaiEntityPropertiesChangedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void RaiseEvent_PsaiEntityDeleted(EventArgs_PsaiEntityDeleted e)
        {
            EventHandler<EventArgs_PsaiEntityDeleted> handler = PsaiEntityDeletedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void RaiseEvent_PsaiEntityAdded(EventArgs_PsaiEntityAdded e)
        {
            EventHandler<EventArgs_PsaiEntityAdded> handler = PsaiEntityAddedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }



        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        internal void RaiseEvent_SegmentMovedToGroup(EventArgs_SegmentMovedToGroup e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<EventArgs_SegmentMovedToGroup> handler = SegmentMovedToGroupEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        internal void RaiseEvent_SoundtrackLoaded()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<EventArgs> handler = PsaiProjectLoadedEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, new EventArgs());
            }
        }

        // used by Undo()-Operations where a previous Project clone is reloaded
        internal void RaiseEvent_ProjectReloaded()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<EventArgs> handler = PsaiProjectReloadedEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, new EventArgs());
            }
        }

        internal void RaiseEvent_OperationFailed(string errorMessage, Exception exception)
        {
            EventHandler<EventArgs_OperationFailed> handler = OperationFailedEvent;
            if (handler != null)
            {
                handler(this, new EventArgs_OperationFailed(errorMessage + "  " + exception.Message + "  InnerException: " + exception.InnerException.Message, exception));
            }
        }

        internal void RaiseEvent_ManualBridgeSegmentToggled(EventArgs_BridgeSegmentToggled e)
        {
            EventHandler<EventArgs_BridgeSegmentToggled> handler = BridgeSegmentToggledEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void RaiseEvent_NewMessageToGui(string message, MessageType type = MessageType.normal)
        {
            EventArgs_NewMessageToGui e = new EventArgs_NewMessageToGui(message, type);
            EventHandler<EventArgs_NewMessageToGui> handler = EventNewMessageToGui;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void RaiseEvent_ProjectPropertiesChanged()
        {
            EventHandler<EventArgs> handler = ProjectPropertiesChangedEvent;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        internal void RaiseEvent_FileInProjectDirChanged(object source, FileSystemEventArgs e)
        {
            EventHandler<FileSystemEventArgs> handler = EventFileInProjectDirChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        internal void RaiseEvent_ProjectSaved()
        {
            EventHandler<EventArgs> handler = ProjectSavedEvent;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        internal void RaiseEvent_CommandQueueChanged()
        {
            EventHandler<EventArgs> handler = CommandQueueChangedEvent;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }


        // just to let the GUI know that some data changed, for updating the menu items
        internal void RaiseEvent_ProjectDataChanged()
        {
            EventHandler<EventArgs> handler = ProjectDataChangedEvent;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        internal void RaiseEvent_NewProjectInitialized()
        {
            EventHandler<EventArgs> handler = NewProjectInitializedEvent;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        internal Theme CreateNewTheme()
        {
            int newThemeId = EditorModel.Instance.Project.GetNextFreeThemeId(1);
            Theme theme = new Theme(newThemeId);
            theme.AddGroup(new Group(theme, "default group"));
            return theme;
        }
    }


    public static class PnxHelperz
    {
        public static void CopyTo(this object S, object T)
        {
            foreach (var pS in S.GetType().GetProperties())
            {
                foreach (var pT in T.GetType().GetProperties())
                {
                    if (pT.Name != pS.Name) continue;

                    if (pT.GetSetMethod() != null)
                    {
                        (pT.GetSetMethod()).Invoke(T, new object[] { pS.GetGetMethod().Invoke(S, null) });
                    }                    
                }
            };
        }


        public static bool PublicInstancePropertiesEqual<T>(T self, T to, params string[] propertiesToCompare) where T : class
        {
            if (self != null && to != null)
            {
                Type type = typeof(T);
                List<string> checkList = new List<string>(propertiesToCompare);
                foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if (checkList.Contains(pi.Name))
                    {
                        object selfValue = type.GetProperty(pi.Name).GetValue(self, null);
                        object toValue = type.GetProperty(pi.Name).GetValue(to, null);

                        if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return self == to;
        }

    }
}
        