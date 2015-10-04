using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace GameDataEditor
{
    public enum GDEImportView
    {
        Default,
        ImportLocalFile,
        LaunchAuthURL,
        Authenticate,
        GoogleSheetDownload,
        ImportComplete
    }


	public class GDEImportExcel : EditorWindow 
	{
	    GUIStyle headerStyle = null;
	    GUIStyle linkStyle = null;
	    GUIStyle buttonStyle = null;
	    GUIStyle labelStyle = null;
	    GUIStyle rateBoxStyle = null;
	    GUIStyle comboBoxStyle = null;
	    GUIStyle textFieldStyle = null;

	    string spreadsheetPath = "";
	    
	    string accessCode = "";
	    int downloadSelectionIndex = 0;

	    static GDEImportView currentView;
	    static GDEImportView nextView;

	    static float windowPadding = 20f;

		GDEExcelDataHelper excelDataHelper;

		public static string googleSheetImportName;

	    public void LoadSettings(GDEImportView view = GDEImportView.Default)
	    {
	        
			spreadsheetPath = GDESettings.Instance.ImportedLocalSpreadsheetName;
			googleSheetImportName = GDESettings.Instance.ImportedGoogleSpreadsheetName;

	        currentView = view;
	        nextView = view;

			minSize = new Vector2(420, 250);
			maxSize = minSize;
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

		void OnInspectorUpdate()
		{
			Repaint();
		}
	    
	    void OnGUI()
	    {
			SetStyles();

			if (currentView.Equals(GDEImportView.Default))
	            DrawDefaultView();
	        else if (currentView.Equals(GDEImportView.LaunchAuthURL))
	            DrawLaunchAuthURL();
	        else if (currentView.Equals(GDEImportView.Authenticate))
	            DrawAuthenticateView();
	        else if (currentView.Equals(GDEImportView.GoogleSheetDownload))
	            DrawGoogleSheetDownload();
	        else if (currentView.Equals(GDEImportView.ImportLocalFile))
	            DrawImportLocalFile();
	        else if (currentView.Equals(GDEImportView.ImportComplete))
	            DrawImportComplete();

	        currentView = nextView;
	    }

	    void DrawDefaultView()
	    {
	        EditorGUILayout.BeginHorizontal();
	        GUILayout.FlexibleSpace();
	        GUIContent content = new GUIContent(GDEConstants.ChooseImportLbl);
	        Vector2 size = headerStyle.CalcSize(content);
	        GUILayout.Label(content, headerStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();

	        GUILayout.Space(20f);

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.FlexibleSpace();
	        if (GUILayout.Button(GDEConstants.ImportLocalLbl, buttonStyle))
	            nextView = GDEImportView.ImportLocalFile;
	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();

	        GUILayout.FlexibleSpace();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.FlexibleSpace();
	        if (GUILayout.Button(GDEConstants.ImportGoogleSSLbl, buttonStyle))
	        {
	            if (HasAuthenticated())
	            {
	                nextView = GDEImportView.GoogleSheetDownload;
	                GDEDriveHelper.Instance.GetSpreadsheetList();

					int index = Array.IndexOf(GDEDriveHelper.Instance.SpreadSheetNames, googleSheetImportName);
					if (GDEDriveHelper.Instance.SpreadSheetNames.IsValidIndex(index))
						downloadSelectionIndex = index;
	            }
	            else
	                nextView = GDEImportView.LaunchAuthURL;
	        }
	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();

	        GUILayout.FlexibleSpace();

	        EditorGUILayout.BeginHorizontal();        
	        GUILayout.FlexibleSpace();
	        if (GUILayout.Button(GDEConstants.ReauthWithGoogleLbl, buttonStyle))
	        {
	            nextView = GDEImportView.LaunchAuthURL;
	        }
	        GUILayout.FlexibleSpace();       
	        EditorGUILayout.EndHorizontal();

	        GUILayout.FlexibleSpace();
	    }

	    void DrawImportLocalFile()
	    {
	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
	        GUIContent content = new GUIContent(GDEConstants.ImportWBLbl);
	        Vector2 size = headerStyle.CalcSize(content);
	        GUILayout.Label(content, headerStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        EditorGUILayout.EndHorizontal();

	        GUILayout.FlexibleSpace();

	        EditorGUILayout.BeginHorizontal();        
	        GUILayout.Space(windowPadding);
	        content.text = GDEConstants.ExcelFileLbl;
	        size = labelStyle.CalcSize(content);
	        GUILayout.Label(content, labelStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        EditorGUILayout.EndHorizontal();

	        EditorGUILayout.BeginHorizontal();        
	        GUILayout.Space(windowPadding);
	        spreadsheetPath = EditorGUILayout.TextField(spreadsheetPath, textFieldStyle);
	        GUILayout.Space(windowPadding);
	        EditorGUILayout.EndHorizontal();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
	        if (GUILayout.Button(GDEConstants.BrowseBtn, buttonStyle))
	        {
	            string newSpreadSheetPath = EditorUtility.OpenFilePanel(GDEConstants.OpenWBLbl, spreadsheetPath, string.Empty);
	            if (!string.IsNullOrEmpty(newSpreadSheetPath) && !newSpreadSheetPath.Equals(spreadsheetPath))
	                spreadsheetPath = newSpreadSheetPath;
	            GUI.FocusControl(string.Empty);
	        }
	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();

	        GUILayout.FlexibleSpace();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding*2f);
	        if (GUILayout.Button(GDEConstants.BackBtn, buttonStyle))        
	            nextView = GDEImportView.Default;

	        GUILayout.FlexibleSpace();
	        
	        if (GUILayout.Button(GDEConstants.ImportBtn, buttonStyle))
	        {
				// Save the import settings
				GDESettings settings = GDESettings.Instance;
				settings.ImportedLocalSpreadsheetName = spreadsheetPath;
				settings.ImportType = ImportExportType.Local;
				settings.Save();

				// Do the import
				GDEExcelManager.DoImport();
	            nextView = GDEImportView.ImportComplete;
	        }
	        GUILayout.Space(windowPadding*2f);
	        EditorGUILayout.EndHorizontal();

	        GUILayout.Space(windowPadding);
	    }

	    void DrawLaunchAuthURL()
	    {
	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
	        GUIContent content = new GUIContent(GDEConstants.AuthWithGoogleLbl);
	        Vector2 size = headerStyle.CalcSize(content);
	        GUILayout.Label(content, headerStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        EditorGUILayout.EndHorizontal();

	        GUILayout.FlexibleSpace();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
			content.text = GDEConstants.AuthInstruction1_1;
	        size = labelStyle.CalcSize(content);
	        GUILayout.Label(content, labelStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding+20f);
	        content.text = GDEConstants.AuthInstruction1_2;
	        size = labelStyle.CalcSize(content);
	        GUILayout.Label(content, labelStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();

	        GUILayout.FlexibleSpace();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
			content.text = GDEConstants.AuthInstruction2_1;
	        size = labelStyle.CalcSize(content);
	        GUILayout.Label(content, labelStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding+20f);
			content.text = GDEConstants.AuthInstruction2_2;
	        size = labelStyle.CalcSize(content);
	        GUILayout.Label(content, labelStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();

	        GUILayout.FlexibleSpace();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
	        if (GUILayout.Button(GDEConstants.BackBtn, buttonStyle))        
	            nextView = GDEImportView.Default;

	        GUILayout.FlexibleSpace();

	        if (GUILayout.Button(GDEConstants.GotoAuthURL, buttonStyle))
	        {
	            GDEDriveHelper.Instance.RequestAuthFromUser();
	            nextView = GDEImportView.Authenticate;
	        }
	        GUILayout.Space(windowPadding*2f);
	        EditorGUILayout.EndHorizontal();

	        GUILayout.Space(windowPadding);
	    }

	    void DrawAuthenticateView()
	    {
	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
	        GUIContent content = new GUIContent(GDEConstants.AuthWithGoogleLbl);
	        Vector2 size = headerStyle.CalcSize(content);
	        GUILayout.Label(content, headerStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        EditorGUILayout.EndHorizontal();

	        GUILayout.FlexibleSpace();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
			content.text = GDEConstants.EnterAccessCodeLbl;
	        size = labelStyle.CalcSize(content);
	        GUILayout.Label(content, labelStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        EditorGUILayout.EndHorizontal();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
	        accessCode = EditorGUILayout.TextField(accessCode, textFieldStyle);
	        GUILayout.Space(windowPadding);
	        EditorGUILayout.EndHorizontal();

	        GUILayout.FlexibleSpace();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding*2f);
	        if (GUILayout.Button(GDEConstants.BackBtn, buttonStyle))        
	            nextView = GDEImportView.LaunchAuthURL;

	        GUILayout.FlexibleSpace();

	        if (accessCode != string.Empty && GUILayout.Button(GDEConstants.SetCodeLbl, buttonStyle))
	        {
	            GDEDriveHelper.Instance.SetAccessCode(accessCode);
	            GDEDriveHelper.Instance.GetSpreadsheetList();
	            nextView = GDEImportView.GoogleSheetDownload;
	        }
	        GUILayout.Space(windowPadding*2f);
	        EditorGUILayout.EndHorizontal();

	        GUILayout.Space(windowPadding);
	    }

	    void DrawGoogleSheetDownload()
	    {
	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
	        GUIContent content = new GUIContent(GDEConstants.DownloadGoogleSheetLbl);
	        Vector2 size = headerStyle.CalcSize(content);
	        GUILayout.Label(content, headerStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        EditorGUILayout.EndHorizontal();
	        
	        GUILayout.FlexibleSpace();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
	        content.text = GDEConstants.SelectSheetLbl;
	        size = labelStyle.CalcSize(content);
	        GUILayout.Label(content, labelStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        EditorGUILayout.EndHorizontal();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
	        downloadSelectionIndex = EditorGUILayout.Popup(downloadSelectionIndex, GDEDriveHelper.Instance.SpreadSheetNames, comboBoxStyle);
	        GUILayout.Space(windowPadding);
	        EditorGUILayout.EndHorizontal();

	        GUILayout.FlexibleSpace();
	        
	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding*2f);
	        if (GUILayout.Button(GDEConstants.BackBtn, buttonStyle))        
	            nextView = GDEImportView.Default;
	        
	        GUILayout.FlexibleSpace();
	        
	        if (GUILayout.Button(GDEConstants.DownloadBtn, buttonStyle))
	        {
				googleSheetImportName = GDEDriveHelper.Instance.SpreadSheetNames[downloadSelectionIndex];

				// Save import settings
				GDESettings settings = GDESettings.Instance;
				settings.ImportedGoogleSpreadsheetName = googleSheetImportName;
				settings.ImportType = ImportExportType.Google;
				settings.Save();
				
				/*
				GoogleDriveHelper driveHelper = GoogleDriveHelper.Instance;
				spreadsheetPath = driveHelper.DownloadSpreadSheet(driveHelper.SpreadSheetNames[downloadSelectionIndex],  
				                                                  "import_" + googleSheetImportName + ".xlsx");
				*/

				GDEExcelManager.DoImport();
				nextView = GDEImportView.ImportComplete;
	        }
	        GUILayout.Space(windowPadding*2f);
	        EditorGUILayout.EndHorizontal();

	        GUILayout.Space(windowPadding);
	    }

	    void DrawImportComplete()
	    {
	        EditorGUILayout.BeginHorizontal();
	        GUILayout.FlexibleSpace();
	        GUIContent content = new GUIContent(GDEConstants.ImportCompleteLbl);
	        Vector2 size = headerStyle.CalcSize(content);
	        GUILayout.Label(content, headerStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();

	        GUILayout.Space(20f);

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
	        content.text = GDEConstants.ImportMsg1;
	        size = labelStyle.CalcSize(content);
	        GUILayout.Label(content, labelStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding);
	        content.text = GDEConstants.ImportMsg2;
	        size = labelStyle.CalcSize(content);
	        GUILayout.Label(content, labelStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();

	        GUILayout.Space(20f);

	        // Draw rate box    
	        float heightOfBox = 50f;
	        float topOfBox = this.position.height * .5f + 5f;
	        float bottomOfBox = topOfBox + heightOfBox;

	        content.text = GDEConstants.ForumLinkText;
	        size = linkStyle.CalcSize(content);

	        float widthOfBox = size.x+10f;
	        float leftOfBox = (this.position.width - widthOfBox)/2f;

	        if (GUI.Button(new Rect(leftOfBox+6f, bottomOfBox-size.y-2f, size.x, size.y), content, linkStyle))
	        {
	            Application.OpenURL(GDEConstants.ForumURL);
	        }
	        
	        content.text = GDEConstants.RateMeText;
	        if(GUI.Button(new Rect(leftOfBox+6f, topOfBox+3f, size.x, size.y), content, linkStyle))
	        {
	            Application.OpenURL(GDEConstants.RateMeURL);
	        }
	        
	        GUI.Box(new Rect(leftOfBox, topOfBox, 2f, heightOfBox), string.Empty, rateBoxStyle);
	        GUI.Box(new Rect(leftOfBox, topOfBox, widthOfBox, 2f), string.Empty, rateBoxStyle);
	        GUI.Box(new Rect(leftOfBox, topOfBox+heightOfBox, widthOfBox+2f, 2f), string.Empty, rateBoxStyle);
	        GUI.Box(new Rect(leftOfBox+widthOfBox, topOfBox, 2f, heightOfBox), string.Empty, rateBoxStyle);

	        GUILayout.FlexibleSpace();

	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Space(windowPadding*2f);
	        if (GUILayout.Button(GDEConstants.ImportAgainBtn, buttonStyle))        
	            nextView = GDEImportView.Default;
	        
	        GUILayout.FlexibleSpace();
	        
	        if (GUILayout.Button(GDEConstants.CloseBtn, buttonStyle))
	        {
				Close();
	        }
	        GUILayout.Space(windowPadding*2f);
	        EditorGUILayout.EndHorizontal();

	        GUILayout.Space(windowPadding);
	    }

	    bool HasAuthenticated()
	    {
			return GDEDriveHelper.Instance.HasAuthenticated();
	    }
	}
}
