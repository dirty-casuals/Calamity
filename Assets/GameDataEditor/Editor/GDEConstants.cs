using System;

namespace GameDataEditor
{
    public class GDEConstants {
        #region Header Strings
        public const string GameDataHeader = "GDE Item Editor";
        public const string DefineDataHeader = "GDE Schema Editor";
        public const string CreateNewItemHeader = "Create a New Item";
        public const string ItemListHeader = "Item List";
        public const string SearchHeader = "Search";
        public const string CreateNewSchemaHeader = "Create a New Schema";
        public const string SchemaListHeader = "Schema List";
        public const string NewFieldHeader = "Add a new field";
		public const string PreferencesHeader = "Game Data Editor";
        #endregion

        #region Button Strings
        public const string SaveBtn = "Save";
        public const string SaveNeededBtn = "Save Needed";
        public const string LoadBtn = "Load";
        public const string ClearSearchBtn = "Clear Search";
        public const string CreateNewItemBtn = "Create New Item";
        public const string DeleteBtn = "Delete";
        public const string ResizeBtn = "Resize";
        public const string AddFieldBtn = "Add Field";
        public const string AddCustomFieldBtn = "Add Custom Field";
        public const string CreateNewSchemaBtn = "Create New Schema";
        public const string RenameBtn = "Rename";
        public const string CancelBtn = "Cancel";
        public const string DeleteSchemaBtn = "Delete Schema";
		public const string BrowseBtn = "Browse ...";
		public const string BackBtn = "Back";
		public const string ImportBtn = "Import";
		public const string DownloadBtn = "Download";
		public const string ImportAgainBtn = "Import Again";
		public const string CloseBtn = "Close";
		public const string CloneItem = "Clone Item";
		public const string CloneSchema = "Clone Schema";
		#endregion

        #region Label Strings
        public const string FilterBySchemaLbl = "Show Items Containing Schema:";
        public const string SchemaLbl = "Schema:";
        public const string ItemNameLbl = "Item Name:";
        public const string ExpandAllLbl = "Expand All";
        public const string CollapseAllLbl = "Collapse All";
        public const string ValueLbl = "Value:";
        public const string ValuesLbl = "Values:";
        public const string SizeLbl = "Size:";
        public const string SchemaNameLbl = "Schema Name:";
        public const string DefaultValueLbl = "Default Value:";
        public const string DefaultValuesLbl = "Default Values:";
        public const string DefaultSizeLbl = "Default Size:";
        public const string BasicFieldTypeLbl = "Basic Field Type:";
        public const string CustomFieldTypeLbl = "Custom Field Type:";
        public const string FieldNameLbl = "Field Name:";
        public const string IsListLbl = "Is List:";
		public const string Is2DListLbl = "Is 2D List:";
		public const string EncryptionTitle = "Encryption Complete!";
		public const string EncryptionWarning = "Be sure to move your plain text gde data file OUT of the resources folder!\n\nPlain Text GDE Data File Path:";
		public const string EncryptionFileLabel = "Encrypted GDE Data File Path:";
		public const string GeneratingLbl = "Generating:";
		public const string DoneGeneratingLbl = "Done Generating:";
		public const string ChooseImportLbl = "Choose Import Method";
		public const string ImportLocalLbl = "Import Local File";
		public const string ImportGoogleSSLbl = "Import Google SpreadSheet";
		public const string ReauthWithGoogleLbl = "Reauthenticate With Google";
		public const string ImportWBLbl = "Import Excel Workbook";
		public const string ExcelFileLbl = "Excel File (.xlsx or .xls)";
		public const string ExcelFileExportLbl = "Excel File (.xlsx)";
		public const string OpenWBLbl = "Open Workbook";
		public const string AuthWithGoogleLbl = "Authenticate With Google";
		public const string AuthInstruction1_1 = "1) Make sure you are logged in to the";
		public const string AuthInstruction1_2 = "correct google account in your browser.";
		public const string AuthInstruction2_1 = "2) Authorize access to your Google Sheet.";
		public const string AuthInstruction2_2 = "Enter the code specified after you accept.";
		public const string GotoAuthURL = "Go to Google Authenticate URL";
		public const string EnterAccessCodeLbl = "Enter Access Code from Google:";
		public const string SetCodeLbl = "Set Code";
		public const string DownloadGoogleSheetLbl = "Download Google Sheet";
		public const string SelectSheetLbl = "Select Spreadsheet to Import:";
		public const string ImportCompleteLbl = "Import Complete!";
		public const string ImportMsg1 = "Your import is complete. Close this window or";
		public const string ImportMsg2 = "select \"Import Again\" to import a different spreadsheet.";
		public const string ImportingScehemasLbl = "Importing Schemas...";
		public const string ImportingGameDataLbl = "Importing Game Data";
		public const string ImportingItemsLbl = "Importing Items...";
		public const string ClearedAuthMsg = "Google OAuth Tokens Cleared";
		public const string ItemLbl = "item";
		public const string ItemsLbl = "items";
		public const string SearchResultFormat = "{0} of {1} {2} displayed";
		public const string _AllLbl = "_All";
		public const string DeleteWarningFormat = "{0} {1} will be deleted!";
		public const string FileSettingsLbl = "File Settings";
		public const string CreateDataFileLbl = "Data File";
		public const string FileDoesNotExistWarning = "Data file not found.";
		public const string PrettyJsonLbl = "Pretty Formatted Json";
		public const string PrettyJsonMsg = "Formatted json is easier to for you to read, but results in larger file size.";
		public const string CreateFileLbl = "Create Data File";
		public const string OpenDataFileLbl = "Open Data File";
		public const string ColorsLbl = "Colors";
		public const string CreateDataColorLbl = "Item Editor Headers";
		public const string DefineDataColorLbl = "Schema Editor Headers";
		public const string HighlightLbl = "Highlight";
		public const string UseDefaults = "Use Defaults";
		public const string ApplyLbl = "Apply";
		public const string PreferencesLbl = "Game Data Editor Preferences";
		public const string PreferencesMenu = "Preferences";
		public const string DefineDataMenu = "Schema Editor";
		public const string CreateDataMenu = "Item Editor";
		public const string EncryptMenu = "Encrypt Data";
		public const string ClearExcelMenu = "Clear Import\xA0& Export Settings";
		public const string EncryptingMsg = "Encrypting...";
		public const string DoneLbl = "Done";
		public const string EncryptionCompleteLbl = "GDE Encryption Complete!";
		public const string ImportSpreadsheetMenu = "Import Spreadsheet";
		public const string GenerateExtensionsMenu = "Generate GDE Data Classes";
		public const string ForumMenu = "GDE Forum";
		public const string DocsMenu = "GDE Documentation";
		public const string RateMenu = "Rate GDE";
		public const string EmailMenu = "Email";
		public const string ContactMenu = "Contact";
		public const string LoadDataMenu = "Load Data";
		public const string LoadAndGenMenu = "Load Data\xA0& Generate Data Classes";
		public const string ExportDlgTitleLbl = "Choose Export Method";
		public const string ExportToLocalFileBtn = "Export to Local File";
		public const string ExportToSheetsBtn = "Update existing Google SpreadSheet";
		public const string ExportToNewSheetBtn = "Upload new Google Spreadsheet";
		public const string ExportBtn = "Export";
		public const string ExportAgainBtn = "Export Again";
		public const string ExportSpreadsheetLbl = "Export Spreadsheet";
		public const string ExportCompleteLbl = "Export Complete!";
		public const string ExportMsg1 = "Your export is complete. Close this window or";
		public const string ExportMsg2 =  "select \""+ ExportAgainBtn + "\" to export to a different spreadsheet.";
		public const string ExportExcelWorkbookLbl= "Export Excel Workbook";
		public const string ExportGoogleSheetLbl = "Update Google Sheet";
		public const string SelectExportSpreadSheetLbl = "Select Spreadsheet to Export:";
		public const string UploadBtn = "Upload";
		public const string ExportNewGoogleSheetLbl = "Upload New Google Sheet";
		public const string NewSheetFileNameLbl = "Enter filename for new sheet:";
		public const string SpreadsheetSectionHeader = "Spreadsheet\nActions";
		public const string PreviewLbl = "Preview";
		public const string ImportSource = "Source{0}:";
		public const string ExportDest = "Destination{0}:";
		public const string GoogleDrive = " (Google Drive)";
		#endregion

		#region Draw Size Keys
		public const string SizeExportDlgTitleLblKey = "1" + KeySuffix;
		public const string SizeExportToLocalFileBtnKey = "2" + KeySuffix;
		public const string SizeExportToSheetsBtnKey = "3" + KeySuffix;
		public const string SizeExportToNewSheetBtnKey = "4" + KeySuffix;
		public const string SizeExportBtnKey = "5" + KeySuffix;
		public const string SizeExportAgainBtnKey = "6" + KeySuffix;
		public const string SizeExportSpreadsheetLblKey = "7" + KeySuffix;
		public const string SizeExportCompleteLblKey = "8" + KeySuffix;
		public const string SizeExportMsg1Key = "9" + KeySuffix;
		public const string SizeExportMsg2Key = "10" + KeySuffix;
		public const string SizeReauthWithGoogleLblKey = "11" + KeySuffix;
		public const string SizeExportExcelWorkbookLblKey = "12" + KeySuffix;
		public const string SizeExcelFileLblKey = "13" + KeySuffix;
		public const string SizeBrowseBtnKey = "14" + KeySuffix;
		public const string SizeBackBtnKey = "15" + KeySuffix;
		public const string SizeAuthWithGoogleLblKey = "16" + KeySuffix;
		public const string SizeAuthInstruction1_1Key = "17" + KeySuffix;
		public const string SizeAuthInstruction1_2Key = "18" + KeySuffix;
		public const string SizeAuthInstruction2_1Key = "19" + KeySuffix;
		public const string SizeAuthInstruction2_2Key = "20" + KeySuffix;
		public const string SizeGotoAuthURLKey = "21" + KeySuffix;
		public const string SizeEnterAccessCodeLblKey = "22" + KeySuffix;
		public const string SizeSetCodeLblKey = "23" + KeySuffix;
		public const string SizeExportGoogleSheetLblKey = "24" + KeySuffix;
		public const string SizeSelectExportSpreadSheetLblKey = "25" + KeySuffix;
		public const string SizeUploadBtnKey = "26" + KeySuffix;
		public const string SizeExportNewGoogleSheetLblKey = "27" + KeySuffix;
		public const string SizeNewSheetFileNameLblKey = "28" + KeySuffix;
		public const string SizeForumLinkTextKey = "29" + KeySuffix;
		public const string SizeCloseBtnKey = "30" + KeySuffix;
		public const string SizeEditorHeaderKey = "31" + KeySuffix;
		public const string SizeFileLblKey = "32" + KeySuffix;
		public const string SizeSpreadsheetSectionHeaderKey = "33" + KeySuffix;
		public const string SizeListSubHeaderKey = "34" + KeySuffix;
		public const string SizeCreateSubHeaderKey = "35" + KeySuffix;
		public const string SizeCreateNewSchemaBtnKey = "36" + KeySuffix;
		public const string SizeRateMeTextKey = "37" + KeySuffix;
		public const string SizeNewFieldHeaderKey = "38" + KeySuffix;
		public const string SizeIsListLblKey = "39" + KeySuffix;
		public const string SizeIs2DListLblKey = "40" + KeySuffix;
		public const string SizeAddFieldBtnKey = "41" + KeySuffix;
		public const string SizeAddCustomFieldBtnKey = "42" + KeySuffix;
		public const string SizeDeleteBtnKey = "43" + KeySuffix;
		public const string SizeDefaultSizeLblKey = "44" + KeySuffix;
		public const string SizeResizeBtnKey = "45" + KeySuffix;
		public const string SizeRenameBtnKey = "46" + KeySuffix;
		public const string SizeCancelBtnKey = "47" + KeySuffix;
		public const string SizeSearchHeaderKey = "48" + KeySuffix;
		public const string SizeClearSearchBtnKey = "49" + KeySuffix;
		public const string SizeCreateNewItemBtnKey = "50" + KeySuffix;
		public const string SizeFilterBySchemaLblKey = "51" + KeySuffix;
		public const string SizeSizeLblKey = "52" + KeySuffix;
		public const string SizeValueLblKey = "53" + KeySuffix;
		public const string SizeDefaultValueLblKey = "54" + KeySuffix;
		public const string SizeExpandAllLblKey = "55" + KeySuffix;
		public const string SizeCollapseAllLblKey = "56" + KeySuffix;
		public const string SizeCloneItemKey = "57" + KeySuffix;
		public const string SizeCloneSchemaKey = "58" + KeySuffix;
		public const string SizePreviewLblKey = "59" + KeySuffix;
		public const string SizePreviewAudioLblKey = "60" + KeySuffix;
		public const string SizeClearSearchEmptyKey = "61" + KeySuffix;
		public const string SizeImportBtnKey = "62" + KeySuffix;
		public const string SizeImportSourceKey = "63" + KeySuffix;
		public const string SizeExportDestKey = "64" + KeySuffix;
		public const string SizeGoogleDriveKey = "65" + KeySuffix;
		public const string SizeExcelFileExportLblKey = "66" + KeySuffix;
		#endregion

        #region Error Strings
        public const string ErrorLbl = "Error!";
        public const string OkLbl = "Ok";
        public const string ErrorCreatingItem = "Error creating item!";
		public const string ErrorCloningItem = "Error cloning item!";
        public const string NoOrInvalidSchema = "No schema or invalid schema selected.";
        public const string SchemaNotFound = "Schema data not found";
        public const string InvalidCustomFieldType = "Invalid custom field type selected.";
        public const string ErrorCreatingField = "Error creating field!";
        public const string ErrorCreatingSchema = "Error creating Schema!";
		public const string ErrorCloningSchema = "Error cloning Schema!";
        public const string SureDeleteSchema = "Are you sure you want to delete this schema?";
        public const string DirectoryNotFound = "Could not find part of the path: {0}";
		public const string ErrorLoadingSpreadsheet = "Error loading spreadsheet. Only text formatted cells are supported!";
		public const string ErrorParsingCellFormat = "Error in Sheet: {0}. Error parsing cell: {1}. Using the default value for that type.";
		public const string CouldNotFindFieldNameRow = "Could not find Field Names row for sheet:";
		public const string CouldNotFindFieldTypeRow = "Could not find Field Types row for sheet:";
		public const string ErrorInSheet = "Error in Sheet {0}: {1} (Cell: {2})";
		public const string LexerExceptionFormat = "Unrecognized symbol '{0}' at index {1} (line {2}, column {3}).";
		public const string FailedToReadScehmaData = "Failed to read schema data:";
		public const string FailedToReadItemData = "Failed to read item data:";
		public const string FieldNameExists = "Field name already exists:";
		public const string SchemaNameExists = "Schema name already exists:";
		public const string SchemaNameInvalid = "Schema name is invalid:";
		public const string ErrorReadingSchema = "Error reading schema data:";
		public const string FieldNameInvalid = "Field name is invalid:";
		public const string ItemNameInvalid = "Item name is invalid:";
		public const string ItemNameExists = "Item name already exists:";
		public const string CouldNotRenameFormat = "Couldn't rename {0} to {1}: {2}";
		public const string ErrorDownloadingSheet = "Error downloading spreadsheet: ";
		public const string ErrorParsingJson = "Error parsing data file: {0}";
        #endregion

		#region Window Constants
		public const float MinLabelWidth = 200f;
		public const int Indent = 20;
		public const float LineHeight = 20f;
		public const float TopBuffer = 2f;
		public const float LeftBuffer = 2f;
		public const float RightBuffer = 2f;
		public const float VectorFieldBuffer = 0.75f;
		public const float MinTextAreaWidth = 100f;
		public const float MinTextAreaHeight = LineHeight;
		public const double DoubleClickTime = 0.5;
		public const double AutoSaveTime = 30;
		public const float PreferencesMinWidth = 640f;
		public const float PreferencesMinHeight = 280f;
		public const string LblSuffix = "_lbl";
		public const string TypeSuffix = "_typ";
		public const string KeySuffix = "_gde_meta_key";
		public const float NewHighlightTimeout = 30f;
		public const string HighlightColor = "#f15c25";
		public const string NewHighlightStart = "#FF6127";
		public const string NewHighlightEnd = "#7F3114";
		public const float NewHighlightRate = 0.01f;
		public const float NewHighlightDuration = 8f;
		public const float NewHightlightPeriod = 1f;
		#endregion

		#region Default Preference Settings
		public const string CreateDataColor = "#013859";
		public const string CreateDataColorPro = "#36ccdb";
		public const string DefineDataColor = "#185e65";
		public const string DefineDataColorPro = "#0488d7";
		public const string DefaultDataFilePath =  "Resources";
		public const string RootDir = "GameDataEditor";
		public const string SettingsPath = "Editor/gde_editor_settings.bytes";
		public const string DataFile = "gde_data.txt";
		#endregion

		#region Link Strings
		public const string RateMeText = "Click To Rate!";
		public const string ForumLinkText = "Suggest Features in the Forum";
		public const string RateMeURL = "http://u3d.as/7YN";
		public const string ForumURL = "http://forum.unity3d.com/threads/game-data-editor-the-visual-data-editor-released.250379/";
		public const string DocURL = "http://gamedataeditor.com/docs/gde-quickstart.html";
		public const string MailTo = "mailto:celeste%40stayathomedevs.com?subject=Question%20about%20GDE&cc=steve%40stayathomedevs.com";
		public const string Twitter = "https://twitter.com/celestipoo";
		public const string BorderTexturePath = "Editor/Textures/boarder.png";
		public const string WarningIconTexturePath = "Editor/Textures/warning.png";
		#endregion

		#region Import Workbook Keys
		public const string WorkbookFilePathKey = "gde_workbookpath";
		#endregion
    }
}
