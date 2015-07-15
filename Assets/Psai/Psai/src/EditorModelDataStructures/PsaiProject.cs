using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using System.IO;
using System.Xml;

namespace psai.Editor
{

    /* This class represents a Psai Project along with all settings made in the Editor
    */
    public class PsaiProject : ICloneable
    {
        public static readonly string SERIALIZATION_PROTOCOL_VERSION = "1.0";

        private ProjectProperties _projectProperties = new ProjectProperties();
        private List<Theme> _themes = new List<Theme>();


        public string InitialExportDirectory
        {
            get;
            set;
        }

        public string SerializedByProtocolVersion
        {
            get;
            set;
        }

        public ProjectProperties Properties
        {
            get { return _projectProperties; }
            set { _projectProperties = value; }
        }

        public List<Theme> Themes
        {
            get { return _themes; }
            set { _themes = value; }
        }


        public void Init()
        {
            _projectProperties = new ProjectProperties();
            _themes.Clear();
        }


        public static PsaiProject LoadProjectFromStream(Stream stream)
        {
            PsaiProject project = null;

            try
            {
                TextReader reader = new StreamReader(stream);
                XmlSerializer serializer = new XmlSerializer(typeof(PsaiProject));
                project = (PsaiProject)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            project.ReconstructReferencesAfterXmlDeserialization();
            return project;

        }


        public static PsaiProject LoadProjectFromXmlFile(string filename)
        {
            try
            {
                FileStream stream = new FileStream(filename, System.IO.FileMode.Open);
                return LoadProjectFromStream(stream);
            }
            catch (Exception e)
            {

                throw e;
            }
        }


        public void SaveAsXmlFile(string filename)
        {
            PrepareForXmlSerialization();

            try
            {
                TextWriter writer = new StreamWriter(filename);
                XmlSerializer serializer = new XmlSerializer(typeof(PsaiProject));

                serializer.Serialize(writer, this);
                writer.Close();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }




        // just for debugging
        public void Report(bool reportGroups, bool reportSegments)
        {
#if DEBUG

            Console.WriteLine("PsaiProject.Report() Themes:");
            foreach (Theme theme in _themes)
            {
                Console.WriteLine(theme.Name);

                if (reportGroups)
                {
                    foreach (Group group in theme.Groups)
                    {
                        Console.WriteLine("Group: " + group.ToString());

                        if (reportSegments)
                        {
                            foreach (Segment segment in group.Segments)
                            {
                                Console.WriteLine("Segment: " + segment.ToString());
                            }
                        }
                    }
                }
            }
#endif
        }

        public bool ConvertProjectFile_From_Legacy_To_0_9_12(string pathToProjectFile)
        {
            if (File.Exists(pathToProjectFile))
            {

                Stream inputFileStream = new FileStream(pathToProjectFile, FileMode.Open, FileAccess.Read);

                XmlReaderSettings settings = new XmlReaderSettings();
                using (XmlReader reader = XmlReader.Create(inputFileStream, settings))
                {

                    // 1. Zeile mit "<PathToAudioData>" droppen, aber den string in der temporären Variable "pathToAudioData" speichern

                    // 2. alle Zeilen mit "<FileName>" droppen

                    // 2. <Snippets> in <Segments> umbenamsen

                    // 3. <Snippet> in <Segment> umbenamsen

                    // 4. <FullPathToFile>
                    //      enthielt früher eine absolute Pfadangabe. Davon nur noch den letzten Teil (Dateinamen) verwenden, und ändern in
                    //      <Path>[pathToAudioData]/[filename]</Path>

                }
            }

            return false;
        }


        // this needs to be called after an XML-Project has been deserialized
        public void ReconstructReferencesAfterXmlDeserialization()
        {

            // update Group & Theme references
            foreach (Theme theme in _themes)
            {
                foreach (Group group in theme.Groups)
                {
                    group.Theme = theme;
                    foreach (Segment snippet in group.Segments)
                    {
                        snippet.Group = group;
                    }
                }
            }

            // reconstruct manually linked/blocked lists
            foreach (Theme theme in _themes)
            {
                theme.ManuallyBlockedTargetThemes.Clear();
                foreach (int blockedThemeId in theme.Serialization_ManuallyBlockedThemeIds)
                {
                    Theme blockedTheme = GetThemeById(blockedThemeId);

                    if (blockedTheme != null)
                    {
                        theme.ManuallyBlockedTargetThemes.Add(blockedTheme);
                    }
                }

                foreach (Group group in theme.Groups)
                {
                    group.ManuallyBlockedGroups.Clear();

                    if (group.Serialization_ManuallyBlockedGroupIds == null)
                        group.Serialization_ManuallyBlockedGroupIds = new List<int>();

                    foreach (int blockedGroupId in group.Serialization_ManuallyBlockedGroupIds)
                    {
                        Group blockedGroup = GetGroupBySerializationId(blockedGroupId);

                        if (blockedGroup != null)
                        {
                            group.ManuallyBlockedGroups.Add(blockedGroup);
                        }
                    }


                    group.ManuallyLinkedGroups.Clear();

                    if (group.Serialization_ManuallyLinkedGroupIds == null)
                        group.Serialization_ManuallyLinkedGroupIds = new List<int>();

                    foreach (int linkedGroupId in group.Serialization_ManuallyLinkedGroupIds)
                    {
                        Group linkedGroup = GetGroupBySerializationId(linkedGroupId);

                        if (linkedGroup != null)
                        {
                            group.ManuallyLinkedGroups.Add(linkedGroup);
                        }
                    }

                    group.ManualBridgeSnippetsOfTargetGroups.Clear();

                    if (group.Serialization_ManualBridgeSegmentIds == null)
                        group.Serialization_ManualBridgeSegmentIds = new List<int>();

                    foreach (int bridgeSnippetId in group.Serialization_ManualBridgeSegmentIds)
                    {
                        Segment bridgeSnippet = GetSnippetById(bridgeSnippetId);
                        if (bridgeSnippet != null)
                        {
                            group.ManualBridgeSnippetsOfTargetGroups.Add(bridgeSnippet);
                        }
                    }

                    foreach (Segment snippet in group.Segments)
                    {
                        snippet.ManuallyBlockedSnippets = new HashSet<Segment>();

                        if (snippet.Serialization_ManuallyBlockedSegmentIds == null)
                            snippet.Serialization_ManuallyBlockedSegmentIds = new List<int>();

                        foreach (int blockedSnippetId in snippet.Serialization_ManuallyBlockedSegmentIds)
                        {
                            Segment blockedSnippet = GetSnippetById(blockedSnippetId);
                            snippet.ManuallyBlockedSnippets.Add(blockedSnippet);
                        }

                        snippet.ManuallyLinkedSnippets = new HashSet<Segment>();

                        if (snippet.Serialization_ManuallyLinkedSegmentIds == null)
                            snippet.Serialization_ManuallyLinkedSegmentIds = new List<int>();

                        foreach (int linkedSnippetId in snippet.Serialization_ManuallyLinkedSegmentIds)
                        {
                            Segment linkedSnippet = GetSnippetById(linkedSnippetId);
                            snippet.ManuallyLinkedSnippets.Add(linkedSnippet);
                        }
                    }
                }
            }

            //AssignUniqueInteralIdForAllEntities();
        }



        // Needs to be called before each audit, in case..:
        //      1. a segment suitability has changed
        //      2. a theme type has changed
        //      3. a compatibility has changed
        //      4. a segment has been deleted

        public psai.net.Soundtrack BuildPsaiDotNetSoundtrackFromProject()
        {
            psai.net.Soundtrack soundtrack = new psai.net.Soundtrack();

            foreach (Theme theme in Themes)
            {
                // special treatment for Highlights: We set each Segment Suitabilities for Start, Middle and End.
                // Otherwise the compatibiliy algorithms would mark some Segment transitions as logically_impossible.
                if (theme.ThemeTypeInt == (int)psai.net.ThemeType.highlightLayer)
                {
                    HashSet<Segment> allSegmentsOfHighlightTheme = theme.GetSegmentsOfAllGroups();
                    foreach (Segment segment in allSegmentsOfHighlightTheme)
                    {
                        segment.IsUsableAtEnd = true;
                        segment.IsUsableInMiddle = true;
                        segment.IsUsableAtEnd = true;
                    }
                }

                soundtrack.m_themes.Add(theme.Id, theme.CreatePsaiDotNetVersion());
            }

            HashSet<Segment> segmentSet = GetSegmentsOfAllThemes();
            foreach (Segment segment in segmentSet)
            {
                segment.BuildCompatibleSegmentsSet(this);
                psai.net.Segment netSegment = segment.CreatePsaiDotNetVersion(this);

                soundtrack.m_snippets.Add(netSegment.Id, netSegment);

                psai.net.Theme theme = soundtrack.getThemeById(netSegment.ThemeId);
                theme.m_segments.Add(netSegment);
            }

            //ProtoBuf_PsaiCoreSoundtrack pbSoundtrack = psaiBinarySoundtrack.CreateProtoBuf();
            //result = new psai.net.Soundtrack(pbSoundtrack);

            //result = psaiBinarySoundtrack.CreatePsaiDotNetVersion(this);
            soundtrack.BuildAllIndirectionSequences();

            return soundtrack;
        }


        // Assigns unique ids to each group and updates the id Lists for manually blocked and linked groups.
        // Call this prior Serialization.
        private void PrepareForXmlSerialization()
        {
            SerializedByProtocolVersion = PsaiProject.SERIALIZATION_PROTOCOL_VERSION;

            // update Group & Theme references
            int groupId = 1;
            foreach (Theme theme in _themes)
            {
                foreach (Group group in theme.Groups)
                {
                    group.Serialization_Id = groupId;
                    groupId++;
                }
            }

            // create the Lists containing the Serialization ids for manually linked and blocked entities
            foreach (Theme theme in _themes)
            {
                //themes
                theme.Serialization_ManuallyBlockedThemeIds = new List<int>();
                foreach (Theme blockedTheme in theme.ManuallyBlockedTargetThemes)
                {
                    if (blockedTheme != null)
                        theme.Serialization_ManuallyBlockedThemeIds.Add(blockedTheme.Id);
                }

                // groups
                foreach (Group group in theme.Groups)
                {
                    group.Serialization_ManuallyBlockedGroupIds = new List<int>();
                    foreach (Group blockedGroup in group.ManuallyBlockedGroups)
                    {
                        if (blockedGroup != null)
                            group.Serialization_ManuallyBlockedGroupIds.Add(blockedGroup.Serialization_Id);
                    }

                    group.Serialization_ManuallyLinkedGroupIds = new List<int>();
                    foreach (Group linkedGroup in group.ManuallyLinkedGroups)
                    {
                        if (linkedGroup != null)
                            group.Serialization_ManuallyLinkedGroupIds.Add(linkedGroup.Serialization_Id);
                    }

                    group.Serialization_ManualBridgeSegmentIds = new List<int>();
                    foreach (Segment bridgeSnippet in group.ManualBridgeSnippetsOfTargetGroups)
                    {
                        if (bridgeSnippet != null)
                            group.Serialization_ManualBridgeSegmentIds.Add(bridgeSnippet.Id);
                    }

                    // Snippets
                    foreach (Segment snippet in group.Segments)
                    {
                        snippet.Serialization_ManuallyBlockedSegmentIds = new List<int>();
                        foreach (Segment blockedSnippet in snippet.ManuallyBlockedSnippets)
                        {
                            if (blockedSnippet != null)
                                snippet.Serialization_ManuallyBlockedSegmentIds.Add(blockedSnippet.Id);
                        }

                        snippet.Serialization_ManuallyLinkedSegmentIds = new List<int>();
                        foreach (Segment linkedSnippet in snippet.ManuallyLinkedSnippets)
                        {
                            if (linkedSnippet != null)
                                snippet.Serialization_ManuallyLinkedSegmentIds.Add(linkedSnippet.Id);
                        }
                    }
                }
            }
        }

        public HashSet<Segment> GetSegmentsOfAllThemes()
        {
            HashSet<Segment> snippets = new HashSet<Segment>();
            foreach (Theme theme in Themes)
            {
                snippets.UnionWith(theme.GetSegmentsOfAllGroups());
            }
            return snippets;
        }

        public Theme GetThemeById(int themeId)
        {
            foreach (Theme theme in Themes)
            {
                if (theme.Id == themeId)
                {
                    return theme;
                }
            }
            return null;
        }

        public Segment GetSnippetById(int id)
        {
            foreach (Theme theme in Themes)
            {
                HashSet<Segment> snippets = theme.GetSegmentsOfAllGroups();
                foreach (Segment snippet in snippets)
                {
                    if (snippet.Id == id)
                    {
                        return snippet;
                    }
                }
            }
            return null;
        }

        public Group GetGroupBySerializationId(int id)
        {
            foreach (Theme theme in _themes)
            {
                foreach (Group group in theme.Groups)
                {
                    if (group.Serialization_Id == id)
                    {
                        return group;
                    }
                }
            }
            return null;
        }


        public void AddPsaiMusicEntity(PsaiMusicEntity entity)
        {
            AddPsaiMusicEntity(entity, -1);     // default: enque entity at the end
        }


        public void AddPsaiMusicEntity(PsaiMusicEntity entity, int targetIndex)
        {
            if (entity is Segment)
            {
                Segment snippet = (Segment)entity;

                if (GetSnippetById(snippet.Id) != null)
                {
                    snippet.Id = GetNextFreeSnippetId(snippet.Id);
                }
                if (snippet.Group != null)
                {
                    snippet.Group.AddSegment(snippet, targetIndex);
                }
            }
            else if (entity is Group)
            {
                Group group = (Group)entity;
                if (group.Theme != null)
                {
                    group.Theme.Groups.Add(group);
                }
            }
            else if (entity is Theme)
            {
                Theme theme = (Theme)entity;
                if (GetThemeById(theme.Id) != null)
                {
                    theme.Id = GetNextFreeThemeId(theme.Id);
                }
                Themes.Add(theme);
            }

            //AssignUniqueInteralIdForAllEntities();
        }


        /*
        public void DeleteMusicEntity(int internalId)
        {
            PsaiMusicEntity entity = GetPsaiMusicEntityByInternalId(internalId);

            if (entity != null)
            {
                DeleteMusicEntity(entity);
            }
        }
         */

        public void DeleteMusicEntity(PsaiMusicEntity entity)
        {
            if (entity is Segment)
            {
                Segment deletedSnippet = (Segment)entity;
                if (deletedSnippet.Group != null)
                {
                    deletedSnippet.Group.RemoveSegment(deletedSnippet);
                }

                HashSet<Segment> allSnippets = GetSegmentsOfAllThemes();
                foreach (Segment snippet in allSnippets)
                {
                    if (snippet.ManuallyLinkedSnippets.Contains(deletedSnippet))
                        snippet.ManuallyLinkedSnippets.Remove(deletedSnippet);

                    if (snippet.ManuallyBlockedSnippets.Contains(deletedSnippet))
                        snippet.ManuallyBlockedSnippets.Remove(deletedSnippet);
                }
            }
            else if (entity is Group)
            {
                Group deletedGroup = (Group)entity;
                if (deletedGroup.Theme != null)
                {
                    deletedGroup.Theme.Groups.Remove(deletedGroup);
                }
                // remove from ManuallyLinkes / Blocked Groups
                HashSet<Group> allGroups = GetGroupsOfAllThemes();
                foreach (Group group in allGroups)
                {
                    if (group.ManuallyBlockedGroups.Contains(deletedGroup))
                        group.ManuallyBlockedGroups.Remove(deletedGroup);

                    if (group.ManuallyLinkedGroups.Contains(deletedGroup))
                        group.ManuallyLinkedGroups.Remove(deletedGroup);
                }

            }
            else if (entity is Theme)
            {
                Theme deletedTheme = (Theme)entity;
                Themes.Remove(deletedTheme);

                foreach (Theme theme in Themes)
                {
                    if (theme.ManuallyBlockedTargetThemes.Contains(deletedTheme))
                        theme.ManuallyBlockedTargetThemes.Remove(deletedTheme);
                }
            }
        }


        // returns the highest SegmentId throughout all Themes of the Project
        public int GetHighestSegmentId()
        {
            int highestSegmentId = 0;
            HashSet<Segment> allSegments = GetSegmentsOfAllThemes();
            foreach (Segment segment in allSegments)
            {
                if (segment.Id > highestSegmentId)
                {
                    highestSegmentId = segment.Id;
                }
            }
            return highestSegmentId;
        }

        public int GetNextFreeSnippetId(int idToStartSearchFrom)
        {
            int id = idToStartSearchFrom;
            if (id <= 1)
                id = 1;

            while (GetSnippetById(id) != null)
            {
                id++;
            }
            return id;
        }


        public HashSet<Group> GetGroupsOfAllThemes()
        {
            HashSet<Group> allGroups = new HashSet<Group>();
            foreach (Theme theme in Themes)
            {
                foreach (Group group in theme.Groups)
                {
                    allGroups.Add(group);
                }
            }
            return allGroups;
        }


        public bool CheckIfSnippetIsManualBridgeSnippetForSourceGroup(Segment snippet, Group sourceGroup)
        {
            return (sourceGroup.ManualBridgeSnippetsOfTargetGroups.Contains(snippet));
        }


        public bool CheckIfThereIsAtLeastOneBridgeSnippetFromSourceGroupToTargetGroup(Group sourceGroup, Group targetGroup)
        {
            return (targetGroup.ContainsAtLeastOneAutomaticBridgeSegment() || targetGroup.ContainsAtLeastOneManualBridgeSegmentForSourceGroup(sourceGroup));
        }

        // returns true if the given Snippet is a manual Bridge Snippet to any group, false otherwise.
        // If a reference to a List<Group> is passed, the method will clear it and fill it with all
        // groups, for which this Snippet is a Manual Bridge Snippet. Pass null to speed up the calculation.
        public bool CheckIfSnippetIsManualBridgeSnippetToAnyGroup(Segment snippet, bool getGroups, out List<Group> groups)
        {
            groups = new List<Group>();

            foreach (Theme theme in Themes)
            {
                foreach (Group group in theme.Groups)
                {
                    if (group.ManualBridgeSnippetsOfTargetGroups.Contains(snippet))
                    {
                        if (!getGroups)
                        {
                            return true;
                        }
                        else
                        {
                            groups.Add(group);
                        }
                    }
                }
            }

            return (groups.Count > 0);
        }


        public void DoUpdateAllParentThemeIdsAndGroupsOfChildPsaiEntities()
        {
            foreach (Theme theme in Themes)
            {
                foreach (Group group in theme.Groups)
                {
                    group.Theme = theme;

                    foreach (Segment snippet in group.Segments)
                    {
                        snippet.Group = group;
                        snippet.ThemeId = theme.Id;
                    }
                }
            }
        }

        public int GetNextFreeThemeId(int idToStartSearchFrom)
        {
            int newId = idToStartSearchFrom;
            if (newId <= 1)
                newId = 1;

            while (GetThemeById(newId) != null)
            {
                newId++;
            }

            return newId;
        }

        public bool CheckIfThemeIdIsInUse(int themeId)
        {
            foreach (Theme theme in Themes)
            {
                if (theme.Id == themeId)
                {
                    return true;
                }
            }
            return false;
        }


        public List<Segment> GetSnippetsById(int id)
        {
            List<Segment> resultSnippets = new List<Segment>();
            HashSet<Segment> allSnippets = GetSegmentsOfAllThemes();

            foreach (Segment tmpSnippet in allSnippets)
            {
                if (tmpSnippet.Id == id)
                    resultSnippets.Add(tmpSnippet);
            }

            return resultSnippets;
        }


        // iterates through all entities and assigns a unique running id to each one.
        // Used to compare cloned items.
        /*
        public void AssignUniqueInteralIdForAllEntities()
        {
            int internalId = 0;

            foreach (Theme theme in this.Themes)
            {
                theme.InternalId = internalId;
                internalId++;

                foreach (Group group in theme.Groups)                
                {
                    group.InternalId = internalId;
                    internalId++;

                    List<Snippet> groupSnippets = group.Snippets;
                    foreach (Snippet snippet in groupSnippets)
                    {
                        snippet.InternalId = internalId;
                        internalId++;
                    }
                }
            }
        }
        */

        /*
        public PsaiMusicEntity GetPsaiMusicEntityByInternalId(int internalId)
        {
            foreach (Theme theme in this.Themes)
            {

                if (theme.InternalId == internalId)
                    return theme;

                foreach (Group group in theme.Groups)
                {
                    if (group.InternalId == internalId)
                        return group;

                    List<Snippet> groupSnippets = group.Snippets;
                    foreach (Snippet snippet in groupSnippets)
                    {
                        if (snippet.InternalId == internalId)
                            return snippet;
                    }
                }
            }

            return null;
        }
        */


        public object Clone()
        {
            PsaiProject clone = new PsaiProject();
            clone.Properties = (ProjectProperties)this.Properties.Clone();

            //AssignUniqueInteralIdForAllEntities();

            // now we copy all the entities from the tmpTheme to this instance,
            // since the GUI holds a reference to 'this' model.
            clone.Themes.Clear();
            foreach (Theme theme in this.Themes)
            {
                Theme themeClone = (Theme)theme.Clone();
                clone.AddPsaiMusicEntity(themeClone);
            }

            // now that we are sure that each theme with group exists, add the manually blocked and linked
            // entities

            HashSet<Segment> allSnippetsSource = this.GetSegmentsOfAllThemes();
            HashSet<Segment> allSnippetsTarget = clone.GetSegmentsOfAllThemes();

            // create Dictionary that maps all source- to all target themes
            Dictionary<Theme, Theme> themeMap = new Dictionary<Theme, Theme>();
            List<Theme>.Enumerator enumThemeSource = this.Themes.GetEnumerator();
            List<Theme>.Enumerator enumThemeTarget = clone.Themes.GetEnumerator();
            while (enumThemeSource.MoveNext())
            {
                enumThemeTarget.MoveNext();
                themeMap.Add(enumThemeSource.Current, enumThemeTarget.Current);
            }

            // create a Dictionary that maps all source- to  all target snippets
            Dictionary<Segment, Segment> snippetMap = new Dictionary<Segment, Segment>();
            HashSet<Segment>.Enumerator enumSnippetsSource = allSnippetsSource.GetEnumerator();
            HashSet<Segment>.Enumerator enumSnippetsTarget = allSnippetsTarget.GetEnumerator();
            while (enumSnippetsSource.MoveNext())
            {
                enumSnippetsTarget.MoveNext();
                snippetMap.Add(enumSnippetsSource.Current, enumSnippetsTarget.Current);
            }

            Dictionary<Group, Group> groupMap = new Dictionary<Group, Group>();

            foreach (Theme sourceTheme in themeMap.Keys)
            {
                Theme targetTheme = themeMap[sourceTheme];

                // add manually blocked themes
                foreach (Theme manuallyBlockedTheme in sourceTheme.ManuallyBlockedTargetThemes)
                {
                    if (themeMap.Keys.Contains(manuallyBlockedTheme))
                        targetTheme.ManuallyBlockedTargetThemes.Add(themeMap[manuallyBlockedTheme]);
                }

                // fill groupMap
                for (int groupIndex = 0; groupIndex < sourceTheme.Groups.Count; groupIndex++)
                {
                    Group sourceGroup = sourceTheme.Groups[groupIndex];
                    Group targetGroup = targetTheme.Groups[groupIndex];

                    groupMap.Add(sourceGroup, targetGroup);
                }
            }


            // update manually linked and blocked group lists of all groups
            foreach (Group group in groupMap.Keys)
            {
                foreach (Group manuallyLinkedGroup in group.ManuallyLinkedGroups)
                {
                    // deleted groups will not appear in the groupMap, therefore we need to test. // TODO: deleting them before saving would be cleaner
                    if (groupMap.Keys.Contains(manuallyLinkedGroup))
                    {
                        groupMap[group].ManuallyLinkedGroups.Add(groupMap[manuallyLinkedGroup]);
                    }
                }
                foreach (Group manuallyBlockedGroup in group.ManuallyBlockedGroups)
                {
                    if (groupMap.Keys.Contains(manuallyBlockedGroup))
                    {
                        groupMap[group].ManuallyBlockedGroups.Add(groupMap[manuallyBlockedGroup]);
                    }
                }

                foreach (Segment bridgeSnippet in group.ManualBridgeSnippetsOfTargetGroups)
                {
                    if (snippetMap.Keys.Contains(bridgeSnippet))
                    {
                        Segment cloneBridgeSnippet = snippetMap[bridgeSnippet];
                        Group cloneGroup = groupMap[group];
                        HashSet<Segment> cloneBridgeSnippets = cloneGroup.ManualBridgeSnippetsOfTargetGroups;

                        cloneBridgeSnippets.Add(cloneBridgeSnippet);
                    }
                }
            }

            //
            foreach (Segment snippet in allSnippetsSource)
            {
                foreach (Segment blockedSnippet in snippet.ManuallyBlockedSnippets)
                {
                    // deleted snippets will not appear in the snippetMap, so we need to test   // TODO: deleting them before saving would be cleaner
                    if (snippetMap.Keys.Contains(blockedSnippet))
                        snippetMap[snippet].ManuallyBlockedSnippets.Add(snippetMap[blockedSnippet]);
                }

                foreach (Segment linkedSnippet in snippet.ManuallyLinkedSnippets)
                {
                    if (snippetMap.Keys.Contains(linkedSnippet))
                        snippetMap[snippet].ManuallyLinkedSnippets.Add(snippetMap[linkedSnippet]);
                }
            }

            return clone;
        }

    }



    [Serializable]
    public class ProjectProperties : ICloneable
    {
        private float _volumeBoost = 0;
        private int _exportSoundQualityInPercent = 100;     // 100 is maximum quality

        public int WarningThresholdPreBeatMillis
        {
            get;
            set;
        }
        
        public bool DefaultCalculatePostAndPrebeatLengthBasedOnBeats
        {
            get;
            set;
        }
        
        public int DefaultSegmentSuitabilites
        {
            get;
            set;
        }

        public ProjectProperties()
        {
            DefaultBpm = 100.0f;
            DefaultPostbeats = 4.0f;
            DefaultPrebeats = 1.0f;
            WarningThresholdPreBeatMillis = 1500;
            DefaultSegmentSuitabilites = (int)psai.net.SegmentSuitability.start | (int)psai.net.SegmentSuitability.middle;
            ForceFullRebuild = true;
        }

        public bool ForceFullRebuild
        {
            get;
            set;
        }

        public float VolumeBoost
        {
            get { return _volumeBoost; }
            set
            {
                if (value >= 0 && value <= 600)
                {
                    _volumeBoost = value;
                }
                else
                {
                    Console.Out.WriteLine("invalid value for VolumeBoost");
                }
            }
        }

        public int ExportSoundQualityInPercent
        {
            get { return _exportSoundQualityInPercent; }
            set
            {
                if (value >= 1 && value <= 100)
                    _exportSoundQualityInPercent = value;
            }
        }

        public float DefaultPrebeats
        {
            get;
            set;
        }

        public float DefaultPostbeats
        {
            get;
            set;
        }

        public float DefaultBpm
        {
            get;
            set;
        }

        public int DefaultPrebeatLengthInSamples
        {
            get;
            set;
        }

        public int DefaultPostbeatLengthInSamples
        {
            get;
            set;
        }

        public ProjectProperties ShallowCopy()
        {
            return (ProjectProperties)this.MemberwiseClone();
        }


        #region ICloneable Members
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        #endregion

    }






}
