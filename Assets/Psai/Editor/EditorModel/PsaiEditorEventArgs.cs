using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace psai.Editor
{
    public class EventArgs_BridgeSegmentToggled : EventArgs
    {
        private Segment _segment;
        private Group _sourceGroup;

        public Segment Segment
        {
            get { return _segment; }
        }

        public Group SourceGroup
        {
            get { return _sourceGroup; }
        }

        public EventArgs_BridgeSegmentToggled(Segment snippet, Group sourceGroup)
        {
            _segment = snippet;
            _sourceGroup = sourceGroup;
        }
    }

    public class EventArgs_CompatibilitySettingChanged : EventArgs
    {
        private PsaiMusicEntity _sourceEntity;
        private PsaiMusicEntity _targetEntity;

        public PsaiMusicEntity SourceEntity
        {
            get { return _sourceEntity; }
        }

        public PsaiMusicEntity TargetEntity
        {
            get { return _targetEntity; }
        }

        public EventArgs_CompatibilitySettingChanged(PsaiMusicEntity sourceEntity, PsaiMusicEntity targetEntity)
        {
            _sourceEntity = sourceEntity;
            _targetEntity = targetEntity;
        }

    }

    public class EventArgs_PsaiEntityDeleted : EventArgs
    {
        private PsaiMusicEntity _deletedEntity;

        internal PsaiMusicEntity DeletedEntity
        {
            get { return _deletedEntity; }
        }

        internal EventArgs_PsaiEntityDeleted(PsaiMusicEntity deletedEntity)
        {
            _deletedEntity = deletedEntity;
        }
    }

    public enum MessageType
    {
        normal,
        warning,
        error,
        critical
    }

    public class EventArgs_NewMessageToGui : EventArgs
    {
        public EventArgs_NewMessageToGui(string message, MessageType type)
        {
            Message = message;
            MessageType = type;
        }

        public string Message
        {
            get;
            set;
        }

        public MessageType MessageType
        {
            get;
            set;
        }
    }


    public class EventArgs_ProgressBarStart : EventArgs
    {
        public EventArgs_ProgressBarStart(string description)
        {
            Description = description;
        }

        internal string Description
        {
            get;
            set;
        }
    }

    public class EventArgs_ProgressBarUpdate : EventArgs
    {
        // a value between 0.0 and 1.0
        internal EventArgs_ProgressBarUpdate(int totalProgressInPercent)
        {
            TotalProgressInPercent = totalProgressInPercent;
        }

        internal int TotalProgressInPercent
        {
            get;
            set;
        }
    }


    public struct ArgsCopyFilesToTargetDir
    {
        public List<Theme> sourceThemes;
        public string sourceDirectory;  // the full path to the base directory of the source Themes (e.g. the source Project Directory)
        public string targetDirectory;
    }


    public class EventArgs_AuditOrExportResult : EventArgs
    {
        internal Dictionary<PsaiMusicEntity, string> entitiesThatCausedErrorInLastAudit = new Dictionary<PsaiMusicEntity, string>();
        internal Dictionary<PsaiMusicEntity, string> entitiesThatCausedWarningInLastAudit = new Dictionary<PsaiMusicEntity, string>();

        internal EventArgs_AuditOrExportResult()
        {
        }


        public bool Canceled
        {
            get;
            set;
        }

        public bool WarningOccurred
        {
            get;
            set;
        }

        public bool ErrorOccurred
        {
            get;
            set;
        }


        public string ResultMessage
        {
            get;
            set;
        }

        public string Caption
        {
            get;
            set;
        }

        public string PsaiCoreBinaryFilename
        {
            get;
            set;
        }

        /*
        public bool ExportStillNeedsToWriteUnityMetaFiles
        {
            get;
            set;
        }
        */

        public int NumberOfAudioFilesCopiedToUnityProject
        {
            get;
            set;
        }
    }


    public class EventArgs_PsaiEntityAdded : EventArgs
    {
        private PsaiMusicEntity _entity;

        public PsaiMusicEntity Entity
        {
            get { return _entity; }
        }

        public EventArgs_PsaiEntityAdded(PsaiMusicEntity entity)
        {
            _entity = entity;
        }
    }

    public class EventArgs_PsaiEntityPropertiesChanged : EventArgs
    {
        PsaiMusicEntity _entity;
        private bool _changeAffectsCompatibilities = false;

        public PsaiMusicEntity Entity
        {
            get { return _entity; }
        }

        public string DescriptionOfChange
        {
            get;
            set;
        }

        public bool ChangeAffectsCompatibilities
        {
            get { return _changeAffectsCompatibilities; }
            private set { _changeAffectsCompatibilities = value; }
        }

        public EventArgs_PsaiEntityPropertiesChanged(PsaiMusicEntity entity, bool changeAffectsCompatibilities)
        {
            _entity = entity;
            ChangeAffectsCompatibilities = changeAffectsCompatibilities;
        }

        public override string ToString()
        {
            return DescriptionOfChange;
        }
    }


    public class EventArgs_OperationFailed : EventArgs
    {

        public string ErrorMessage
        {
            get;
            private set;
        }

        public Exception Exception
        {
            get;
            private set;
        }

        public EventArgs_OperationFailed(string errorMessage, Exception exception)
        {
            this.ErrorMessage = errorMessage;
            this.Exception = exception;
        }
    }

    public class EventArgs_SegmentMovedToGroup : EventArgs
    {
        public List<Group> AffectedGroups
        {
            get;
            set;
        }

        public int MovedSegmentsCount
        {
            get;
            set;
        }

        public Segment FirstSegmentMoved
        {
            get;
            private set;
        }


        public EventArgs_SegmentMovedToGroup(List<Group> affectedGroups, Segment[] movedSegments)
        {
            AffectedGroups = affectedGroups;
            MovedSegmentsCount = movedSegments.Length;
            FirstSegmentMoved = movedSegments[0];
        }
    }
}
