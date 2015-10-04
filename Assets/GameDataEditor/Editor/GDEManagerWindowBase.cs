using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GameDataEditor;

public class GDESaveHook : UnityEditor.AssetModificationProcessor
{
	public static string[] OnWillSaveAssets(string[] paths)
	{
		if (GDEItemManager.ItemsNeedSave || GDEItemManager.SchemasNeedSave)
			GDEItemManager.Save();
		return paths;
	}
}

public abstract class GDEManagerWindowBase : EditorWindow {

    protected HashSet<string> entryFoldoutState = new HashSet<string>();
    protected HashSet<string> listFieldFoldoutState = new HashSet<string>();
	protected HashSet<string> previewState = new HashSet<string>();
    protected bool currentFoldoutAllState = false;

    protected Dictionary<string, int> newListCountDict = new Dictionary<string, int>();

    protected string filterText = string.Empty;
	protected string newFilterText = string.Empty;

    protected Vector2 verticalScrollbarPosition;

	protected float scrollViewHeight = 0;
	protected float scrollViewY = 0;
	protected bool shouldRecalculateHeights = true;
	protected float currentGroupHeightTotal = 0;

    protected Dictionary<string, float> groupHeights = new Dictionary<string, float>();
    protected Dictionary<string, float> groupHeightBySchema = new Dictionary<string, float>();

	protected bool shouldRebuildEntriesList = true;
	protected Dictionary<string, Dictionary<string, object>> entriesToDraw;
	protected List<string> deleteEntries = new List<string>();
	protected List<string> cloneEntries = new List<string>();

    protected GUIStyle foldoutStyle = null;
    protected GUIStyle labelStyle = null;
    protected GUIStyle saveButtonStyle = null;
    protected GUIStyle loadButtonStyle = null;
    protected GUIStyle linkStyle = null;
    protected GUIStyle rateBoxStyle = null;
	protected GUIStyle audioPreviewStyle = null;
	protected GUIStyle searchStyle = null;
	protected GUIStyle searchCancelStyle = null;
	protected GUIStyle searchCancelEmptyStyle = null;

	static GameObject _audioSampler = null;
	protected static GameObject AudioSampler
	{
		get
		{
			if (_audioSampler == null)
			{
				_audioSampler = new GameObject("GDE Audio Sampler", typeof(AudioSource));
				_audioSampler.hideFlags = HideFlags.HideAndDontSave;
			}
			return _audioSampler;
		}
	}

	static AudioSource _audioSource = null;
    protected static AudioSource AudioSamplerAS
	{
		get
		{
			if (_audioSource == null)
				_audioSource = AudioSampler.GetComponent<AudioSource>();
			return _audioSource;
		}
	}
    
	protected Vector2 size = Vector2.zero;

	GUIContent _content;
	protected GUIContent content
	{
		get
		{
			if (_content == null)
				_content = new GUIContent();
			return _content;
		}
	}

	protected string highlightContent;
	protected float highlightStartTime;
	protected Color32 highlightColorStart;
	protected Color32 highlightColorEnd;
	protected Color32 newhighlightColor;
	protected string newhighlightColorStr;
	protected float lerpColorProgress;

	GDEDrawHelper _drawHelper;
	protected GDEDrawHelper drawHelper
	{
		get {
			if (_drawHelper == null)
				_drawHelper = new GDEDrawHelper(this);
			return _drawHelper;
		}
	}

    protected string saveButtonText = GDEConstants.SaveBtn;

    protected Color headerColor = Color.red;
	protected string mainHeaderText = "Oops";
	protected float buttonHeightMultiplier = 1.5f;
	protected float buttonWidth = 60f;

    protected string highlightColor;

	protected string SearchCancelContent = string.Empty;

    protected double lastClickTime = 0;
    protected string lastClickedKey = string.Empty;
    protected HashSet<string> editingFields = new HashSet<string>();
    protected Dictionary<string, string> editFieldTextDict = new Dictionary<string, string>();
    protected delegate bool DoRenameDelgate(string oldValue, string newValue, Dictionary<string, object> data, out string errorMsg);

    void OnFocus()
    {
        if (GDEItemManager.AllItems.Count.Equals(0) && GDEItemManager.AllSchemas.Count.Equals(0) && !GDEItemManager.ItemsNeedSave && !GDEItemManager.SchemasNeedSave)
            GDEItemManager.Load();
    }

	void SetStyles()
	{
		if (labelStyle.IsNullOrEmpty())
		{
			labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.richText = true;
		}

		if (saveButtonStyle.IsNullOrEmpty())
		{
			saveButtonStyle = new GUIStyle(GUI.skin.button);
			saveButtonStyle.fontSize = 14;
		}

		if (loadButtonStyle.IsNullOrEmpty())
		{
			loadButtonStyle = new GUIStyle(GUI.skin.button);
			loadButtonStyle.fontSize = 14;
		}

		if (foldoutStyle.IsNullOrEmpty())
		{
			foldoutStyle = new GUIStyle(EditorStyles.foldout);
			foldoutStyle.richText = true;
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

		if (audioPreviewStyle.IsNullOrEmpty())
		{
			audioPreviewStyle = new GUIStyle(EditorStyles.foldout);
		}

		if (searchStyle.IsNullOrEmpty())
		{
			var style = GUI.skin.FindStyle("ToolbarSeachTextField");
			if (!style.IsNullOrEmpty())
				searchStyle = new GUIStyle(style);
			else
				searchStyle = new GUIStyle(EditorStyles.textField);

			searchStyle.fixedWidth = 200f;
			searchStyle.fontSize = 12;
			searchStyle.fixedHeight = 16;
			searchStyle.contentOffset = new Vector2(0, -1);
        }

		if (searchCancelStyle.IsNullOrEmpty())
		{
			var style = GUI.skin.FindStyle("ToolbarSeachCancelButton");
			if (!style.IsNullOrEmpty())
				searchCancelStyle = new GUIStyle(style);
			else
			{
				searchCancelStyle = new GUIStyle(GUI.skin.button);
				SearchCancelContent = "X";
			}

			searchCancelStyle.fixedHeight = 16;
		}

		if (searchCancelEmptyStyle.IsNullOrEmpty())
		{
			var style = GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty");
			if (!style.IsNullOrEmpty())
				searchCancelEmptyStyle = new GUIStyle(style);
			else
				searchCancelEmptyStyle = new GUIStyle();

			searchCancelEmptyStyle.fixedHeight = 16;
		}
	}

	protected virtual void Update()
	{
		if (string.IsNullOrEmpty(highlightContent))
			return;

		float timeElapsed = Time.realtimeSinceStartup - highlightStartTime;

		// Flip the colors
		if (lerpColorProgress >= 1f)
		{
			if (highlightColorEnd.NearlyEqual(GDEConstants.NewHighlightStart.ToColor32()))
			{
				highlightColorEnd = GDEConstants.NewHighlightEnd.ToColor32();
				highlightColorStart = GDEConstants.NewHighlightStart.ToColor32();
			}
			else
			{
				highlightColorEnd = GDEConstants.NewHighlightStart.ToColor32();
				highlightColorStart = GDEConstants.NewHighlightEnd.ToColor32();
			}
			lerpColorProgress = 0f;
		}

		if (timeElapsed >= GDEConstants.NewHighlightDuration)
		{
			highlightContent = string.Empty;
			highlightStartTime = 0;
			lerpColorProgress = 0;
		}
		else if ((timeElapsed%GDEConstants.NewHighlightRate).NearlyEqual(0f))
		{
			// Update the color
			newhighlightColor = Color32.Lerp(highlightColorStart, highlightColorEnd, lerpColorProgress/GDEConstants.NewHightlightPeriod);
			newhighlightColorStr = newhighlightColor.ToHexString();

			lerpColorProgress += GDEConstants.NewHighlightRate;
		}
	}

	void OnInspectorUpdate()
	{
		Repaint();
	}

    #region OnGUI and DrawHeader Methods
    protected virtual void OnGUI()
    {
		SetStyles();

		highlightColor = GDESettings.Instance.HighlightColor;

        drawHelper.ResetToTop();

        // Process page up/down press & home/end press
        if (Event.current.isKey)
        {
            if (Event.current.keyCode == KeyCode.PageDown)
            {
                verticalScrollbarPosition.y += scrollViewHeight/2f;
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.PageUp)
            {
                verticalScrollbarPosition.y -= scrollViewHeight/2f;
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.Home)
            {
                verticalScrollbarPosition.y = 0;
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.End)
            {
                verticalScrollbarPosition.y = CalculateGroupHeightsTotal()-GDEConstants.LineHeight-scrollViewY;
                Event.current.Use();
            }
        }

		drawHelper.DrawMainHeaderLabel(mainHeaderText, headerColor, GDEConstants.SizeEditorHeaderKey);
        DrawHeader();

        DrawCreateSection();
    }

    protected virtual void DrawHeader()
    {
        size.x = 60;

        if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()*buttonHeightMultiplier), GDEConstants.LoadBtn, loadButtonStyle))
        {
            Load();
            GUI.FocusControl(string.Empty);
        }
        drawHelper.CurrentLinePosition += (size.x + 2);

        content.text = FilePath();
		drawHelper.TryGetCachedSize(GDEConstants.SizeFileLblKey, content, EditorStyles.label, out size);
		EditorGUI.SelectableLabel(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), drawHelper.WidthLeftOnCurrentLine(), size.y), content.text);
        drawHelper.CurrentLinePosition += (size.x + 2);

        drawHelper.NewLine(buttonHeightMultiplier+.1f);

        if (NeedToSave())
        {
            size.x = 110;
            saveButtonStyle.normal.textColor = Color.red;
            saveButtonStyle.fontStyle = FontStyle.Bold;
            saveButtonText = GDEConstants.SaveNeededBtn;
        }
        else
        {
            size.x = 60;
            saveButtonStyle.normal.textColor = GUI.skin.button.normal.textColor;
            saveButtonStyle.fontStyle = FontStyle.Normal;
            saveButtonText = GDEConstants.SaveBtn;
        }

        if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()*buttonHeightMultiplier), saveButtonText, saveButtonStyle))
            Save();

        drawHelper.NewLine(buttonHeightMultiplier+.5f);

		drawHelper.DrawSectionSeparator();

		DrawSpreadsheetSection();

		drawHelper.NewLine(.5f);

        if (DrawFilterSection())
            ClearSearch();

		drawHelper.DrawSectionSeparator();
    }

    protected virtual void DrawRateBox(float left, float top, float width, float height)
    {
        GUI.Box(new Rect(left, top, 2f, height), string.Empty, rateBoxStyle);
        GUI.Box(new Rect(left, top, width, 2f), string.Empty, rateBoxStyle);
        GUI.Box(new Rect(left, top+height, width+2, 2f), string.Empty, rateBoxStyle);
        GUI.Box(new Rect(left+width, top, 2f, height), string.Empty, rateBoxStyle);
    }

	void DrawSpreadsheetSection()
	{
		GDESettings settings = GDESettings.Instance;

		drawHelper.NewLine(0.75f);
		drawHelper.DrawSubHeader(GDEConstants.SpreadsheetSectionHeader, headerColor, GDEConstants.SizeSpreadsheetSectionHeaderKey, false);
		drawHelper.CurrentLinePosition += 5f;

		float pos = drawHelper.CurrentLinePosition;

		// Import Button
		content.text = GDEConstants.ImportBtn;
		if (!drawHelper.SizeCache.ContainsKey(GDEConstants.SizeImportBtnKey))
	    {
			size.x = buttonWidth;
			size.y = loadButtonStyle.CalcHeight(content, size.x);
			drawHelper.SizeCache.Add(GDEConstants.SizeImportBtnKey, new Vector2(size.x, size.y));
		}
		else
			size = drawHelper.SizeCache[GDEConstants.SizeImportBtnKey];

		if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()*buttonHeightMultiplier), content, loadButtonStyle))
		{
			GDEExcelManager.DoImport();
		}
		drawHelper.CurrentLinePosition += size.x + 4f;

		// Import label
		string path;
		if (!settings.ImportType.Equals(ImportExportType.None))
		{
			if (settings.ImportType.Equals(ImportExportType.Google))
			{
				content.text = string.Format(GDEConstants.ImportSource, GDEConstants.GoogleDrive);
				path = settings.ImportedGoogleSpreadsheetName;
			}
			else
			{
				content.text = string.Format(GDEConstants.ImportSource, string.Empty);
				path = settings.ImportedLocalSpreadsheetName;
			}

			size = labelStyle.CalcSize(content);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine()+drawHelper.LineHeight*(buttonHeightMultiplier-1)/2f, size.x, size.y), content, labelStyle);
			drawHelper.CurrentLinePosition += (size.x + 2f);

			content.text = path;
			size = labelStyle.CalcSize(content);
			drawHelper.TryGetCachedSize(content.text, content, labelStyle, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine()+drawHelper.LineHeight*(buttonHeightMultiplier-1)/2f, size.x, size.y), content, labelStyle);
			drawHelper.CurrentLinePosition += (size.x + 2f);
		}

		drawHelper.NewLine(buttonHeightMultiplier);
		drawHelper.CurrentLinePosition = pos;

		// Export Button
		content.text = GDEConstants.ExportBtn;
		if (!drawHelper.SizeCache.ContainsKey(GDEConstants.SizeExportBtnKey))
		{
			size.x = buttonWidth;
			size.y = loadButtonStyle.CalcHeight(content, size.x);
			drawHelper.SizeCache.Add(GDEConstants.SizeExportBtnKey, new Vector2(size.x, size.y));
		}
		else
			size = drawHelper.SizeCache[GDEConstants.SizeExportBtnKey];

		if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()*buttonHeightMultiplier), content, loadButtonStyle))
		{
			GDEExcelManager.DoExport();
		}
		drawHelper.CurrentLinePosition += size.x + 4f;

		// Export label
		if (!settings.ExportType.Equals(ImportExportType.None))
		{
			if (settings.ExportType.Equals(ImportExportType.Google))
			{
				content.text = string.Format(GDEConstants.ExportDest, GDEConstants.GoogleDrive);
				path = settings.ExportedGoogleSpreadsheetPath;
			}
			else
			{
				content.text = string.Format(GDEConstants.ExportDest, string.Empty);
				path = settings.ExportedLocalSpreadsheetName;
			}

			size = labelStyle.CalcSize(content);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine()+drawHelper.LineHeight*(buttonHeightMultiplier-1)/2f, size.x, size.y), content, labelStyle);
			drawHelper.CurrentLinePosition += (size.x + 2f);

			content.text = path;
			size = labelStyle.CalcSize(content);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine()+drawHelper.LineHeight*(buttonHeightMultiplier-1)/2f, size.x, size.y), content, labelStyle);
			drawHelper.CurrentLinePosition += (size.x + 2f);
		}

		drawHelper.NewLine(buttonHeightMultiplier+.25f);

		drawHelper.DrawSectionSeparator();
	}

	protected virtual void DrawEntryFooter(string cloneLbl, string cloneSizeKey, string entryKey)
	{
		content.text = cloneLbl;
		drawHelper.TryGetCachedSize(cloneSizeKey, content, GUI.skin.button, out size);
		if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
			cloneEntries.Add(entryKey);
		drawHelper.CurrentLinePosition += (size.x + 2f);

		content.text = GDEConstants.DeleteBtn;
		drawHelper.TryGetCachedSize(GDEConstants.SizeDeleteBtnKey, content, GUI.skin.button, out size);
		if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
			deleteEntries.Add(entryKey);

		drawHelper.NewLine();

		drawHelper.DrawSectionSeparator();

		drawHelper.NewLine(0.25f);
	}
    #endregion

    #region GUI Position Methods
    protected virtual void SetGroupHeight(string forKey, float height)
    {
		groupHeights[forKey] = height;
    }
    #endregion

    #region Foldout Methods
    protected virtual void DrawEditableLabel(string editableLabel, string editFieldKey, DoRenameDelgate doRename)
    {
        DrawEditableLabel(editableLabel, editFieldKey, doRename, null);
    }

    protected virtual void DrawEditableLabel(string editableLabel, string editFieldKey, DoRenameDelgate doRename, Dictionary<string, object> data)
    {
        if (!editingFields.Contains(editFieldKey))
        {
			if (editableLabel.Equals(highlightContent))
				content.text = editableLabel.HighlightSubstring(highlightContent, newhighlightColorStr);
			else
				content.text = editableLabel.HighlightSubstring(filterText, highlightColor);

			drawHelper.TryGetCachedSize(editFieldKey, content, labelStyle, out size);
			size.x = Math.Max(size.x, GDEConstants.MinLabelWidth);

            if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle))
            {
                if (EditorApplication.timeSinceStartup - lastClickTime < GDEConstants.DoubleClickTime && lastClickedKey.Equals(editFieldKey))
                {
                    lastClickedKey = string.Empty;
                    editingFields.Add(editFieldKey);
                }
                else
                {
                    lastClickedKey = editFieldKey;
                    editingFields.Remove(editFieldKey);
                }

                lastClickTime = EditorApplication.timeSinceStartup;
            }
            drawHelper.CurrentLinePosition += (size.x + 2);
        }
        else
        {
            string editFieldText;
            if (!editFieldTextDict.TryGetValue(editFieldKey, out editFieldText))
                editFieldText = editableLabel;

            string newValue = DrawResizableTextBox(editFieldText);
            editFieldTextDict.TryAddOrUpdateValue(editFieldKey, newValue);

            if (!newValue.Equals(editableLabel))
            {
                content.text = GDEConstants.RenameBtn;
				drawHelper.TryGetCachedSize(GDEConstants.SizeRenameBtnKey, content, GUI.skin.button, out size);
				if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content) && doRename != null)
                {
                    string error;
                    if (doRename(editableLabel, newValue, data, out error))
                    {
                        editingFields.Remove(editFieldKey);
                        editFieldTextDict.Remove(editFieldKey);
                        GUI.FocusControl(string.Empty);

						// Clear the current component size so its recalculated next draw cycle
						drawHelper.SizeCache.Remove(editFieldKey);
                    }
                    else
                        EditorUtility.DisplayDialog(GDEConstants.ErrorLbl, string.Format(GDEConstants.CouldNotRenameFormat, editableLabel, newValue, error), GDEConstants.OkLbl);
                }
                drawHelper.CurrentLinePosition += (size.x + 2);

                content.text = GDEConstants.CancelBtn;
				drawHelper.TryGetCachedSize(GDEConstants.SizeCancelBtnKey, content, GUI.skin.button, out size);
				if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
                {
                    editingFields.Remove(editFieldKey);
                    editFieldTextDict.Remove(editFieldKey);
                    GUI.FocusControl(string.Empty);
                }
                drawHelper.CurrentLinePosition += (size.x + 2);
            }
            else
            {
                content.text = GDEConstants.CancelBtn;
				drawHelper.TryGetCachedSize(GDEConstants.SizeCancelBtnKey, content, GUI.skin.button, out size);
				if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
                {
                    editingFields.Remove(editFieldKey);
                    editFieldTextDict.Remove(editFieldKey);
                    GUI.FocusControl(string.Empty);
                }
                drawHelper.CurrentLinePosition += (size.x + 2);
            }
        }
    }

    protected virtual bool DrawFoldout(string foldoutLabel, string key, string editableLabel, string editFieldKey, DoRenameDelgate doRename)
    {
        bool currentFoldoutState = entryFoldoutState.Contains(key);

		content.text = foldoutLabel;
		drawHelper.TryGetCachedSize(foldoutLabel+GDEConstants.LblSuffix, content, foldoutStyle, out size);
        bool newFoldoutState = EditorGUI.Foldout(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), currentFoldoutState,
                                                 foldoutLabel.HighlightSubstring(filterText, highlightColor), foldoutStyle);
        drawHelper.CurrentLinePosition += (size.x + 2);

		if (doRename != null)
        	DrawEditableLabel(editableLabel, editFieldKey, doRename);

        SetFoldout(newFoldoutState, key);

        return newFoldoutState;
    }

    protected virtual void DrawExpandCollapseAllFoldout(string[] forKeys, string headerText)
    {
        drawHelper.DrawSubHeader(headerText, headerColor, GDEConstants.SizeListSubHeaderKey);

        string key;
		if (currentFoldoutAllState)
		{
            content.text = GDEConstants.CollapseAllLbl;
			key = GDEConstants.SizeCollapseAllLblKey;
		}
        else
		{
            content.text = GDEConstants.ExpandAllLbl;
			key = GDEConstants.SizeExpandAllLblKey;
		}

		drawHelper.TryGetCachedSize(key, content, EditorStyles.foldout, out size);
        bool newFoldAllState = EditorGUI.Foldout(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), currentFoldoutAllState, content.text);
        if (newFoldAllState != currentFoldoutAllState)
        {
            SetAllFoldouts(newFoldAllState, forKeys);
            currentFoldoutAllState = newFoldAllState;

            // Reset scrollbar if we just collapsed everything
            if (!newFoldAllState)
                verticalScrollbarPosition.y = 0;

			shouldRecalculateHeights = true;
        }

        drawHelper.NewLine();
    }

    protected virtual void SetAllFoldouts(bool state, string[] forKeys)
    {
        foreach(string key in forKeys)
            SetFoldout(state, key);
    }

    protected virtual void SetFoldout(bool state, string forKey)
    {
        if (state)
            entryFoldoutState.Add(forKey);
        else
            entryFoldoutState.Remove(forKey);
    }
    #endregion

    #region Draw Field Methods
    protected virtual void DrawBool(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object currentValue;
            bool newValue;
            string key = fieldName;

            data.TryGetValue(key, out currentValue);

            content.text = label;
			drawHelper.TryGetCachedSize(label, content, EditorStyles.label, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
            drawHelper.CurrentLinePosition += (size.x + 2);

            size.x = 50;
            newValue = EditorGUI.Toggle(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), Convert.ToBoolean(currentValue));
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (newValue != Convert.ToBoolean(currentValue))
            {
                data[key] = newValue;
                SetNeedToSave(true);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawListBool(GUIContent label, int index, bool value, IList boolList)
    {
		try
        {
            bool newValue;

			drawHelper.TryGetCachedSize(label.text, label, EditorStyles.label, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), label);
            drawHelper.CurrentLinePosition += (size.x + 2);

            size.x = 30;
            newValue = EditorGUI.Toggle(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), value);
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (value != newValue)
            {
                boolList[index] = newValue;
                SetNeedToSave(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawInt(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object currentValue;
            int newValue;
            string key = fieldName;

            data.TryGetValue(key, out currentValue);

            content.text = label;
			drawHelper.TryGetCachedSize(label, content, EditorStyles.label, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
            drawHelper.CurrentLinePosition += (size.x + 2);

            size.x = 50;
            newValue = EditorGUI.IntField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), Convert.ToInt32(currentValue));
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (newValue != Convert.ToInt32(currentValue))
            {
                data[key] = newValue;
                SetNeedToSave(true);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawListInt(GUIContent label, int index, int value, IList intList)
    {
        try
        {
            int newValue;

			drawHelper.TryGetCachedSize(label.text, label, EditorStyles.label, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), label);
            drawHelper.CurrentLinePosition += (size.x + 2);

            size.x = 50;
            newValue = EditorGUI.IntField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), value);
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (value != newValue)
            {
                intList[index] = newValue;
                SetNeedToSave(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawFloat(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object currentValue;
            float newValue;
            string key = fieldName;

            data.TryGetValue(key, out currentValue);

            content.text = label;
			drawHelper.TryGetCachedSize(label, content, EditorStyles.label, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
            drawHelper.CurrentLinePosition += (size.x + 2);

            size.x = 50;
            newValue = EditorGUI.FloatField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), Convert.ToSingle(currentValue));
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (newValue != Convert.ToSingle(currentValue))
            {
                data[key] = newValue;
                SetNeedToSave(true);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawListFloat(GUIContent label, int index, float value, IList floatList)
    {
        try
        {
            float newValue;

			drawHelper.TryGetCachedSize(label.text, label, EditorStyles.label, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), label);
            drawHelper.CurrentLinePosition += (size.x + 2);

            size.x = 50;
            newValue = EditorGUI.FloatField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), value);
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (value != newValue)
            {
                floatList[index] = newValue;
                SetNeedToSave(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawString(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            string key = fieldName;
            object currentValue;

            data.TryGetValue(key, out currentValue);

            content.text = label;
			drawHelper.TryGetCachedSize(label, content, EditorStyles.label, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
            drawHelper.CurrentLinePosition += (size.x + 2);

            string newValue = DrawResizableTextBox(currentValue as string);

            if (newValue != (string)currentValue)
            {
                data[key] = newValue;
                SetNeedToSave(true);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual string DrawResizableTextBox(string text)
    {
        content.text = text;
        size = GUI.skin.textField.CalcSize(content);
        size.x = Math.Min(Math.Max(size.x, GDEConstants.MinTextAreaWidth), drawHelper.WidthLeftOnCurrentLine() - 62);
        size.y = Math.Max(size.y, drawHelper.StandardHeight());

        string newValue = EditorGUI.TextArea(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content.text);
        drawHelper.CurrentLinePosition += (size.x + 2);

        float tempLinePosition = drawHelper.CurrentLinePosition;
        drawHelper.NewLine(size.y/GDEConstants.LineHeight - 1);
        drawHelper.CurrentLinePosition = tempLinePosition;

        return newValue;
    }

    protected virtual void DrawListString(GUIContent label, int index, string value, IList stringList)
    {
        try
        {
			drawHelper.TryGetCachedSize(label.text, label, EditorStyles.label, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), label);
            drawHelper.CurrentLinePosition += (size.x + 2);

            string newValue = DrawResizableTextBox(value);

            if (!value.Equals(newValue))
            {
                stringList[index] = newValue;
                SetNeedToSave(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawVector2(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object temp = null;
            Dictionary<string, object> vectDict = null;
            Vector2 currentValue = Vector2.zero;
            Vector2 newValue;
            string key = fieldName;

            if (data.TryGetValue(key, out temp))
            {
                vectDict = temp as Dictionary<string, object>;
                currentValue.x = Convert.ToSingle(vectDict["x"]);
                currentValue.y = Convert.ToSingle(vectDict["y"]);
            }

            size.x = 136;
            newValue = EditorGUI.Vector2Field(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.VectorFieldHeight()), label, currentValue);
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (newValue != currentValue)
            {
                vectDict["x"] = newValue.x;
                vectDict["y"] = newValue.y;
                data[key] = vectDict;
                SetNeedToSave(true);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawListVector2(GUIContent label, int index, Dictionary<string, object> value, IList vectorList)
    {
        try
        {
            Vector2 newValue;
            Vector2 currentValue = Vector2.zero;

            currentValue.x = Convert.ToSingle(value["x"]);
            currentValue.y = Convert.ToSingle(value["y"]);

            size.x = 136;
            newValue = EditorGUI.Vector2Field(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.VectorFieldHeight()), label.text, currentValue);
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (newValue != currentValue)
            {
                value["x"] = newValue.x;
                value["y"] = newValue.y;
                vectorList[index] = value;
                SetNeedToSave(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawVector3(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object temp = null;
            Dictionary<string, object> vectDict = null;
            Vector3 currentValue = Vector3.zero;
            Vector3 newValue;
            string key = fieldName;

            if (data.TryGetValue(key, out temp))
            {
                vectDict = temp as Dictionary<string, object>;
                currentValue.x = Convert.ToSingle(vectDict["x"]);
                currentValue.y = Convert.ToSingle(vectDict["y"]);
                currentValue.z = Convert.ToSingle(vectDict["z"]);
            }

            size.x = 200;
            newValue = EditorGUI.Vector3Field(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.VectorFieldHeight()), label, currentValue);
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (newValue != currentValue)
            {
                vectDict["x"] = newValue.x;
                vectDict["y"] = newValue.y;
                vectDict["z"] = newValue.z;
                data[key] = vectDict;
                SetNeedToSave(true);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawListVector3(GUIContent label, int index, Dictionary<string, object> value, IList vectorList)
    {
        try
        {
            Vector3 newValue;
            Vector3 currentValue = Vector3.zero;

            currentValue.x = Convert.ToSingle(value["x"]);
            currentValue.y = Convert.ToSingle(value["y"]);
            currentValue.z = Convert.ToSingle(value["z"]);

            size.x = 200;
            newValue = EditorGUI.Vector3Field(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.VectorFieldHeight()), label.text, currentValue);
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (newValue != currentValue)
            {
                value["x"] = newValue.x;
                value["y"] = newValue.y;
                value["z"] = newValue.z;
                vectorList[index] = value;
                SetNeedToSave(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawVector4(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            object temp = null;
            Dictionary<string, object> vectDict = null;
            Vector4 currentValue = Vector4.zero;
            Vector4 newValue;
            string key = fieldName;

            if (data.TryGetValue(key, out temp))
            {
                vectDict = temp as Dictionary<string, object>;
                currentValue.x = Convert.ToSingle(vectDict["x"]);
                currentValue.y = Convert.ToSingle(vectDict["y"]);
                currentValue.z = Convert.ToSingle(vectDict["z"]);
                currentValue.w = Convert.ToSingle(vectDict["w"]);
            }

            size.x = 228;
            newValue = EditorGUI.Vector4Field(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.VectorFieldHeight()), label, currentValue);
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (newValue != currentValue)
            {
                vectDict["x"] = newValue.x;
                vectDict["y"] = newValue.y;
                vectDict["z"] = newValue.z;
                vectDict["w"] = newValue.w;
                data[key] = vectDict;
                SetNeedToSave(true);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawListVector4(GUIContent label, int index, Dictionary<string, object> value, IList vectorList)
    {
        try
        {
            Vector4 newValue;
            Vector4 currentValue = Vector4.zero;

            currentValue.x = Convert.ToSingle(value["x"]);
            currentValue.y = Convert.ToSingle(value["y"]);
            currentValue.z = Convert.ToSingle(value["z"]);
            currentValue.w = Convert.ToSingle(value["w"]);

            size.x = 228;
            newValue = EditorGUI.Vector4Field(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.VectorFieldHeight()), label.text, currentValue);
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (newValue != currentValue)
            {
                value["x"] = newValue.x;
                value["y"] = newValue.y;
                value["z"] = newValue.z;
                value["w"] = newValue.w;
                vectorList[index] = value;
                SetNeedToSave(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawColor(string fieldName, Dictionary<string, object> data, string label)
    {
        try
        {
            Color newValue;
            Color currentValue = Color.white;
            object temp;
			Dictionary<string, object> colorDict = new Dictionary<string, object>();

            if (data.TryGetValue(fieldName, out temp))
            {
                colorDict = temp as Dictionary<string, object>;
                colorDict.TryGetFloat("r", out currentValue.r);
                colorDict.TryGetFloat("g", out currentValue.g);
                colorDict.TryGetFloat("b", out currentValue.b);
                colorDict.TryGetFloat("a", out currentValue.a);
            }

            content.text = label;
			drawHelper.TryGetCachedSize(label, content, EditorStyles.label, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
            drawHelper.CurrentLinePosition += (size.x + 2);

            size.x = 230 - size.x;
            newValue = EditorGUI.ColorField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), currentValue);
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (newValue != currentValue)
            {
                colorDict.TryAddOrUpdateValue("r", newValue.r);
                colorDict.TryAddOrUpdateValue("g", newValue.g);
                colorDict.TryAddOrUpdateValue("b", newValue.b);
                colorDict.TryAddOrUpdateValue("a", newValue.a);

                SetNeedToSave(true);
            }
        }
        catch(Exception ex)
        {
            // Don't log ExitGUIException here. This is a unity bug with ObjectField and ColorField.
            if (!(ex is ExitGUIException))
                Debug.LogError(ex);
        }

    }

    protected virtual void DrawListColor(GUIContent label, int index, Dictionary<string, object> value, IList colorList)
    {
        try
        {
            Color newValue;
            Color currentValue = Color.white;

            value.TryGetFloat("r", out currentValue.r);
            value.TryGetFloat("g", out currentValue.g);
            value.TryGetFloat("b", out currentValue.b);
            value.TryGetFloat("a", out currentValue.a);

			drawHelper.TryGetCachedSize(label.text, label, EditorStyles.label, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), label);
            drawHelper.CurrentLinePosition += (size.x + 2);

            size.x = 230 - size.x;
            newValue = EditorGUI.ColorField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), currentValue);
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (newValue != currentValue)
            {
                value.TryAddOrUpdateValue("r", newValue.r);
                value.TryAddOrUpdateValue("g", newValue.g);
                value.TryAddOrUpdateValue("b", newValue.b);
                value.TryAddOrUpdateValue("a", newValue.a);

                colorList[index] = value;
                SetNeedToSave(true);
            }
        }
        catch (Exception ex)
        {
            // Don't log ExitGUIException here. This is a unity bug with ObjectField and ColorField.
			if (!(ex is ExitGUIException))
				Debug.LogError(ex);
        }
    }

	protected virtual void DrawAudio(string fieldKey, string fieldName, Dictionary<string, object> data, string label)
	{
		try
		{
			string key = fieldName;
			object currentValue;

			data.TryGetValue(key, out currentValue);

			content.text = label;
			drawHelper.TryGetCachedSize(label, content, EditorStyles.label, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
			drawHelper.CurrentLinePosition += (size.x + 2);

			size.x = 230f - size.x;
			AudioClip newClip = EditorGUI.ObjectField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), currentValue as AudioClip, typeof(AudioClip), false) as AudioClip;
			drawHelper.CurrentLinePosition += (size.x + 2);

			DrawAudioPreview(fieldKey, newClip);

			if (newClip != currentValue)
			{
				data[key] = newClip;
				SetNeedToSave(true);
			}
		}
		catch(Exception ex)
		{
			// Don't log ExitGUIException here. This is a unity bug with ObjectField and ColorField.
			if (!(ex is ExitGUIException))
				Debug.LogError(ex);
		}
	}

	protected virtual void DrawListAudio(string fieldKey, GUIContent label, int index, AudioClip value, IList goList)
	{
		try
		{
			drawHelper.TryGetCachedSize(label.text, label, EditorStyles.label, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), label);
			drawHelper.CurrentLinePosition += (size.x + 2);

			size.x = 230f - size.x;
			AudioClip newValue = EditorGUI.ObjectField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), value, typeof(AudioClip), false) as AudioClip;
			drawHelper.CurrentLinePosition += (size.x + 2);

			DrawAudioPreview(fieldKey, newValue);

			if (newValue != value)
			{
				goList[index] = newValue;
				SetNeedToSave(true);
			}
		}
		catch (Exception ex)
		{
			// Don't log ExitGUIException here. This is a unity bug with ObjectField and ColorField.
			if (!(ex is ExitGUIException))
				Debug.LogError(ex);
		}
	}

	protected virtual void DrawAudioPreview(string fieldKey, AudioClip clip)
	{
		if (clip != null)
		{
			bool isPlayingThisClip = false;

			if (AudioSamplerAS.isPlaying && AudioSamplerAS.clip.Equals(clip))
			{
				isPlayingThisClip = true;
				audioPreviewStyle.normal = EditorStyles.foldout.focused;
			}
			else
			{
				audioPreviewStyle.normal = EditorStyles.foldout.active;
			}

			content.text = string.Empty;
			drawHelper.TryGetCachedSize(GDEConstants.SizePreviewAudioLblKey, content, audioPreviewStyle, out size);
			if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), string.Empty, audioPreviewStyle))
			{
				if (isPlayingThisClip)
				{
					AudioSamplerAS.Stop();
				}
				else
				{
					AudioSamplerAS.clip = clip;
					AudioSamplerAS.Play();
				}
			}

			drawHelper.CurrentLinePosition += (size.x + 2);
		}
	}

	protected virtual void DrawObject<T>(string fieldKey, string fieldName, Dictionary<string, object> data, string label) where T : UnityEngine.Object
	{
		try
		{
			string key = fieldName;
			object currentValue;

			data.TryGetValue(key, out currentValue);

			content.text = label;
			drawHelper.TryGetCachedSize(label, content, EditorStyles.label, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
			drawHelper.CurrentLinePosition += (size.x + 2);

			size.x = 230f - size.x;
			T newValue = EditorGUI.ObjectField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), currentValue as T, typeof(T), false) as T;
			drawHelper.CurrentLinePosition += (size.x + 2);

			DrawPreview<T>(fieldKey, newValue);

			if (newValue != currentValue)
			{
				data[key] = newValue;
				SetNeedToSave(true);
			}
		}
		catch(Exception ex)
		{
			// Don't log ExitGUIException here. This is a unity bug with ObjectField and ColorField.
			if (!(ex is ExitGUIException))
				Debug.LogError(ex);
		}
	}

	protected virtual void DrawListObject<T>(string fieldKey, GUIContent label, int index, T value, IList goList) where T : UnityEngine.Object
	{
		try
		{
			drawHelper.TryGetCachedSize(label.text, label, EditorStyles.label, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), label);
			drawHelper.CurrentLinePosition += (size.x + 2);

			size.x = 230f - size.x;
			T newValue = EditorGUI.ObjectField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), value, typeof(T), false) as T;
			drawHelper.CurrentLinePosition += (size.x + 2);

			DrawPreview<T>(fieldKey, newValue);

			if (newValue != value)
			{
				goList[index] = newValue;
				SetNeedToSave(true);
			}
		}
		catch (Exception ex)
		{
			// Don't log ExitGUIException here. This is a unity bug with ObjectField and ColorField.
			if (!(ex is ExitGUIException))
				Debug.LogError(ex);
		}
	}

	protected virtual void DrawPreview<T>(string fieldKey, T newValue) where T : UnityEngine.Object
	{
		Texture2D preview = null;
		if (newValue != null)
			preview = AssetPreview.GetAssetPreview(newValue);

		if (preview != null)
		{
			content.text = GDEConstants.PreviewLbl;
			drawHelper.TryGetCachedSize(GDEConstants.SizePreviewLblKey, content, EditorStyles.foldout, out size);

			bool curFoldoutState = previewState.Contains(fieldKey);
			bool newFoldoutState = EditorGUI.Foldout(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), curFoldoutState, content);
			drawHelper.CurrentLinePosition += (size.x + 2);
			if (newFoldoutState != curFoldoutState)
			{
				if (newFoldoutState)
					previewState.Add(fieldKey);
				else
					previewState.Remove(fieldKey);
			}

			if (newFoldoutState)
			{
				EditorGUI.DrawPreviewTexture(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), preview.width, preview.height), preview);
				drawHelper.CurrentLinePosition += (preview.width + 2);
				drawHelper.NewLine(preview.height/drawHelper.LineHeight);
			}
		}
	}

    protected virtual void DrawCustom(string fieldName, Dictionary<string, object> data, bool canEdit, List<string> possibleValues = null)
    {
        try
        {
            object currentValue;
            int newIndex;
            int currentIndex;
            string key = fieldName;

            data.TryGetValue(key, out currentValue);

            if (canEdit && possibleValues != null)
            {
                currentIndex = possibleValues.IndexOf(currentValue as string);

                content.text = GDEConstants.ValueLbl;
				drawHelper.TryGetCachedSize(GDEConstants.SizeValueLblKey, content, EditorStyles.label, out size);
				EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
                drawHelper.CurrentLinePosition += (size.x + 2);

                size.x = 80;
                newIndex = EditorGUI.Popup(new Rect(drawHelper.CurrentLinePosition, drawHelper.PopupTop(), size.x, drawHelper.StandardHeight()), currentIndex, possibleValues.ToArray());
                drawHelper.CurrentLinePosition += (size.x + 2);

                if (newIndex != currentIndex)
                {
                    data[key] = possibleValues[newIndex];
                    SetNeedToSave(true);
                }
            }
            else
            {
                content.text = GDEConstants.DefaultValueLbl + " null";
				drawHelper.TryGetCachedSize(GDEConstants.SizeDefaultValueLblKey, content, EditorStyles.label, out size);
				EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
                drawHelper.CurrentLinePosition += (size.x + 4);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    protected virtual void DrawListCustom(GUIContent label, int index, string value, IList customList,  bool canEdit, List<string> possibleValues = null)
    {
        try
        {
            int newIndex;
            int currentIndex;

            if (canEdit && possibleValues != null)
            {
                currentIndex = possibleValues.IndexOf(value);

				drawHelper.TryGetCachedSize(label.text, label, EditorStyles.label, out size);
                EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), label);
                drawHelper.CurrentLinePosition += (size.x + 2);

                size.x = 80;
                newIndex = EditorGUI.Popup(new Rect(drawHelper.CurrentLinePosition, drawHelper.PopupTop(), size.x, drawHelper.StandardHeight()), currentIndex, possibleValues.ToArray());
                drawHelper.CurrentLinePosition += (size.x + 2);

                if (newIndex != currentIndex)
                {
                    customList[index] = possibleValues[newIndex];
                    SetNeedToSave(true);
                }
            }
            else
            {
                label.text += " null";
				drawHelper.TryGetCachedSize(label.text, label, EditorStyles.label, out size);
				EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), label);
                drawHelper.CurrentLinePosition += (size.x + 2);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

	protected virtual void HighlightNew(string content)
	{
		highlightContent = content;
		highlightStartTime = Time.realtimeSinceStartup;
		lerpColorProgress = 0f;

		newhighlightColor = GDEConstants.NewHighlightStart.ToColor32();
		highlightColorStart = GDEConstants.NewHighlightStart.ToColor32();
		highlightColorEnd = GDEConstants.NewHighlightEnd.ToColor32();
	}
    #endregion

    #region Filter/Sorting Methods
    protected virtual bool DrawFilterSection()
    {
        content.text = GDEConstants.SearchHeader;
		size = drawHelper.DrawSubHeader(content.text, headerColor, GDEConstants.SizeSearchHeaderKey, false);

		drawHelper.NewLine(.1f);
		drawHelper.CurrentLinePosition += (size.x + 4f);

        // Text search
        size.x = searchStyle.fixedWidth;
        newFilterText = EditorGUI.TextField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine()+GDEConstants.LineHeight/4f-2f, size.x, drawHelper.StandardHeight()), newFilterText, searchStyle);
        drawHelper.CurrentLinePosition += (size.x);

		if (!newFilterText.Equals(filterText))
		{
			filterText = newFilterText;
			shouldRecalculateHeights = true;
			shouldRebuildEntriesList = true;
		}

        GUIStyle cancelStyle;
		string cancelKey;
		content.text = SearchCancelContent;

		if(string.IsNullOrEmpty(newFilterText))
		{
			cancelStyle = searchCancelEmptyStyle;
			cancelKey = GDEConstants.SizeClearSearchEmptyKey;
			content.text = string.Empty;
		}
		else
		{
			cancelStyle = searchCancelStyle;
			cancelKey = GDEConstants.SizeClearSearchBtnKey;
		}

		drawHelper.TryGetCachedSize(cancelKey, content, cancelStyle, out size);
		bool clearSearch = GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine()+GDEConstants.LineHeight/4f-2f, size.x, size.y), content, cancelStyle);
        drawHelper.CurrentLinePosition += (size.x + 2);

        return clearSearch;
    }

    protected virtual int NumberOfItemsBeingShown()
    {
        if (entriesToDraw != null)
			return entriesToDraw.Count;

		return 0;
    }

    protected virtual void ClearSearch()
    {
		newFilterText = string.Empty;
        filterText = string.Empty;
        GUI.FocusControl(string.Empty);

		shouldRebuildEntriesList = true;
		shouldRecalculateHeights = true;
    }

	protected Dictionary<string, Dictionary<string, object>> GetEntriesToDraw(Dictionary<string, Dictionary<string, object>> source)
	{
		Dictionary<string, Dictionary<string, object>> entries = new Dictionary<string, Dictionary<string, object>>();

		foreach(var entry in source)
		{
			if (!ShouldFilter(entry.Key, entry.Value))
				entries.Add(entry.Key, entry.Value);
		}

		return entries;
	}
    #endregion

    #region List Helper Methods
    protected virtual void ResizeList(IList list, int size, object defaultValue)
    {
		// Remove from the end until the size matches what we want
        if (list.Count > size)
        {
			while(list.Count > size)
				list.RemoveAt(list.Count-1);

			SetNeedToSave(true);
        }
        else if (list.Count < size)
        {
            // Add entries with the default value until the size is what we want
            for (int i = list.Count; i < size; i++)
				list.Add(defaultValue.DeepCopyCollection());

            SetNeedToSave(true);
        }
    }
    #endregion

    #region Save/Load methods
    protected virtual void Load()
    {
        GDEItemManager.Load();

        entryFoldoutState.Clear();
        listFieldFoldoutState.Clear();
        currentFoldoutAllState = false;
        newListCountDict.Clear();
        filterText = string.Empty;
        groupHeights.Clear();
        groupHeightBySchema.Clear();
        editingFields.Clear();
        editFieldTextDict.Clear();

		deleteEntries.Clear();
		cloneEntries.Clear();

		drawHelper.SizeCache.Clear();
		shouldRecalculateHeights = true;
		shouldRebuildEntriesList = true;
    }

    protected virtual void Save()
    {
        GDEItemManager.Save();
    }
    #endregion

    #region Abstract methods
    protected abstract bool Create(object data);
    protected abstract void Remove(string key);
	protected abstract bool Clone(string key);

    protected abstract void DrawEntry(string key, Dictionary<string, object> data);
    protected abstract void DrawCreateSection();

    protected abstract bool ShouldFilter(string key, Dictionary<string, object> data);

    protected abstract bool NeedToSave();
    protected abstract void SetNeedToSave(bool shouldSave);

    protected abstract string FilePath();

    protected abstract float CalculateGroupHeightsTotal();
    #endregion
}
