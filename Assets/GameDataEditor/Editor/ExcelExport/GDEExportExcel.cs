using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace GameDataEditor
{
    public class GDEExportExcel : EditorWindow
    {
        public enum View
        {
            Default,
            LocalFile,
            LaunchAuthURL,
            Authenticate,
            UploadExisting,
            UploadNew,
            ImportComplete,

            None
        }

        GUIStyle headerStyle = null;
        GUIStyle linkStyle = null;
        GUIStyle buttonStyle = null;
        GUIStyle labelStyle = null;
        GUIStyle rateBoxStyle = null;
        GUIStyle comboBoxStyle = null;
        GUIStyle textFieldStyle = null;

        GUIContent _content;
        GUIContent content
        {
            get
            {
                if (_content == null)
                    _content = new GUIContent();
                return _content;
            }
        }

        Vector2 size;

        string spreadsheetPath = "";
        string spreadsheetName = "";

        string accessCode = "";
        int downloadSelectionIndex = 0;

        static View currentView;
        static View nextView;
        static View viewAfterAuth = View.None;

        public static string googleSheetImportName;
        public static int googleSheetImportIndex;

        GDEDrawHelper _drawHelper;
        GDEDrawHelper drawHelper
        {
            get
            {
                if (_drawHelper == null)
                    _drawHelper = new GDEDrawHelper(this, 2f, 10f, 10f, 10f, 20f);

                return _drawHelper;
            }
        }

        void SetStyles()
        {
            if (headerStyle.IsNullOrEmpty())
            {
                headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.fontSize = 16;
            }

            if (buttonStyle.IsNullOrEmpty())
            {
                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fontSize = 14;
                buttonStyle.padding = new RectOffset(15, 15, 5, 5);
            }

            if (labelStyle.IsNullOrEmpty())
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = 14;
                labelStyle.padding = new RectOffset(0, 0, 3, 3);
            }

            if (linkStyle.IsNullOrEmpty())
            {
                linkStyle = new GUIStyle(GUI.skin.label);
                linkStyle.fontSize = 16;
                linkStyle.alignment = TextAnchor.MiddleCenter;
                linkStyle.normal.textColor = Color.blue;
            }

            if (rateBoxStyle.IsNullOrEmpty())
            {
                rateBoxStyle = new GUIStyle(GUI.skin.box);
                rateBoxStyle.normal.background = (Texture2D)AssetDatabase.LoadAssetAtPath(GDESettings.RelativeRootDir + Path.DirectorySeparatorChar +
					GDEConstants.BorderTexturePath, typeof(Texture2D));
                rateBoxStyle.border = new RectOffset(2, 2, 2, 2);
            }

            if (comboBoxStyle.IsNullOrEmpty())
            {
                comboBoxStyle = new GUIStyle(EditorStyles.popup);
                comboBoxStyle.fontSize = 14;
                comboBoxStyle.fixedHeight = 24f;
            }

            if (textFieldStyle.IsNullOrEmpty())
            {
                textFieldStyle = new GUIStyle(GUI.skin.textField);
                textFieldStyle.fontSize = 14;
                textFieldStyle.fixedHeight = 22f;
                textFieldStyle.alignment = TextAnchor.UpperLeft;
                textFieldStyle.margin = new RectOffset(0, 0, 0, 8);
            }
        }

        public void LoadSettings(View view = View.Default)
        {
            spreadsheetPath = GDESettings.Instance.ExportedGoogleSpreadsheetPath;

            currentView = view;
            nextView = view;

            minSize = new Vector2(420, 250);
            maxSize = minSize;
        }

        void OnGUI()
        {
            SetStyles();

            size = Vector2.zero;

            drawHelper.ResetToTop();

            if (currentView.Equals(View.Default))
                DrawDefaultView();
            else if (currentView.Equals(View.LaunchAuthURL))
                DrawLaunchAuthURL();
            else if (currentView.Equals(View.Authenticate))
                DrawAuthenticateView();
            else if (currentView.Equals(View.UploadExisting))
                DrawUploadExistingView();
            else if (currentView.Equals(View.UploadNew))
                DrawUploadNewView();
            else if (currentView.Equals(View.LocalFile))
                DrawExportLocalFile();
            else if (currentView.Equals(View.ImportComplete))
                DrawImportCompleteView();

            currentView = nextView;
        }

        void DrawDefaultView()
        {
            content.text = GDEConstants.ExportDlgTitleLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExportDlgTitleLblKey, content, headerStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, headerStyle);

            drawHelper.NewLine(2);

            content.text = GDEConstants.ExportToLocalFileBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExportToLocalFileBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, buttonStyle))
                nextView = View.LocalFile;

            drawHelper.NewLine(2.5f);

            content.text = GDEConstants.ExportToSheetsBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExportToSheetsBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, buttonStyle))
            {
                if (HasAuthenticated())
                {
                    nextView = View.UploadExisting;
                    GDEDriveHelper.Instance.GetSpreadsheetList();
                }
                else
                {
                    nextView = View.LaunchAuthURL;
                    viewAfterAuth = View.UploadExisting;
                }
            }

            drawHelper.NewLine(2.5f);

            content.text = GDEConstants.ExportToNewSheetBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExportToNewSheetBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, buttonStyle))
            {
                if (HasAuthenticated())
                {
                    nextView = View.UploadNew;
                }
                else
                {
                    nextView = View.LaunchAuthURL;
                    viewAfterAuth = View.UploadNew;
                }
            }

            drawHelper.NewLine(2.5f);

            content.text = GDEConstants.ReauthWithGoogleLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeReauthWithGoogleLblKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, buttonStyle))
            {
                nextView = View.LaunchAuthURL;
            }
        }

       void DrawExportLocalFile()
        {
            content.text = GDEConstants.ExportExcelWorkbookLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExportExcelWorkbookLblKey, content, headerStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, headerStyle);

            drawHelper.NewLine(2);

            content.text = GDEConstants.ExcelFileExportLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExcelFileExportLblKey, content, labelStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);
            drawHelper.CurrentLinePosition += size.x + 2f;
            drawHelper.NewLine();

            spreadsheetPath = EditorGUI.TextField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), drawHelper.FullSeparatorWidth(),
                                                           textFieldStyle.fixedHeight), spreadsheetPath, textFieldStyle);
            drawHelper.CurrentLinePosition += size.x + 2f;
            drawHelper.NewLine(1.1f);

            content.text = GDEConstants.BrowseBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeBrowseBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, buttonStyle))
            {
                string newSpreadSheetPath = EditorUtility.OpenFilePanel(GDEConstants.OpenWBLbl, spreadsheetPath, string.Empty);
                if (!string.IsNullOrEmpty(newSpreadSheetPath) && !newSpreadSheetPath.Equals(spreadsheetPath))
                    spreadsheetPath = newSpreadSheetPath;
                GUI.FocusControl(string.Empty);
            }

            // Draw Back & Export Buttons
            content.text = GDEConstants.BackBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeBackBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, position.height-size.y-drawHelper.BottomBuffer, size.x, size.y), content, buttonStyle))
                nextView = View.Default;

            content.text = GDEConstants.ExportBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExportBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(position.width-size.x-drawHelper.LeftBuffer, position.height-size.y-drawHelper.BottomBuffer, size.x, size.y), content, buttonStyle))
            {
                GDESettings settings = GDESettings.Instance;
                settings.ExportedLocalSpreadsheetName = spreadsheetPath;
				settings.ExportType = ImportExportType.Local;
                settings.Save();

				GDEExcelManager.DoExport();
				nextView = View.ImportComplete;
            }
        }

        void DrawLaunchAuthURL()
        {
            content.text = GDEConstants.AuthWithGoogleLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeAuthWithGoogleLblKey, content, headerStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, headerStyle);

            drawHelper.NewLine(2);

            content.text = GDEConstants.AuthInstruction1_1;
            drawHelper.TryGetCachedSize(GDEConstants.SizeAuthInstruction1_1Key, content, labelStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);

            drawHelper.NewLine();

            content.text = GDEConstants.AuthInstruction1_2;
            drawHelper.TryGetCachedSize(GDEConstants.SizeAuthInstruction1_2Key, content, labelStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);

            drawHelper.NewLine(2);

            content.text = GDEConstants.AuthInstruction2_1;
            drawHelper.TryGetCachedSize(GDEConstants.SizeAuthInstruction2_1Key, content, labelStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);

            drawHelper.NewLine();

            content.text = GDEConstants.AuthInstruction2_2;
            drawHelper.TryGetCachedSize(GDEConstants.SizeAuthInstruction2_2Key, content, labelStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);


            // Draw Back & GOTO Auth buttons
            content.text = GDEConstants.BackBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeBackBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, position.height-size.y-drawHelper.BottomBuffer, size.x, size.y), content, buttonStyle))
                nextView = View.Default;

            content.text = GDEConstants.GotoAuthURL;
            drawHelper.TryGetCachedSize(GDEConstants.SizeGotoAuthURLKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(position.width-size.x-drawHelper.LeftBuffer, position.height-size.y-drawHelper.BottomBuffer, size.x, size.y), content, buttonStyle))
            {
                GDEDriveHelper.Instance.RequestAuthFromUser();
                nextView = View.Authenticate;
            }
        }

        void DrawAuthenticateView()
        {
            content.text = GDEConstants.AuthWithGoogleLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeAuthWithGoogleLblKey, content, headerStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, headerStyle);

            drawHelper.NewLine(2);

            content.text = GDEConstants.EnterAccessCodeLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeEnterAccessCodeLblKey, content, labelStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);

            drawHelper.NewLine(1.1f);

            accessCode = EditorGUI.TextField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), drawHelper.FullSeparatorWidth(), size.y), accessCode, textFieldStyle);

            // Draw Back & Set Code Buttons
            content.text = GDEConstants.BackBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeBackBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, position.height-size.y-drawHelper.BottomBuffer, size.x, size.y), content, buttonStyle))
                nextView = View.LaunchAuthURL;

            content.text = GDEConstants.SetCodeLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeSetCodeLblKey, content, buttonStyle, out size);
            if (!string.IsNullOrEmpty(accessCode) && GUI.Button(new Rect(position.width-size.x-drawHelper.LeftBuffer, position.height-size.y-drawHelper.BottomBuffer, size.x, size.y), content, buttonStyle))
            {
                GDEDriveHelper.Instance.SetAccessCode(accessCode);
                GDEDriveHelper.Instance.GetSpreadsheetList();

                if (!viewAfterAuth.Equals(View.None))
                {
                    nextView = viewAfterAuth;
                    viewAfterAuth = View.None;
                }
                else
                {
                    nextView = View.Default;
                }

                GUI.FocusControl(string.Empty);
            }
        }

        void DrawUploadExistingView()
        {
            content.text = GDEConstants.ExportGoogleSheetLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExportGoogleSheetLblKey, content, headerStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, headerStyle);

            drawHelper.NewLine(2);

            content.text = GDEConstants.SelectExportSpreadSheetLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeSelectExportSpreadSheetLblKey, content, labelStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);

            drawHelper.NewLine(1.1f);

            downloadSelectionIndex = EditorGUI.Popup(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), drawHelper.FullSeparatorWidth(), size.y),
                downloadSelectionIndex, GDEDriveHelper.Instance.SpreadSheetNames, comboBoxStyle);

            // Draw Back & Upload Buttons
            content.text = GDEConstants.BackBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeBackBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, position.height-size.y-drawHelper.BottomBuffer, size.x, size.y), content, buttonStyle))
                nextView = View.Default;

            content.text = GDEConstants.UploadBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeUploadBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(position.width-size.x-drawHelper.LeftBuffer, position.height-size.y-drawHelper.BottomBuffer, size.x, size.y), content, buttonStyle))
            {
				GDESettings settings = GDESettings.Instance;
				settings.ExportedGoogleSpreadsheetPath = GDEDriveHelper.Instance.SpreadSheetNames[downloadSelectionIndex];
				settings.ExportType = ImportExportType.Google;
				settings.Save();

				GDEExcelManager.DoExport();
                nextView = View.ImportComplete;
            }
        }

        void DrawUploadNewView()
        {
            content.text = GDEConstants.ExportNewGoogleSheetLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExportToNewSheetBtnKey, content, headerStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, headerStyle);

            drawHelper.NewLine(2);

            content.text = GDEConstants.NewSheetFileNameLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeNewSheetFileNameLblKey, content, labelStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);

            drawHelper.NewLine(1.1f);

            spreadsheetName = EditorGUI.TextField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), drawHelper.FullSeparatorWidth(), size.y),
                spreadsheetName, textFieldStyle);

            // Draw Back & Upload Buttons
            content.text = GDEConstants.BackBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeBackBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, position.height-size.y-drawHelper.BottomBuffer, size.x, size.y), content, buttonStyle))
                nextView = View.Default;

            content.text = GDEConstants.UploadBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeUploadBtnKey, content, buttonStyle, out size);
            if (!string.IsNullOrEmpty(spreadsheetName) &&
                GUI.Button(new Rect(position.width-size.x-drawHelper.LeftBuffer, position.height-size.y-drawHelper.BottomBuffer, size.x, size.y), content, buttonStyle))
            {
				GDESettings settings = GDESettings.Instance;
				settings.ExportedGoogleSpreadsheetPath = spreadsheetName;
				settings.ExportType = ImportExportType.Google;
				settings.Save();

				GDEExcelManager.DoExport(true);
                nextView = View.ImportComplete;
            }
        }

        void DrawImportCompleteView()
        {
            content.text = GDEConstants.ExportCompleteLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExportCompleteLblKey, content, headerStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, headerStyle);

            drawHelper.NewLine(2);

            content.text = GDEConstants.ExportMsg1;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExportMsg1Key, content, labelStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);

            drawHelper.NewLine(1.1f);

            content.text = GDEConstants.ExportMsg2;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExportMsg2Key, content, labelStyle, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CenteredOnLine(size.x), drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);

            // Draw rate box
            float heightOfBox = 50f;
            float topOfBox = this.position.height * .5f + 5f;
            float bottomOfBox = topOfBox + heightOfBox;

            content.text = GDEConstants.ForumLinkText;
            drawHelper.TryGetCachedSize(GDEConstants.SizeForumLinkTextKey, content, linkStyle, out size);

            float widthOfBox = size.x + 10f;
            float leftOfBox = (this.position.width - widthOfBox) / 2f;

            if (GUI.Button(new Rect(leftOfBox + 6f, bottomOfBox - size.y - 2f, size.x, size.y), content, linkStyle))
            {
                Application.OpenURL(GDEConstants.ForumURL);
            }

            content.text = GDEConstants.RateMeText;
            if (GUI.Button(new Rect(leftOfBox + 6f, topOfBox + 3f, size.x, size.y), content, linkStyle))
            {
                Application.OpenURL(GDEConstants.RateMeURL);
            }

            // Draw Export Again & Close Buttons
            content.text = GDEConstants.ExportAgainBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeExportAgainBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, position.height-size.y-drawHelper.BottomBuffer, size.x, size.y), content, buttonStyle))
                nextView = View.Default;

            content.text = GDEConstants.CloseBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeCloseBtnKey, content, buttonStyle, out size);
            if (GUI.Button(new Rect(position.width-size.x-drawHelper.LeftBuffer, position.height-size.y-drawHelper.BottomBuffer, size.x, size.y), content, buttonStyle))
            {
                Close();
            }
        }

        bool HasAuthenticated()
        {
            return GDEDriveHelper.Instance.HasAuthenticated();
        }
    }
}
