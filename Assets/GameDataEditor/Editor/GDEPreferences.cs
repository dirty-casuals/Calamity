using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using GameDataEditor;

public class GDEPreferences : EditorWindow
{
	static GUIStyle headerStyle = null;
	static GUIStyle textFieldStyle = null;

	static Color32 createDataColor;
	static Color32 defineDataColor;
	static Color32 highlightColor;

	static string defaultCreateDataColor;
	static string defaultDefineDataColor;

	static string dataFilePath;
	static bool prettyJson;

	static Vector2 size;

	static GDEDrawHelper _drawHelper;
	static GDEDrawHelper drawHelper
	{
		get
		{
			if (_drawHelper == null)
				_drawHelper = new GDEDrawHelper(null);
			return _drawHelper;
		}
	}

	static GUIContent _content;
	static GUIContent content
	{
		get
		{
			if (_content == null)
				_content = new GUIContent();
			return _content;
		}
	}

	static void SetStyles()
	{
		if (headerStyle.IsNullOrEmpty())
		{
			headerStyle = new GUIStyle(GUI.skin.label);
			headerStyle.fontStyle = FontStyle.Bold;
			headerStyle.fontSize = 16;
		}

		if (textFieldStyle.IsNullOrEmpty())
		{
			textFieldStyle = new GUIStyle(EditorStyles.textField);
			textFieldStyle.wordWrap = true;
		}
	}

	static void LoadDefaultColors()
	{
		if (EditorGUIUtility.isProSkin)
		{
			defaultCreateDataColor = GDEConstants.CreateDataColorPro;
			defaultDefineDataColor = GDEConstants.DefineDataColorPro;
		}
		else 
		{
			defaultCreateDataColor = GDEConstants.CreateDataColor;
			defaultDefineDataColor = GDEConstants.DefineDataColor;
		}
	}
        
	[PreferenceItem(GDEConstants.PreferencesHeader)]
    static void OnGUI()
    {
		SetStyles();
		LoadPreferences();

        content.text = GDEConstants.FileSettingsLbl;
		drawHelper.TryGetCachedSize(GDEConstants.FileSettingsLbl, content, headerStyle, out size);
        GUILayout.Label(content.text, headerStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));

        EditorGUILayout.BeginHorizontal();
		content.text = GDEConstants.CreateDataFileLbl;
		drawHelper.TryGetCachedSize(content.text, content, EditorStyles.label, out size);
        EditorGUILayout.LabelField(content.text, string.Empty, GUILayout.Width(size.x + 2));
        dataFilePath = EditorGUILayout.TextArea(dataFilePath, textFieldStyle);

		content.text = GDEConstants.BrowseBtn;
		drawHelper.TryGetCachedSize(content.text, content, GUI.skin.button, out size);
        if (GUILayout.Button(content.text, GUILayout.Width(size.x)))
        {
            string newDataFilePath = EditorUtility.OpenFilePanel(GDEConstants.OpenDataFileLbl, dataFilePath, string.Empty);
            if (!string.IsNullOrEmpty(newDataFilePath) && !newDataFilePath.Equals(dataFilePath))
                dataFilePath = newDataFilePath;
            GUI.FocusControl(string.Empty);
        }
        EditorGUILayout.EndHorizontal();

		if (!File.Exists(dataFilePath))
		{
			GUILayout.Space(drawHelper.LineHeight/4f);

			content.text = GDEConstants.FileDoesNotExistWarning;
			drawHelper.TryGetCachedSize(content.text, content, EditorStyles.label, out size);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginVertical(GUILayout.Width(size.x * 1.5f));
			GUILayout.Space(3);
			EditorGUILayout.HelpBox(content.text, MessageType.Warning);
			EditorGUILayout.EndVertical();
			
			content.text = GDEConstants.CreateFileLbl;
			drawHelper.TryGetCachedSize(content.text, content, GUI.skin.button, out size);
			if (GUILayout.Button(content, GUILayout.Width(size.x), GUILayout.ExpandHeight(true)))
				GDEItemManager.CreateFileIfMissing(dataFilePath);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		GUILayout.Space(drawHelper.LineHeight/2f);

		EditorGUILayout.BeginHorizontal();
		content.text = GDEConstants.PrettyJsonLbl;
		drawHelper.TryGetCachedSize(content.text, content, EditorStyles.toggle, out size);

		prettyJson = EditorGUILayout.Toggle(prettyJson, GUILayout.Width(20));

		EditorGUILayout.LabelField(content, GUILayout.Width(size.x + 2));

		if (prettyJson)
			EditorGUILayout.HelpBox(GDEConstants.PrettyJsonMsg, MessageType.Warning);
		else
			EditorGUILayout.HelpBox(GDEConstants.PrettyJsonMsg, MessageType.Info);
		EditorGUILayout.EndHorizontal();

        GUILayout.Space(drawHelper.LineHeight);

        content.text = GDEConstants.ColorsLbl;
		drawHelper.TryGetCachedSize(content.text, content, headerStyle, out size);
        GUILayout.Label(content.text, headerStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));

        createDataColor = EditorGUILayout.ColorField(GDEConstants.CreateDataColorLbl, createDataColor);
        defineDataColor = EditorGUILayout.ColorField(GDEConstants.DefineDataColorLbl, defineDataColor);
        highlightColor = EditorGUILayout.ColorField(GDEConstants.HighlightLbl, highlightColor);

		GUILayout.FlexibleSpace();

		content.text = GDEConstants.UseDefaults;
		drawHelper.TryGetCachedSize(content.text, content, GUI.skin.button, out size);
        if (GUILayout.Button(GDEConstants.UseDefaults, GUILayout.Width(size.x * 1.52f)))
            LoadDefaults();

		if (GUI.changed)
			SavePreferences();
    }

    static void LoadPreferences()
    {
		LoadDefaultColors();

		dataFilePath = GDESettings.Instance.DataFilePath;
        if (dataFilePath.Equals(GDESettings.DefaultDataFilePath))
            CreateDirectory(Path.GetDirectoryName(dataFilePath));

		GDESettings settings = GDESettings.Instance;

		createDataColor = settings.CreateDataColor.ToColor32();
		defineDataColor = settings.DefineDataColor.ToColor32();
		highlightColor = settings.HighlightColor.ToColor32();

		prettyJson = settings.PrettyJson;
    }

	static void LoadDefaults()
    {
		dataFilePath = GDESettings.DefaultDataFilePath;
		GDEItemManager.CreateFileIfMissing(dataFilePath);

        createDataColor = defaultCreateDataColor.ToColor();
        defineDataColor = defaultDefineDataColor.ToColor();
        highlightColor = GDEConstants.HighlightColor.ToColor();

		prettyJson = false;

        GUI.FocusControl(string.Empty);
    }

	static void SavePreferences()
    {
		GDESettings settings = GDESettings.Instance;
		settings.PrettyJson = prettyJson;

		settings.CreateDataColor = createDataColor.ToHexString();
		settings.DefineDataColor = defineDataColor.ToHexString();
		settings.HighlightColor = highlightColor.ToHexString();
		settings.DataFilePath = dataFilePath;

		if (File.Exists(dataFilePath))
		{
	        GDEItemManager.Load(true);
		    GUI.FocusControl(string.Empty);
		}
    
		settings.Save();
    }

	static void CreateDirectory(string dir)
    {
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }
}

