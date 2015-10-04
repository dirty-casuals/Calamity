using UnityEngine;
using UnityEditor;
using GameDataEditor;

using System;
using System.IO;

public class GDEMenu : EditorWindow {

	const string contextItemLocation = "Assets/Game Data Editor";
	const string menuItemLocation = "Window/Game Data Editor";
	const int menuItemStartPriority = 300;

	[MenuItem(menuItemLocation + "/" + GDEConstants.DefineDataMenu, false, menuItemStartPriority)]
	static void GDESchemaEditor()
	{
		EditorWindow.GetWindow<GDESchemaManagerWindow>(false, GDEConstants.DefineDataMenu);
	}
	
	[MenuItem(menuItemLocation + "/" + GDEConstants.CreateDataMenu, false, menuItemStartPriority+1)]
	static void GDEItemEditor()
	{
		EditorWindow.GetWindow<GDEItemManagerWindow>(false, GDEConstants.CreateDataMenu);
	}

	
	// **** Divider Here **** //


	[MenuItem(menuItemLocation + "/" + GDEConstants.EncryptMenu, false, menuItemStartPriority+12)]
	static void GDEEncrypt()
	{
		Debug.Log(GDEConstants.EncryptingMsg);
		string dataFilePath = GDEItemManager.DataFilePath;
		GDEEncryption.Encrypt(File.ReadAllText(dataFilePath), GDEItemManager.EncryptedFilePath);
		Debug.Log(GDEConstants.DoneLbl);

		var window = EditorWindow.GetWindow<GDEEncryptionWindow>(true, GDEConstants.EncryptionCompleteLbl);

		window.minSize = new Vector2(650, 200);
		window.Show();
	}

	[MenuItem(menuItemLocation + "/" + GDEConstants.GenerateExtensionsMenu, false, menuItemStartPriority+13)]
	public static void DoGenerateCustomExtensions()
	{
		GDEItemManager.Load();
		
		GDECodeGen.GenStaticKeysClass(GDEItemManager.AllSchemas);
		GDECodeGen.GenClasses(GDEItemManager.AllSchemas);
		
		AssetDatabase.Refresh();
	}


	// **** Divider Here **** //


	[MenuItem(menuItemLocation + "/" + GDEConstants.ImportSpreadsheetMenu, false, menuItemStartPriority+24)]
	public static void DoSpreadsheetImport()
	{
		GDEExcelManager.DoImport();
	}

	/// <summary>
	/// Displays the Localization Editor Export Spreadsheet Wizard
	/// </summary>
	[MenuItem(menuItemLocation + "/" + GDEConstants.ExportSpreadsheetLbl, false, menuItemStartPriority+25)]
	public static void DoExportSpreadsheet()
	{
		GDEExcelManager.DoExport();
	}

	[MenuItem(menuItemLocation + "/" + GDEConstants.ClearExcelMenu, false, menuItemStartPriority+26)]
	static void GDEClearExcelSettings()
	{
		GDEExcelManager.ClearExcelSettings();
	}


	// **** Divider Here **** //


	[MenuItem(menuItemLocation + "/" + GDEConstants.ForumMenu, false, menuItemStartPriority+40)]
	static void GDEForumPost()
	{
		Application.OpenURL(GDEConstants.ForumURL);
	}

	[MenuItem(menuItemLocation + "/" + GDEConstants.DocsMenu, false, menuItemStartPriority+41)]
	static void GDEFreeDocs()
	{
		Application.OpenURL(GDEConstants.DocURL);
	}

	[MenuItem(menuItemLocation + "/" + GDEConstants.RateMenu, false, menuItemStartPriority+42)]
	static void GDERateMe()
	{
		Application.OpenURL(GDEConstants.RateMeURL);
	}

	[MenuItem(menuItemLocation + "/" + GDEConstants.ContactMenu + "/" + GDEConstants.EmailMenu, false, menuItemStartPriority+43)]
	static void GDEEmail()
	{
		Application.OpenURL(GDEConstants.MailTo);
	}

	[MenuItem(menuItemLocation + "/" + GDEConstants.ContactMenu + "/Twitter" , false, menuItemStartPriority+44)]
	static void GDETwitter()
	{
		Application.OpenURL(GDEConstants.Twitter);
	}


	// **** Context Menu Below **** //


	[MenuItem(contextItemLocation + "/" + GDEConstants.LoadDataMenu, true)]
	static bool GDELoadDataValidation()
	{
		return Selection.activeObject != null && Selection.activeObject.GetType() == typeof(TextAsset);
	}

	[MenuItem(contextItemLocation + "/" + GDEConstants.LoadDataMenu, false, menuItemStartPriority)]
	static void GDELoadData () 
	{
		string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
		string fullPath = Path.GetFullPath(assetPath);

		GDESettings.Instance.DataFilePath = fullPath;
		GDESettings.Instance.Save();

		GDEItemManager.Load(true);
	}

	[MenuItem(contextItemLocation + "/" + GDEConstants.LoadAndGenMenu, true)]
	static bool GDELoadAndGenDataValidation()
	{
		return GDELoadDataValidation();
	}

	[MenuItem(contextItemLocation + "/" + GDEConstants.LoadAndGenMenu, false, menuItemStartPriority+1)]
	static void GDELoadAndGenData () 
	{
		GDELoadData();
		DoGenerateCustomExtensions();
	}
}

