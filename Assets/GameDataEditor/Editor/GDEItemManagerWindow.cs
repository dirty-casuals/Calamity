using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameDataEditor;

public class GDEItemManagerWindow : GDEManagerWindowBase
{
    string newItemName = string.Empty;
    int schemaIndex = 0;

    int filterSchemaIndex = 0;

    Dictionary<string, string> renamedItems = new Dictionary<string, string>();

	#region OnGUI Method
    protected override void OnGUI()
    {
        mainHeaderText = GDEConstants.GameDataHeader;

		// Set the header color
		headerColor = GDESettings.Instance.CreateDataColor.ToColor();
		headerColor.a = 1f;

		if (shouldRebuildEntriesList || entriesToDraw == null || GDEItemManager.ShouldReloadItems)
		{
			entriesToDraw = GetEntriesToDraw(GDEItemManager.AllItems);
			shouldRebuildEntriesList = false;
			shouldRecalculateHeights = true;
			GDEItemManager.ShouldReloadItems = false;
		}

		base.OnGUI();

        DrawExpandCollapseAllFoldout(GDEItemManager.AllItems.Keys.ToArray(), GDEConstants.ItemListHeader);


        float currentGroupHeightTotal = CalculateGroupHeightsTotal();
        scrollViewHeight = drawHelper.HeightToBottomOfWindow();
        scrollViewY = drawHelper.TopOfLine();
        verticalScrollbarPosition = GUI.BeginScrollView(new Rect(drawHelper.CurrentLinePosition, scrollViewY, drawHelper.FullWindowWidth(), scrollViewHeight),
                                                        verticalScrollbarPosition,
                                                        new Rect(drawHelper.CurrentLinePosition, scrollViewY, drawHelper.ScrollViewWidth(), currentGroupHeightTotal));

        int count = 0;
        foreach (KeyValuePair<string, Dictionary<string, object>> item in entriesToDraw)
        {
            float currentGroupHeight;
            groupHeights.TryGetValue(item.Key, out currentGroupHeight);

            if (currentGroupHeight == 0f ||
                (currentGroupHeight.NearlyEqual(GDEConstants.LineHeight) && entryFoldoutState.Contains(item.Key)))
            {
                string itemSchema = GDEItemManager.GetSchemaForItem(item.Key);
                if (!groupHeightBySchema.TryGetValue(itemSchema, out currentGroupHeight))
                    currentGroupHeight = GDEConstants.LineHeight;
            }

			int isVisible = drawHelper.IsVisible(verticalScrollbarPosition, scrollViewHeight, scrollViewY, currentGroupHeight);
			if (isVisible == 1)
			{
				break;
			}

            if (isVisible == 0 ||
                (count == GDEItemManager.AllItems.Count-1 && verticalScrollbarPosition.y.NearlyEqual(currentGroupHeightTotal - GDEConstants.LineHeight)))
            {
                DrawEntry(item.Key, item.Value);
            }
            else if (isVisible == -1)
            {
                drawHelper.NewLine(currentGroupHeight/GDEConstants.LineHeight);
            }

            count++;
        }
        GUI.EndScrollView();

        // Remove any items that were deleted
        foreach(string deletedkey in deleteEntries)
            Remove(deletedkey);
        deleteEntries.Clear();

        // Rename any items that were renamed
        string error;
        foreach(KeyValuePair<string, string> pair in renamedItems)
        {
            if (!GDEItemManager.RenameItem(pair.Key, pair.Value, null, out error))
                EditorUtility.DisplayDialog(GDEConstants.ErrorLbl, string.Format(GDEConstants.CouldNotRenameFormat, pair.Key, pair.Value, error), GDEConstants.OkLbl);
        }

        renamedItems.Clear();

		// Clone any items
		foreach(string itemKey in cloneEntries)
			Clone(itemKey);
		cloneEntries.Clear();
    }
    #endregion

    #region Draw Methods
    protected override void DrawCreateSection()
    {
        float topOfSection = drawHelper.TopOfLine() + 4f;
        float bottomOfSection = 0;
        float leftBoundary = drawHelper.CurrentLinePosition;

		drawHelper.DrawSubHeader(GDEConstants.CreateNewItemHeader, headerColor, GDEConstants.SizeCreateSubHeaderKey);

        size.x = 60;
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), GDEConstants.SchemaLbl);
        drawHelper.CurrentLinePosition += (size.x + 2);
        if (drawHelper.CurrentLinePosition > leftBoundary)
            leftBoundary = drawHelper.CurrentLinePosition;

        size.x = 100;
        schemaIndex = EditorGUI.Popup(new Rect(drawHelper.CurrentLinePosition, drawHelper.PopupTop(), size.x, drawHelper.StandardHeight()), schemaIndex, GDEItemManager.SchemaKeyArray);
        drawHelper.CurrentLinePosition += (size.x + 6);
        if (drawHelper.CurrentLinePosition > leftBoundary)
            leftBoundary = drawHelper.CurrentLinePosition;

        size.x = 75;
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), GDEConstants.ItemNameLbl);
        drawHelper.CurrentLinePosition += (size.x + 2);
        if (drawHelper.CurrentLinePosition > leftBoundary)
            leftBoundary = drawHelper.CurrentLinePosition;

        size.x = 180;
        newItemName = EditorGUI.TextField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), newItemName);
        drawHelper.CurrentLinePosition += (size.x + 2);
        if (drawHelper.CurrentLinePosition > leftBoundary)
            leftBoundary = drawHelper.CurrentLinePosition;

        content.text = GDEConstants.CreateNewItemBtn;
		drawHelper.TryGetCachedSize(GDEConstants.SizeCreateNewItemBtnKey, content, GUI.skin.button, out size);
		if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
        {
            if (GDEItemManager.SchemaKeyArray.IsValidIndex(schemaIndex))
            {
                List<object> args = new List<object>();
                args.Add(GDEItemManager.SchemaKeyArray[schemaIndex]);
                args.Add(newItemName);

                if (Create(args))
                {
                    newItemName = string.Empty;
                    GUI.FocusControl(string.Empty);
                }
            }
            else
                EditorUtility.DisplayDialog(GDEConstants.ErrorCreatingItem, GDEConstants.NoOrInvalidSchema, GDEConstants.OkLbl);
        }
        drawHelper.CurrentLinePosition += (size.x + 6);
        if (drawHelper.CurrentLinePosition > leftBoundary)
            leftBoundary = drawHelper.CurrentLinePosition;

        drawHelper.NewLine(1.5f);

        bottomOfSection = drawHelper.TopOfLine();

        drawHelper.DrawSectionSeparator();

        leftBoundary += 5f;

        // Draw rate box
		Vector2 forumLinkSize;
		Vector2 rateLinkSize;

		content.text = GDEConstants.ForumLinkText;
		drawHelper.TryGetCachedSize(GDEConstants.SizeForumLinkTextKey, content, linkStyle, out forumLinkSize);

		content.text = GDEConstants.RateMeText;
		drawHelper.TryGetCachedSize(GDEConstants.SizeRateMeTextKey, content, linkStyle, out rateLinkSize);

		float boxWidth = Math.Max(forumLinkSize.x, rateLinkSize.x);

		content.text = GDEConstants.ForumLinkText;
		if (GUI.Button(new Rect(leftBoundary+(boxWidth-forumLinkSize.x)/2f+5.5f, bottomOfSection-size.y-15f, forumLinkSize.x, forumLinkSize.y), content, linkStyle))
		{
			Application.OpenURL(GDEConstants.ForumURL);
		}

		content.text = GDEConstants.RateMeText;
		if(GUI.Button(new Rect(leftBoundary+(boxWidth-rateLinkSize.x)/2f+5.5f, topOfSection+15f, rateLinkSize.x, rateLinkSize.y), content, linkStyle))
		{
			Application.OpenURL(GDEConstants.RateMeURL);
		}

		DrawRateBox(leftBoundary, topOfSection, boxWidth+10f, bottomOfSection-topOfSection);
    }

    protected override bool DrawFilterSection()
    {
        bool clearSearch = base.DrawFilterSection();

		int totalItems = GDEItemManager.AllItems.Count;
        string itemText = totalItems != 1 ? GDEConstants.ItemsLbl : GDEConstants.ItemLbl;
        if (!string.IsNullOrEmpty(filterText) ||
            (GDEItemManager.FilterSchemaKeyArray.IsValidIndex(filterSchemaIndex) && !GDEItemManager.FilterSchemaKeyArray[filterSchemaIndex].Equals(GDEConstants._AllLbl)))
        {
			float pos = drawHelper.TopOfLine()+drawHelper.LineHeight*.1f;

			content.text = string.Format(GDEConstants.SearchResultFormat, NumberOfItemsBeingShown(), totalItems, itemText);
			drawHelper.TryGetCachedSize(content.text, content, EditorStyles.label, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, pos, size.x, size.y), content);
            drawHelper.CurrentLinePosition += (size.x + 2);
        }

        drawHelper.NewLine(1.25f);

        // Filter dropdown
        content.text = GDEConstants.FilterBySchemaLbl;
		drawHelper.TryGetCachedSize(GDEConstants.SizeFilterBySchemaLblKey, content, EditorStyles.label, out size);
        EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
        drawHelper.CurrentLinePosition += (size.x + 8);

        size.x = 100;
        int newIndex = EditorGUI.Popup(new Rect(drawHelper.CurrentLinePosition, drawHelper.PopupTop(), size.x, drawHelper.StandardHeight()), filterSchemaIndex, GDEItemManager.FilterSchemaKeyArray);
		if (GDEItemManager.FilterSchemaKeyArray.IsValidIndex(newIndex) && newIndex != filterSchemaIndex)
		{
			shouldRecalculateHeights = true;
			shouldRebuildEntriesList = true;
			filterSchemaIndex = newIndex;
		}

        drawHelper.NewLine(1.25f);

        return clearSearch;
    }

    protected override void DrawEntry(string itemKey, Dictionary<string, object> data)
    {
        float beginningHeight = drawHelper.CurrentHeight();
        string schemaType = "<unknown>";
        object temp;

        if (data.TryGetValue(GDMConstants.SchemaKey, out temp))
            schemaType = temp as string;

		bool currentFoldoutState = entryFoldoutState.Contains(itemKey);

        // Start drawing below
		bool isOpen = DrawFoldout(schemaType+":", itemKey, itemKey, itemKey, RenameItem);
		drawHelper.NewLine();

        if (isOpen)
        {
            bool shouldDrawSpace = false;
            bool didDrawSpaceForSection = false;
			bool isFirstSection = true;

            // Draw the basic types
            foreach(BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
            {
                List<string> fieldKeys = GDEItemManager.ItemFieldKeysOfType(itemKey, fieldType.ToString(), 0);
                foreach(string fieldKey in fieldKeys)
                {
                    drawHelper.CurrentLinePosition += GDEConstants.Indent;
                    DrawSingleField(schemaType, itemKey, fieldKey, data);
                    shouldDrawSpace = true;
					isFirstSection = false;
                }
            }

            // Draw the custom types
            foreach(string fieldKey in GDEItemManager.ItemCustomFieldKeys(itemKey, 0))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection && !isFirstSection)
                {
                    drawHelper.NewLine(0.5f);
                    didDrawSpaceForSection = true;
                }

                drawHelper.CurrentLinePosition += GDEConstants.Indent;
                DrawSingleField(schemaType, itemKey, fieldKey, data);
                shouldDrawSpace = true;
				isFirstSection = false;
            }
            didDrawSpaceForSection = false;

			// Draw the basic lists
			for(int dimension=1;  dimension <= 2;  dimension++)
			{
				foreach(BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
				{
					List<string> fieldKeys = GDEItemManager.ItemFieldKeysOfType(itemKey, fieldType.ToString(), dimension);
					foreach(string fieldKey in fieldKeys)
					{
						if (shouldDrawSpace && !didDrawSpaceForSection && !isFirstSection)
						{
							drawHelper.NewLine(0.5f);
							didDrawSpaceForSection = true;
						}

						drawHelper.CurrentLinePosition += GDEConstants.Indent;

						if (dimension == 1)
							DrawListField(schemaType, itemKey, fieldKey, data);
						else
							Draw2DListField(schemaType, itemKey, fieldKey, data);

						shouldDrawSpace = true;
						isFirstSection = false;
						didDrawSpaceForSection = true;
					}
				}
				didDrawSpaceForSection = false;
			}

            // Draw the custom lists
			for(int dimension=1;  dimension <= 2;  dimension++)
			{
	            foreach(string fieldKey in GDEItemManager.ItemCustomFieldKeys(itemKey, dimension))
	            {
					if (shouldDrawSpace && !didDrawSpaceForSection && !isFirstSection)
	                {
	                    drawHelper.NewLine(0.5f);
	                    didDrawSpaceForSection = true;
	                }

	                drawHelper.CurrentLinePosition += GDEConstants.Indent;
					if (dimension == 1)
						DrawListField(schemaType, itemKey, fieldKey, data);
					else
						Draw2DListField(schemaType, itemKey, fieldKey, data);

	                shouldDrawSpace = true;
					isFirstSection = false;
					didDrawSpaceForSection = true;
	            }
				didDrawSpaceForSection = false;
			}

            drawHelper.NewLine(0.5f);

			DrawEntryFooter(GDEConstants.CloneItem, GDEConstants.SizeCloneItemKey, itemKey);
        }
        else if (!isOpen && currentFoldoutState)
        {
            // Collapse any list foldouts as well
            List<string> listKeys = GDEItemManager.ItemListFieldKeys(itemKey);
            string foldoutKey;
            foreach(string listKey in listKeys)
            {
                foldoutKey = string.Format(GDMConstants.MetaDataFormat, itemKey, listKey);
                listFieldFoldoutState.Remove(foldoutKey);
            }
        }

        float newGroupHeight = drawHelper.CurrentHeight() - beginningHeight;
        float currentGroupHeight;

		groupHeights.TryGetValue(itemKey, out currentGroupHeight);

		if (!newGroupHeight.NearlyEqual(currentGroupHeight))
		{
			currentGroupHeightTotal -= currentGroupHeight;
			currentGroupHeightTotal += newGroupHeight;

			SetSchemaHeight(schemaType, newGroupHeight);
		}

        SetGroupHeight(itemKey, newGroupHeight);
    }

    void DrawSingleField(string schemaKey, string itemKey, string fieldKey, Dictionary<string, object> itemData)
    {
		string fieldPreviewKey = schemaKey+"_"+itemKey+"_"+fieldKey;
		string fieldType;
        itemData.TryGetString(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldKey), out fieldType);

        BasicFieldType fieldTypeEnum = BasicFieldType.Undefined;
        if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
        {
            fieldTypeEnum = (BasicFieldType)Enum.Parse(typeof(BasicFieldType), fieldType);
			fieldType = GDEItemManager.GetVariableTypeFor(fieldTypeEnum);
		}

		content.text = fieldType;
		drawHelper.TryGetCachedSize(schemaKey+fieldKey+GDEConstants.TypeSuffix, content, labelStyle, out size);
		size.x = Math.Max(size.x, GDEConstants.MinLabelWidth);

        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);
        drawHelper.CurrentLinePosition += (size.x + 2);

		content.text = fieldKey.HighlightSubstring(filterText, highlightColor);
		drawHelper.TryGetCachedSize(schemaKey+fieldKey+GDEConstants.LblSuffix, content, labelStyle, out size);
		size.x = Math.Max(size.x, GDEConstants.MinLabelWidth);

        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);
        drawHelper.CurrentLinePosition += (size.x + 2);

        switch(fieldTypeEnum)
        {
            case BasicFieldType.Bool:
            {
                DrawBool(fieldKey, itemData, GDEConstants.ValueLbl);
                drawHelper.NewLine();
                break;
            }
            case BasicFieldType.Int:
            {
                DrawInt(fieldKey, itemData, GDEConstants.ValueLbl);
                drawHelper.NewLine();
                break;
            }
            case BasicFieldType.Float:
            {
                DrawFloat(fieldKey, itemData, GDEConstants.ValueLbl);
                drawHelper.NewLine();
                break;
            }
            case BasicFieldType.String:
            {
                DrawString(fieldKey, itemData, GDEConstants.ValueLbl);
                drawHelper.NewLine();
                break;
            }
            case BasicFieldType.Vector2:
            {
				DrawVector2(fieldKey, itemData, GDEConstants.ValueLbl);
                drawHelper.NewLine(GDEConstants.VectorFieldBuffer+1);
                break;
            }
            case BasicFieldType.Vector3:
            {
				DrawVector3(fieldKey, itemData, GDEConstants.ValueLbl);
                drawHelper.NewLine(GDEConstants.VectorFieldBuffer+1);
                break;
            }
            case BasicFieldType.Vector4:
            {
				DrawVector4(fieldKey, itemData, GDEConstants.ValueLbl);
                drawHelper.NewLine(GDEConstants.VectorFieldBuffer+1);
                break;
            }
            case BasicFieldType.Color:
            {
				DrawColor(fieldKey, itemData, GDEConstants.ValueLbl);
                drawHelper.NewLine();
                break;
            }
			case BasicFieldType.GameObject:
			{
				DrawObject<GameObject>(fieldPreviewKey, fieldKey, itemData, GDEConstants.ValueLbl);
				drawHelper.NewLine();
				break;
			}
			case BasicFieldType.Texture2D:
			{
				DrawObject<Texture2D>(fieldPreviewKey, fieldKey, itemData, GDEConstants.ValueLbl);
				drawHelper.NewLine();
				break;
			}
			case BasicFieldType.Material:
			{
				DrawObject<Material>(fieldPreviewKey, fieldKey, itemData, GDEConstants.ValueLbl);
				drawHelper.NewLine();
				break;
			}
			case BasicFieldType.AudioClip:
			{
				DrawAudio(fieldPreviewKey, fieldKey, itemData, GDEConstants.ValueLbl);
				drawHelper.NewLine();
				break;
			}
            default:
            {
                List<string> itemKeys = GetPossibleCustomValues(schemaKey, fieldType);
                DrawCustom(fieldKey, itemData, true, itemKeys);
                drawHelper.NewLine();
                break;
            }
        }
    }

    void DrawListField(string schemaKey, string itemKey, string fieldKey, Dictionary<string, object> itemData)
    {
        try
        {
			string foldoutKey = string.Format(GDMConstants.MetaDataFormat, itemKey, fieldKey);
            bool newFoldoutState;
            bool currentFoldoutState = listFieldFoldoutState.Contains(foldoutKey);
            object defaultResizeValue = null;

            string fieldType;
            itemData.TryGetString(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldKey), out fieldType);

            BasicFieldType fieldTypeEnum = BasicFieldType.Undefined;
            if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
            {
                fieldTypeEnum = (BasicFieldType)Enum.Parse(typeof(BasicFieldType), fieldType);
				fieldType = GDEItemManager.GetVariableTypeFor(fieldTypeEnum);
				defaultResizeValue = GDEItemManager.GetDefaultValueForType(fieldTypeEnum);
            }

			content.text = string.Format("List<{0}>", fieldType);
			drawHelper.TryGetCachedSize(schemaKey+fieldKey+GDEConstants.TypeSuffix, content, EditorStyles.foldout, out size);
			size.x = Math.Max(size.x, GDEConstants.MinLabelWidth);
            newFoldoutState = EditorGUI.Foldout(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), currentFoldoutState, content);
            drawHelper.CurrentLinePosition += (size.x + 2);

			content.text = fieldKey.HighlightSubstring(filterText, highlightColor);
			drawHelper.TryGetCachedSize(schemaKey+fieldKey+GDEConstants.LblSuffix, content, labelStyle, out size);
			size.x = Math.Max(size.x, GDEConstants.MinLabelWidth);
            GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), fieldKey.HighlightSubstring(filterText, highlightColor), labelStyle);
            drawHelper.CurrentLinePosition += (size.x + 2);

            if (newFoldoutState != currentFoldoutState)
            {
                if (newFoldoutState)
                    listFieldFoldoutState.Add(foldoutKey);
                else
                    listFieldFoldoutState.Remove(foldoutKey);
            }

            object temp = null;
            IList list = null;

            if (itemData.TryGetValue(fieldKey, out temp))
				list = temp as IList;

            content.text = GDEConstants.SizeLbl;
			drawHelper.TryGetCachedSize(GDEConstants.SizeSizeLblKey, content, EditorStyles.label, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
            drawHelper.CurrentLinePosition += (size.x + 2);

            int newListCount;
            string listCountKey = string.Format(GDMConstants.MetaDataFormat, itemKey, fieldKey);
            if (newListCountDict.ContainsKey(listCountKey))
            {
                newListCount = newListCountDict[listCountKey];
            }
            else
            {
                newListCount = list.Count;
                newListCountDict.Add(listCountKey, newListCount);
            }

            size.x = 40;
            newListCount = EditorGUI.IntField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), newListCount);
            drawHelper.CurrentLinePosition += (size.x + 4);

            content.text = GDEConstants.ResizeBtn;
			drawHelper.TryGetCachedSize(GDEConstants.SizeResizeBtnKey, content, GUI.skin.button, out size);
			newListCountDict[listCountKey] = newListCount;
            if (newListCount != list.Count && GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
            {
                ResizeList(list, newListCount, defaultResizeValue);
                newListCountDict[listCountKey] = newListCount;
                drawHelper.CurrentLinePosition += (size.x + 2);
            }

            drawHelper.NewLine();

            if (newFoldoutState)
            {
                for (int i = 0; i < list.Count; i++)
                {
					drawHelper.CurrentLinePosition += GDEConstants.Indent*2;
					content.text = string.Format("[{0}]:", i);

                    switch (fieldTypeEnum) {
                        case BasicFieldType.Bool:
                        {
                            DrawListBool(content, i, Convert.ToBoolean(list[i]), list);
                            drawHelper.NewLine();
                            break;
                        }
                        case BasicFieldType.Int:
                        {
							DrawListInt(content, i, Convert.ToInt32(list[i]), list);
                            drawHelper.NewLine();
                            break;
                        }
                        case BasicFieldType.Float:
                        {
							DrawListFloat(content, i, Convert.ToSingle(list[i]), list);
                            drawHelper.NewLine();
                            break;
                        }
                        case BasicFieldType.String:
                        {
							DrawListString(content, i, list[i] as string, list);
                            drawHelper.NewLine();
                            break;
                        }
                        case BasicFieldType.Vector2:
                        {
							DrawListVector2(content, i, list[i] as Dictionary<string, object>, list);
                            drawHelper.NewLine(GDEConstants.VectorFieldBuffer+1);
                            break;
                        }
                        case BasicFieldType.Vector3:
                        {
							DrawListVector3(content, i, list[i] as Dictionary<string, object>, list);
                            drawHelper.NewLine(GDEConstants.VectorFieldBuffer+1);
                            break;
                        }
                        case BasicFieldType.Vector4:
                        {
							DrawListVector4(content, i, list[i] as Dictionary<string, object>, list);
                            drawHelper.NewLine(GDEConstants.VectorFieldBuffer+1);
                            break;
                        }
                        case BasicFieldType.Color:
                        {
							DrawListColor(content, i, list[i] as Dictionary<string, object>, list);
                            drawHelper.NewLine();
                            break;
                        }
						case BasicFieldType.GameObject:
						{
							DrawListObject<GameObject>(foldoutKey+i, content, i, list[i] as GameObject, list);
							drawHelper.NewLine();
							break;
						}
						case BasicFieldType.Texture2D:
						{
							DrawListObject<Texture2D>(foldoutKey+i, content, i, list[i] as Texture2D, list);
							drawHelper.NewLine();
							break;
						}
						case BasicFieldType.Material:
						{
							DrawListObject<Material>(foldoutKey+i, content, i, list[i] as Material, list);
							drawHelper.NewLine();
							break;
						}
						case BasicFieldType.AudioClip:
						{
							DrawListAudio(foldoutKey+i, content, i, list[i] as AudioClip, list);
							drawHelper.NewLine();
							break;
						}
                        default:
                        {
                            List<string> itemKeys = GetPossibleCustomValues(schemaKey, fieldType);
							DrawListCustom(content, i, list[i] as string, list, true, itemKeys);
                            drawHelper.NewLine();
                            break;
                        }
                    }
                }
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

	void Draw2DListField(string schemaKey, string itemKey, string fieldKey, Dictionary<string, object> itemData)
	{
		try
		{
			string foldoutKey = string.Format(GDMConstants.MetaDataFormat, itemKey, fieldKey);
			object defaultResizeValue;

			string fieldType;
			itemData.TryGetString(string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldKey), out fieldType);

			BasicFieldType fieldTypeEnum = BasicFieldType.Undefined;
			if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
			{
				fieldTypeEnum = (BasicFieldType)Enum.Parse(typeof(BasicFieldType), fieldType);
				fieldType = GDEItemManager.GetVariableTypeFor(fieldTypeEnum);
			}

			content.text = string.Format("List<List<{0}>>", fieldType);
			bool isOpen = DrawFoldout(content.text, foldoutKey, string.Empty, string.Empty, null);

			drawHelper.CurrentLinePosition = Math.Max(drawHelper.CurrentLinePosition, GDEConstants.MinLabelWidth+GDEConstants.Indent+4);
			content.text = fieldKey.HighlightSubstring(filterText, highlightColor);
			drawHelper.TryGetCachedSize(schemaKey+fieldKey+GDEConstants.LblSuffix, content, labelStyle, out size);

			size.x = Math.Max(size.x, GDEConstants.MinLabelWidth);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), fieldKey.HighlightSubstring(filterText, highlightColor), labelStyle);
			drawHelper.CurrentLinePosition += (size.x + 2);

			object temp = null;
			IList list = null;

			if (itemData.TryGetValue(fieldKey, out temp))
				list = temp as IList;

			content.text = GDEConstants.SizeLbl;
			drawHelper.TryGetCachedSize(GDEConstants.SizeSizeLblKey, content, EditorStyles.label, out size);
			EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
			drawHelper.CurrentLinePosition += (size.x + 2);

			int newListCount;
			string listCountKey = string.Format(GDMConstants.MetaDataFormat, itemKey, fieldKey);
			if (newListCountDict.ContainsKey(listCountKey))
			{
				newListCount = newListCountDict[listCountKey];
			}
			else
			{
				newListCount = list.Count;
				newListCountDict.Add(listCountKey, newListCount);
			}

			size.x = 40;
			newListCount = EditorGUI.IntField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), newListCount);
			drawHelper.CurrentLinePosition += (size.x + 4);

			content.text = GDEConstants.ResizeBtn;
			drawHelper.TryGetCachedSize(GDEConstants.SizeResizeBtnKey, content, GUI.skin.button, out size);
			newListCountDict[listCountKey] = newListCount;
			if (list != null && newListCount != list.Count && GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
			{
				if (GDEItemManager.IsUnityType(fieldTypeEnum))
					defaultResizeValue = Activator.CreateInstance(list.GetType().GetGenericArguments()[0]);
				else
					defaultResizeValue = new List<object>();

				ResizeList(list, newListCount, defaultResizeValue);
				newListCountDict[listCountKey] = newListCount;
				drawHelper.CurrentLinePosition += (size.x + 2);
			}

			drawHelper.NewLine();

			if (isOpen)
			{
				defaultResizeValue = GDEItemManager.GetDefaultValueForType(fieldTypeEnum);
				for (int index = 0; index < list.Count; index++)
				{
					IList subList = list[index] as IList;

					drawHelper.CurrentLinePosition += GDEConstants.Indent*2;
					content.text = string.Format("[{0}]:    List<{1}>", index, fieldType);

					isOpen = DrawFoldout(content.text, foldoutKey+"_"+index, string.Empty, string.Empty, null);
					drawHelper.CurrentLinePosition += 4;

					// Draw resize
					content.text = GDEConstants.SizeLbl;
					drawHelper.TryGetCachedSize(GDEConstants.SizeSizeLblKey, content, EditorStyles.label, out size);
					EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
					drawHelper.CurrentLinePosition += (size.x + 2);

					listCountKey = string.Format(GDMConstants.MetaDataFormat, schemaKey, fieldKey)+"_"+index;
					if (newListCountDict.ContainsKey(listCountKey))
					{
						newListCount = newListCountDict[listCountKey];
					}
					else
					{
						newListCount = subList.Count;
						newListCountDict.Add(listCountKey, newListCount);
					}

					size.x = 40;
					newListCount = EditorGUI.IntField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), newListCount);
					drawHelper.CurrentLinePosition += (size.x + 2);

					newListCountDict[listCountKey] = newListCount;

					content.text = GDEConstants.ResizeBtn;
					drawHelper.TryGetCachedSize(GDEConstants.SizeResizeBtnKey, content, GUI.skin.button, out size);
					if (newListCount != subList.Count)
					{
						if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
							ResizeList(subList, newListCount, defaultResizeValue);
						drawHelper.CurrentLinePosition += (size.x + 2);
					}

					drawHelper.NewLine();

					if (isOpen)
					{
						for (int x = 0; x < subList.Count; x++)
						{
							drawHelper.CurrentLinePosition += GDEConstants.Indent*3;
							content.text = string.Format("[{0}][{1}]:", index, x);

							switch (fieldTypeEnum)
							{
								case BasicFieldType.Bool:
								{
									DrawListBool(content, x, Convert.ToBoolean(subList[x]), subList);
									drawHelper.NewLine();
									break;
								}
								case BasicFieldType.Int:
								{
									DrawListInt(content, x, Convert.ToInt32(subList[x]), subList);
									drawHelper.NewLine();
									break;
								}
								case BasicFieldType.Float:
								{
									DrawListFloat(content, x, Convert.ToSingle(subList[x]), subList);
									drawHelper.NewLine();
									break;
								}
								case BasicFieldType.String:
								{
									DrawListString(content, x, subList[x] as string, subList);
									drawHelper.NewLine();
									break;
								}
								case BasicFieldType.Vector2:
								{
									DrawListVector2(content, x, subList[x] as Dictionary<string, object>, subList);
									drawHelper.NewLine(GDEConstants.VectorFieldBuffer+1);
									break;
								}
								case BasicFieldType.Vector3:
								{
									DrawListVector3(content, x, subList[x] as Dictionary<string, object>, subList);
									drawHelper.NewLine(GDEConstants.VectorFieldBuffer+1);
									break;
								}
								case BasicFieldType.Vector4:
								{
									DrawListVector4(content, x, subList[x] as Dictionary<string, object>, subList);
									drawHelper.NewLine(GDEConstants.VectorFieldBuffer+1);
									break;
								}
								case BasicFieldType.Color:
								{
									DrawListColor(content, x, subList[x] as Dictionary<string, object>, subList);
									drawHelper.NewLine();
									break;
								}
								case BasicFieldType.GameObject:
								{
									DrawListObject<GameObject>(foldoutKey+index+"_"+x, content, x, subList[x] as GameObject, subList);
									drawHelper.NewLine();
									break;
								}
								case BasicFieldType.Texture2D:
								{
									DrawListObject<Texture2D>(foldoutKey+index+"_"+x, content, x, subList[x] as Texture2D, subList);
									drawHelper.NewLine();
									break;
								}
								case BasicFieldType.Material:
								{
									DrawListObject<Material>(foldoutKey+index+"_"+x, content, x, subList[x] as Material, subList);
									drawHelper.NewLine();
									break;
								}
								case BasicFieldType.AudioClip:
								{
									DrawListAudio(foldoutKey+index+"_"+x, content, x, subList[x] as AudioClip, subList);
									drawHelper.NewLine();
									break;
								}
								default:
								{
									List<string> itemKeys = GetPossibleCustomValues(schemaKey, fieldType);
									DrawListCustom(content, x, subList[x] as string, subList, true, itemKeys);
									drawHelper.NewLine();
									break;
								}
							}
						}
					}
				}
			}
		}
		catch(Exception ex)
		{
			Debug.LogError(ex);
		}
	}

    List<string> GetPossibleCustomValues(string fieldKey, string fieldType)
    {
        object temp;
        List<string> itemKeys = new List<string>();
        itemKeys.Add("null");

        // Build a list of possible custom field values
        // All items that match the schema type of the custom field type
        // will be added to the selection list
        foreach(KeyValuePair<string, Dictionary<string, object>> item in GDEItemManager.AllItems)
        {
            string itemType = "<unknown>";
            Dictionary<string, object> itemData = item.Value as Dictionary<string, object>;

            if (itemData.TryGetValue(GDMConstants.SchemaKey, out temp))
                itemType = temp as string;

            if (item.Key.Equals(fieldKey) || !itemType.Equals(fieldType))
                continue;

            itemKeys.Add(item.Key);
        }

        return itemKeys;
    }
    #endregion

    #region Filter Methods
    protected override bool ShouldFilter(string itemKey, Dictionary<string, object> itemData)
    {
        if (itemData == null)
            return true;

        string schemaType = "<unknown>";
        itemData.TryGetString(GDMConstants.SchemaKey, out schemaType);

        // Return if we don't match any of the filter types
        if (GDEItemManager.FilterSchemaKeyArray.IsValidIndex(filterSchemaIndex) &&
            !GDEItemManager.FilterSchemaKeyArray[filterSchemaIndex].Equals(GDEConstants._AllLbl) &&
            !schemaType.Equals(GDEItemManager.FilterSchemaKeyArray[filterSchemaIndex]))
            return true;
		else if (!GDEItemManager.FilterSchemaKeyArray[filterSchemaIndex].Equals(GDEConstants._AllLbl) &&
		         schemaType.Equals(GDEItemManager.FilterSchemaKeyArray[filterSchemaIndex]) &&
		         string.IsNullOrEmpty(filterText))
			return false;

        bool schemaKeyMatch = schemaType.ToLower().Contains(filterText.ToLower());
        bool fieldKeyMatch = !GDEItemManager.ShouldFilterByField(schemaType, filterText);
        bool itemKeyMatch = itemKey.ToLower().Contains(filterText.ToLower());

        // Return if the schema keys don't contain the filter text or
        // if the schema fields don't contain the filter text
        if (!schemaKeyMatch && !fieldKeyMatch && !itemKeyMatch)
            return true;

        return false;
    }

    protected override void ClearSearch()
    {
        base.ClearSearch();
        filterSchemaIndex = GDEItemManager.FilterSchemaKeyArray.ToList().IndexOf(GDEConstants._AllLbl);
    }
    #endregion

    #region Load/Save/Create/Remove Item Methods
    protected override void Load()
    {
        base.Load();

        newItemName = string.Empty;
        schemaIndex = 0;
        filterSchemaIndex = 0;
        renamedItems.Clear();
    }

    protected override bool Create(object data)
    {
        bool result = true;
        List<object> args = data as List<object>;
        string schemaKey = args[0] as string;
        string itemName = args[1] as string;

        Dictionary<string, object> schemaData = null;
        if (GDEItemManager.AllSchemas.TryGetValue(schemaKey, out schemaData))
        {
            Dictionary<string, object> itemData = schemaData.DeepCopy();
            itemData.Add(GDMConstants.SchemaKey, schemaKey);

            string error;
            if (GDEItemManager.AddItem(itemName, itemData, out error))
            {
                SetFoldout(true, itemName);
                SetNeedToSave(true);

				HighlightNew(itemName);
            }
            else
            {
                result = false;
                EditorUtility.DisplayDialog(GDEConstants.ErrorCreatingItem, error, GDEConstants.OkLbl);
            }
        }
        else
        {
            result = false;
            EditorUtility.DisplayDialog(GDEConstants.ErrorLbl, GDEConstants.SchemaNotFound + ": " + schemaKey, GDEConstants.OkLbl);
        }

        return result;
    }

	protected override bool Clone(string key)
	{
		bool result = true;
		string error;
		string newKey;

		result = GDEItemManager.CloneItem(key, out newKey, out error);
		if (result)
		{
			SetNeedToSave(true);
			SetFoldout(true, newKey);

			HighlightNew(newKey);
		}
		else
		{
			EditorUtility.DisplayDialog(GDEConstants.ErrorCloningItem, error, GDEConstants.OkLbl);
			result = false;
		}

		return result;
	}

    protected override void Remove(string key)
    {
        GDEItemManager.RemoveItem(key);
        SetNeedToSave(true);
    }

    protected override bool NeedToSave()
    {
        return GDEItemManager.ItemsNeedSave;
    }

    protected override void SetNeedToSave(bool shouldSave)
    {
        GDEItemManager.ItemsNeedSave = shouldSave;
    }
    #endregion

    #region Helper Methods
    void SetSchemaHeight(string schemaKey, float groupHeight)
    {
		if (!groupHeight.NearlyEqual(drawHelper.LineHeight))
	        groupHeightBySchema[schemaKey] = groupHeight;
    }

	protected override float CalculateGroupHeightsTotal()
    {
		if (!shouldRecalculateHeights)
			return currentGroupHeightTotal;

		currentGroupHeightTotal = 0;
		float itemHeight = 0;
        float schemaHeight = 0;
        string schema = string.Empty;

        foreach(var item in entriesToDraw)
        {
            groupHeights.TryGetValue(item.Key, out itemHeight);
			if (itemHeight < GDEConstants.LineHeight)
			{
				itemHeight = GDEConstants.LineHeight;
				SetGroupHeight(item.Key, itemHeight);
			}

			//Check to see if this item's height has been updated
            //otherwise use the min height for the schema
            if (entryFoldoutState.Contains(item.Key) && itemHeight.NearlyEqual(GDEConstants.LineHeight))
            {
                schema = GDEItemManager.GetSchemaForItem(item.Key);
                groupHeightBySchema.TryGetValue(schema, out schemaHeight);

				// Only use the schema height if its greater than
				// the default item height
				if (schemaHeight > itemHeight)
				{
					currentGroupHeightTotal += schemaHeight;
					SetGroupHeight(item.Key, schemaHeight);
				}
				else
					currentGroupHeightTotal += itemHeight;
            }
            else
                currentGroupHeightTotal += itemHeight;
        }

		shouldRecalculateHeights = false;

        return currentGroupHeightTotal;
    }

    protected override string FilePath()
    {
        return GDEItemManager.DataFilePath;
    }
    #endregion

    #region Rename Methods
    protected bool RenameItem(string oldItemKey, string newItemKey, Dictionary<string, object> data, out string error)
    {
        error = string.Empty;
        renamedItems.Add(oldItemKey, newItemKey);
        return true;
    }
    #endregion
}
