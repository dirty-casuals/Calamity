using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor;

using Random = UnityEngine.Random;

public class SetDataScene : SetDataSceneBase {

	GDESetSingleData singleData;
	GDESetListData listData;
	GDESetTwoDListData twoDListData;
    
    int indexToRemove = 0;
	string newKey = "";
    
    protected override void InitGDE()
    {
        if (singleData != null && listData != null && twoDListData != null)
            return;
        
        if (!GDEDataManager.Init("set_data_scene"))
        {
            Debug.LogError(SetDataSceneStrings.ErrorInitializing);
        }
        else
        {
			var allData = GDEDataManager.DataDictionary;
			allData.TryGetCustom(GDESetSceneItemKeys.SetSingle_single_data, out singleData);
			allData.TryGetCustom(GDESetSceneItemKeys.SetList_list_data, out listData);
			allData.TryGetCustom(GDESetSceneItemKeys.SetTwoDList_twod_list_data, out twoDListData);
        }
    }

	void Awake() 
	{
		InitGDE();
	}
    
    void OnGUI()
    {
        skin = GUI.skin;
        size = Vector2.zero;
        if (content == null)
			content = new GUIContent();

        ResetToTop();
        
		DrawLabel(SetDataSceneStrings.HeaderLbl);
		DrawLabel(SetDataSceneStrings.DescriptionLbl);
		NewLine();

		if (DrawButton(SetDataSceneStrings.ResetAllBtn))
		{
			singleData.ResetAll();
			listData.ResetAll();
			twoDListData.ResetAll();
		}
        
        NewLine(2);
        
		DrawLabel(SetDataSceneStrings.SingleOrListLbl);
		NewLine();
		if (DrawButton(SetDataSceneStrings.SingleBtn))
			selectedType = DataType.Single;
		if (DrawButton(SetDataSceneStrings.ListBtn))
			selectedType = DataType.List;

		NewLine(2);

		DrawLabel(SetDataSceneStrings.TwoDListLbl);
		NewLine();
		if (DrawButton("Bool 2D"))
			selectedType = DataType.Bool2D;
		if (DrawButton("Int 2D"))
			selectedType = DataType.Int2D;
		if (DrawButton("Float 2D"))
			selectedType = DataType.Float2D;
		if (DrawButton("String 2D"))
			selectedType = DataType.String2D;
		if (DrawButton("Vec2 2D"))
			selectedType = DataType.Vec2_2D;
		if (DrawButton("Vec3 2D"))
			selectedType = DataType.Vec3_2D;
		if (DrawButton("Vec4 2D"))
			selectedType = DataType.Vec4_2D;
		if (DrawButton("Color 2D"))
			selectedType = DataType.Color_2D;
		if (DrawButton("Custom 2D"))
			selectedType = DataType.Custom_2D;
		
		NewLine(2);

		DrawLabel(SetDataSceneStrings.SetDataLbl);
		NewLine(3);
		if (selectedType.Equals(DataType.Single))
			DrawSingleData();
		else if (selectedType.Equals(DataType.List))
			DrawListData();
		else if (selectedType.Equals(DataType.Bool2D))
			DrawBoolData();
		else if (selectedType.Equals(DataType.Int2D))
			DrawIntData();
		else if (selectedType.Equals(DataType.Float2D))
			DrawFloatData();
		else if (selectedType.Equals(DataType.String2D))
			DrawStringData();
		else if (selectedType.Equals(DataType.Vec2_2D))
			DrawVector2Data();
		else if (selectedType.Equals(DataType.Vec3_2D))
			DrawVector3Data();
		else if (selectedType.Equals(DataType.Vec4_2D))
			DrawVector4Data();
		else if (selectedType.Equals(DataType.Color_2D))
			DrawColorData();
		else if (selectedType.Equals(DataType.Custom_2D))
			DrawCustomData();
    }

	void DrawSingleData()
	{
		if (singleData != null)
		{   
			Indent();

			// Draw string_field
			DrawLabel("string_field:");
			singleData.string_field = DrawString(singleData.string_field);
			Indent();
			if (DrawButton(SetDataSceneStrings.Reset))
				singleData.Reset_string_field();
			
			NewLine();
			Indent();
			
			// Draw bool_field
			DrawLabel("bool_field:");
			singleData.bool_field = DrawBool(singleData.bool_field);
			Indent();
			if (DrawButton(SetDataSceneStrings.Reset))
				singleData.Reset_bool_field();
			
			NewLine();
			Indent();
			
			// Draw float_field
			DrawLabel("float_field:");
			singleData.float_field = DrawFloat(singleData.float_field);
			Indent();
			if (DrawButton(SetDataSceneStrings.Reset))
				singleData.Reset_float_field();
			
			NewLine();
			Indent();
			
			// Draw int_field
			DrawLabel("int_field:");
			singleData.int_field = DrawInt(singleData.int_field);
			Indent();
			if (DrawButton(SetDataSceneStrings.Reset))
				singleData.Reset_int_field();
			
			NewLine();
			Indent();
			
			// Draw vector2_field
			DrawLabel("vector2_field:");
			singleData.vector2_field = DrawVector2(singleData.vector2_field);
			Indent();
			if (DrawButton(SetDataSceneStrings.Reset))
				singleData.Reset_vector2_field();
			
			NewLine();
			Indent();
			
			// Draw vector3_field
			DrawLabel("vector3_field:");
			singleData.vector3_field = DrawVector3(singleData.vector3_field);
			Indent();
			if (DrawButton(SetDataSceneStrings.Reset))
				singleData.Reset_vector3_field();
			
			NewLine();
			Indent();
			
			// Draw vector4_field
			DrawLabel("vector4_field:");
			singleData.vector4_field = DrawVector4(singleData.vector4_field);
			Indent();
			if (DrawButton(SetDataSceneStrings.Reset))
				singleData.Reset_vector4_field();
			
			NewLine();
			Indent();
			
			
			// Draw custom_field
			DrawLabel("custom_field");
			
			Indent();
			if (DrawButton(SetDataSceneStrings.Reset))
				singleData.Reset_custom_field();
			
			DrawLabel(singleData.custom_field.Key+":");
			
			string oldKey = singleData.custom_field.Key;
			newKey = DrawString(newKey);
			
			singleData.custom_field.description = DrawString(singleData.custom_field.description);
			if (oldKey != newKey && DrawButton(SetDataSceneStrings.SetRefBtn))
			{
				GDESetCustomData newCustomRef;
				GDEDataManager.DataDictionary.TryGetCustom(newKey, out newCustomRef);
				singleData.custom_field = newCustomRef;
			}
		}
	}

	void DrawListData()
	{
		if (listData != null)
		{
			Indent();

			// Draw bool_list_field
			bool needsSave = false;
			DrawLabel("bool_list_field:");
			Indent();
			
			if (DrawButton(SetDataSceneStrings.Reset))
				listData.Reset_bool_list_field();
			if (DrawButton(SetDataSceneStrings.AddNewBool))
			{
				listData.bool_list_field.Add(false);
				listData.Set_bool_list_field();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				listData.bool_list_field.RemoveAt(indexToRemove);
				listData.Set_bool_list_field();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<listData.bool_list_field.Count;  i++)
			{
				if (i > 0)
					DrawLabel("|");
				
				bool oldVal = listData.bool_list_field[i];
				listData.bool_list_field[i] = DrawBool(listData.bool_list_field[i]);
				
				needsSave = (listData.bool_list_field[i] != oldVal) | needsSave;
			}
			if (needsSave)
				listData.Set_bool_list_field();
			
			NewLine(2);
			Indent();
			
			// Draw float_list_field
			needsSave = false;
			DrawLabel("float_list_field:");
			Indent();
			
			if (DrawButton(SetDataSceneStrings.Reset))
				listData.Reset_float_list_field();
			if (DrawButton(SetDataSceneStrings.AddNewFloat))
			{
				listData.float_list_field.Add(0);
				listData.Set_float_list_field();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				listData.float_list_field.RemoveAt(indexToRemove);
				listData.Set_float_list_field();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<listData.float_list_field.Count;  i++)
			{
				if (i > 0)
					DrawLabel("|");
				
				float oldVal = listData.float_list_field[i];
				listData.float_list_field[i] = DrawFloat(listData.float_list_field[i]);
				
				needsSave = (listData.float_list_field[i] != oldVal) | needsSave;
			}
			if (needsSave)
				listData.Set_float_list_field();
			
			NewLine(2);
			Indent();
			
			// Draw int_list_field
			needsSave = false;
			DrawLabel("int_list_field:");
			Indent();
			
			if (DrawButton(SetDataSceneStrings.Reset))
				listData.Reset_int_list_field();
			if (DrawButton(SetDataSceneStrings.AddNewInt))
			{
				listData.int_list_field.Add(0);
				listData.Set_int_list_field();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				listData.int_list_field.RemoveAt(indexToRemove);
				listData.Set_int_list_field();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<listData.int_list_field.Count;  i++)
			{
				if (i > 0)
					DrawLabel("|");
				
				int oldVal = listData.int_list_field[i];
				listData.int_list_field[i] = DrawInt(listData.int_list_field[i]);
				
				needsSave = (listData.int_list_field[i] != oldVal) | needsSave;
			}
			if (needsSave)
				listData.Set_int_list_field();
			
			NewLine(2);
			Indent();
			
			// Draw string_list_field
			needsSave = false;
			DrawLabel("string_list_field:");
			Indent();
			
			if (DrawButton(SetDataSceneStrings.Reset))
				listData.Reset_string_list_field();
			if (DrawButton(SetDataSceneStrings.AddNewString))
			{
				listData.string_list_field.Add(string.Empty);
				listData.Set_string_list_field();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				listData.string_list_field.RemoveAt(indexToRemove);
				listData.Set_string_list_field();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<listData.string_list_field.Count;  i++)
			{
				if (i > 0)
				{
					NewLine();
					Indent(2);
				}
				
				string oldVal = listData.string_list_field[i];
				listData.string_list_field[i] = DrawString(listData.string_list_field[i]);
				
				needsSave = (listData.string_list_field[i] != oldVal) | needsSave;
			}
			if (needsSave)
				listData.Set_string_list_field();
			
			NewLine(2);
			Indent();
			
			// Draw vector2_list_field
			needsSave = false;
			DrawLabel("vector2_list_field:");
			Indent();
			
			if (DrawButton(SetDataSceneStrings.Reset))
				listData.Reset_vector2_list_field();
			if (DrawButton(SetDataSceneStrings.AddNewVec2))
			{
				listData.vector2_list_field.Add(Vector2.zero);
				listData.Set_vector2_list_field();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				listData.vector2_list_field.RemoveAt(indexToRemove);
				listData.Set_vector2_list_field();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<listData.vector2_list_field.Count;  i++)
			{
				if (i > 0)
				{
					NewLine();
					Indent(2);
				}
				
				Vector2 oldVal = listData.vector2_list_field[i];
				listData.vector2_list_field[i] = DrawVector2(listData.vector2_list_field[i]);
				
				needsSave = (listData.vector2_list_field[i] != oldVal) | needsSave;
			}
			if (needsSave)
				listData.Set_vector2_list_field();
			
			NewLine(2);
			Indent();
			
			// Draw vector3_list_field
			needsSave = false;
			DrawLabel("vector3_list_field:");
			Indent();
			
			if (DrawButton(SetDataSceneStrings.Reset))
				listData.Reset_vector3_list_field();
			if (DrawButton(SetDataSceneStrings.AddNewVec3))
			{
				listData.vector3_list_field.Add(Vector3.zero);
				listData.Set_vector3_list_field();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				listData.vector3_list_field.RemoveAt(indexToRemove);
				listData.Set_vector3_list_field();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<listData.vector3_list_field.Count;  i++)
			{
				if (i > 0)
				{
					NewLine();
					Indent(2);
				}
				
				Vector3 oldVal = listData.vector3_list_field[i];
				listData.vector3_list_field[i] = DrawVector3(listData.vector3_list_field[i]);
				
				needsSave = (listData.vector3_list_field[i] != oldVal) | needsSave;
			}
			if (needsSave)
				listData.Set_vector3_list_field();
			
			NewLine(2);
			Indent();
			
			// Draw vector4_list_field
			needsSave = false;
			DrawLabel("vector4_list_field:");
			Indent();
			
			if (DrawButton(SetDataSceneStrings.Reset))
				listData.Reset_vector4_list_field();
			if (DrawButton(SetDataSceneStrings.AddNewVec4))
			{
				listData.vector4_list_field.Add(Vector4.zero);
				listData.Set_vector4_list_field();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				listData.vector4_list_field.RemoveAt(indexToRemove);
				listData.Set_vector4_list_field();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<listData.vector4_list_field.Count;  i++)
			{
				if (i > 0)
				{
					NewLine();
					Indent(2);
				}
				
				Vector4 oldVal = listData.vector4_list_field[i];
				listData.vector4_list_field[i] = DrawVector4(listData.vector4_list_field[i]);
				
				needsSave = (listData.vector4_list_field[i] != oldVal) | needsSave;
			}
			if (needsSave)
				listData.Set_vector4_list_field();
			
			NewLine(2);
			Indent();
			
			// Draw custom_list_field
			needsSave = false;
			DrawLabel("custom_list_field:");
			Indent();
			
			if (DrawButton(SetDataSceneStrings.Reset))
				listData.Reset_custom_list_field();
			if (DrawButton(SetDataSceneStrings.AddNewCustom))
			{
				GDESetCustomData newCustom = new GDESetCustomData(Random.value.ToString());
				newCustom.description = SetDataSceneStrings.KeyLbl+" \""+newCustom.Key+"\"";
				
				listData.custom_list_field.Add(newCustom);
				listData.Set_custom_list_field();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				listData.custom_list_field.RemoveAt(indexToRemove);
				listData.Set_custom_list_field();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<listData.custom_list_field.Count;  i++)
			{
				if (i > 0)
				{
					NewLine();
					Indent(2);
				}
				
				DrawLabel(listData.custom_list_field[i].Key+":");
				
				string oldKey = listData.custom_list_field[i].Key;
				newKey = DrawString(newKey);
				
				string oldVal = listData.custom_list_field[i].description;
				listData.custom_list_field[i].description = DrawString(listData.custom_list_field[i].description);
				
				if (oldKey != newKey && DrawButton(SetDataSceneStrings.SetRefBtn))
				{
					GDESetCustomData newCustomRef;
					GDEDataManager.DataDictionary.TryGetCustom(newKey, out newCustomRef);
					listData.custom_list_field[i] = newCustomRef;
					
					needsSave = true;
				}
				
				needsSave = (listData.custom_list_field[i].description != oldVal) | needsSave;
			}
			
			if (needsSave)
				listData.Set_custom_list_field();
		}
	}

	void DrawBoolData()
	{
		bool needsSave = false;
		DrawLabel("2D Bool List");
		
		NewLine();
		Indent();
		
		if (DrawButton(SetDataSceneStrings.AddNewList))
			twoDListData.bool_2dlist.Add(new List<bool>());
		
		indexToRemove = DrawInt(indexToRemove);
		if (DrawButton(SetDataSceneStrings.RemoveList))
		{
			twoDListData.bool_2dlist.RemoveAt(indexToRemove);
			twoDListData.Set_bool_2dlist();
		}
		
		if (DrawButton(SetDataSceneStrings.Reset))
			twoDListData.Reset_bool_2dlist();
		
		NewLine(2);
		Indent();
		
		foreach(var subList in twoDListData.bool_2dlist)
		{
			// Draw bool_list_field
			if (DrawButton(SetDataSceneStrings.AddNewBool))
			{
				subList.Add(false);
				twoDListData.Set_bool_2dlist();
			}
			
			Indent();
			
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				subList.RemoveAt(indexToRemove);
				twoDListData.Set_bool_2dlist();
			}
			
			NewLine();
			Indent(2);
			
			for(var i=0;  i<subList.Count;  i++)
			{
				if (i > 0)
					DrawLabel("|");
				
				bool oldVal = subList[i];
				subList[i] = DrawBool(subList[i]);
				
				needsSave = (subList[i] != oldVal) | needsSave;
			}
			
			NewLine(2);
			Indent();
		}
		
		if (needsSave)
			twoDListData.Set_bool_2dlist();
	}
	
	void DrawIntData()
	{
		bool needsSave = false;
		DrawLabel("2D Int List");
		
		NewLine();
		Indent();
		
		if (DrawButton(SetDataSceneStrings.AddNewList))
			twoDListData.int_2dlist.Add(new List<int>());
		
		indexToRemove = DrawInt(indexToRemove);
		if (DrawButton(SetDataSceneStrings.RemoveList))
		{
			twoDListData.int_2dlist.RemoveAt(indexToRemove);
			twoDListData.Set_int_2dlist();
		}
		
		if (DrawButton(SetDataSceneStrings.Reset))
			twoDListData.Reset_int_2dlist();
		
		NewLine(2);
		Indent();
		
		foreach(var subList in twoDListData.int_2dlist)
		{
			// Draw int_list_field
			if (DrawButton(SetDataSceneStrings.AddNewInt))
			{
				subList.Add(0);
				twoDListData.Set_int_2dlist();
			}
			
			Indent();
			
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				subList.RemoveAt(indexToRemove);
				twoDListData.Set_int_2dlist();
			}
			
			NewLine();
			Indent(2);
			
			for(var i=0;  i<subList.Count;  i++)
			{
				if (i > 0)
					DrawLabel("|");
				
				int oldVal = subList[i];
				subList[i] = DrawInt(subList[i]);
				
				needsSave = (subList[i] != oldVal) | needsSave;
			}
			
			NewLine(2);
			Indent();
		}
		
		if (needsSave)
			twoDListData.Set_int_2dlist();
	}
	
	void DrawFloatData()
	{
		bool needsSave = false;
		DrawLabel("2D Float List");
		
		NewLine();
		Indent();
		
		if (DrawButton(SetDataSceneStrings.AddNewList))
			twoDListData.float_2dlist.Add(new List<float>());
		
		indexToRemove = DrawInt(indexToRemove);
		if (DrawButton(SetDataSceneStrings.RemoveList))
		{
			twoDListData.float_2dlist.RemoveAt(indexToRemove);
			twoDListData.Set_float_2dlist();
		}
		
		if (DrawButton(SetDataSceneStrings.Reset))
			twoDListData.Reset_float_2dlist();
		
		NewLine(2);
		Indent();
		
		foreach(var subList in twoDListData.float_2dlist)
		{
			// Draw float_list_field
			if (DrawButton(SetDataSceneStrings.AddNewFloat))
			{
				subList.Add(0);
				twoDListData.Set_float_2dlist();
			}
			
			Indent();
			
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				subList.RemoveAt(indexToRemove);
				twoDListData.Set_float_2dlist();
			}
			
			NewLine();
			Indent(2);
			
			for(var i=0;  i<subList.Count;  i++)
			{
				if (i > 0)
					DrawLabel("|");
				
				float oldVal = subList[i];
				subList[i] = DrawFloat(subList[i]);
				
				needsSave = (subList[i] != oldVal) | needsSave;
			}
			
			NewLine(2);
			Indent();
		}
		
		if (needsSave)
			twoDListData.Set_float_2dlist();
	}
	
	void DrawStringData()
	{
		bool needsSave = false;
		DrawLabel("2D String List");
		
		NewLine();
		Indent();
		
		if (DrawButton(SetDataSceneStrings.AddNewList))
			twoDListData.string_2dlist.Add(new List<string>());
		
		indexToRemove = DrawInt(indexToRemove);
		if (DrawButton(SetDataSceneStrings.RemoveList))
		{
			twoDListData.string_2dlist.RemoveAt(indexToRemove);
			twoDListData.Set_string_2dlist();
		}
		
		if (DrawButton(SetDataSceneStrings.Reset))
			twoDListData.Reset_string_2dlist();
		
		NewLine(2);
		Indent();
		
		foreach(var subList in twoDListData.string_2dlist)
		{
			if (DrawButton(SetDataSceneStrings.AddNewString))
			{
				subList.Add(string.Empty);
				twoDListData.Set_string_2dlist();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				subList.RemoveAt(indexToRemove);
				twoDListData.Set_string_2dlist();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<subList.Count;  i++)
			{
				if (i > 0)
				{
					NewLine();
					Indent(2);
				}
				
				string oldVal = subList[i];
				subList[i] = DrawString(subList[i]);
				
				needsSave = (subList[i] != oldVal) | needsSave;
			}
			
			NewLine(2);
			Indent();
		}
		
		if (needsSave)
			twoDListData.Set_string_2dlist();
	}
	
	void DrawVector2Data()
	{
		bool needsSave = false;
		DrawLabel("2D Vector2 List");
		
		NewLine();
		Indent();
		
		if (DrawButton(SetDataSceneStrings.AddNewList))
			twoDListData.vector2_2dlist.Add(new List<Vector2>());
		
		indexToRemove = DrawInt(indexToRemove);
		if (DrawButton(SetDataSceneStrings.RemoveList))
		{
			twoDListData.vector2_2dlist.RemoveAt(indexToRemove);
			twoDListData.Set_vector2_2dlist();
		}
		
		if (DrawButton(SetDataSceneStrings.Reset))
			twoDListData.Reset_vector2_2dlist();

		NewLine(2);
		Indent();
		
		foreach(var subList in twoDListData.vector2_2dlist)
		{
			if (DrawButton(SetDataSceneStrings.AddNewVec2))
			{
				subList.Add(Vector2.zero);
				twoDListData.Set_vector2_2dlist();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				subList.RemoveAt(indexToRemove);
				twoDListData.Set_vector2_2dlist();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<subList.Count;  i++)
			{
				if (i > 0)
				{
					NewLine();
					Indent(2);
				}
				
				Vector2 oldVal = subList[i];
				subList[i] = DrawVector2(subList[i]);
				
				needsSave = (subList[i] != oldVal) | needsSave;
			}
			
			NewLine(2);
			Indent();
		}
		
		if (needsSave)
			twoDListData.Set_vector2_2dlist();
	}
	
	void DrawVector3Data()
	{
		bool needsSave = false;
		DrawLabel("2D Vector3 List");
		
		NewLine();
		Indent();
		
		if (DrawButton(SetDataSceneStrings.AddNewList))
			twoDListData.vector3_2dlist.Add(new List<Vector3>());
		
		indexToRemove = DrawInt(indexToRemove);
		if (DrawButton(SetDataSceneStrings.RemoveList))
		{
			twoDListData.vector3_2dlist.RemoveAt(indexToRemove);
			twoDListData.Set_vector3_2dlist();
		}
		
		if (DrawButton(SetDataSceneStrings.Reset))
			twoDListData.Reset_vector3_2dlist();
		
		NewLine(2);
		Indent();
		
		foreach(var subList in twoDListData.vector3_2dlist)
		{
			if (DrawButton(SetDataSceneStrings.AddNewVec3))
			{
				subList.Add(Vector3.zero);
				twoDListData.Set_vector3_2dlist();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				subList.RemoveAt(indexToRemove);
				twoDListData.Set_vector3_2dlist();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<subList.Count;  i++)
			{
				if (i > 0)
				{
					NewLine();
					Indent(2);
				}
				
				Vector3 oldVal = subList[i];
				subList[i] = DrawVector3(subList[i]);
				
				needsSave = (subList[i] != oldVal) | needsSave;
			}
			
			NewLine(2);
			Indent();
		}
		
		if (needsSave)
			twoDListData.Set_vector3_2dlist();
	}
	
	void DrawVector4Data()
	{
		bool needsSave = false;
		DrawLabel("2D Vector4 List");
		
		NewLine();
		Indent();
		
		if (DrawButton(SetDataSceneStrings.AddNewList))
			twoDListData.vector4_2dlist.Add(new List<Vector4>());
		
		indexToRemove = DrawInt(indexToRemove);
		if (DrawButton(SetDataSceneStrings.RemoveList))
		{
			twoDListData.vector4_2dlist.RemoveAt(indexToRemove);
			twoDListData.Set_vector4_2dlist();
		}
		
		if (DrawButton(SetDataSceneStrings.Reset))
			twoDListData.Reset_vector4_2dlist();
		
		NewLine(2);
		Indent();
		
		foreach(var subList in twoDListData.vector4_2dlist)
		{
			if (DrawButton(SetDataSceneStrings.AddNewVec4))
			{
				subList.Add(Vector4.zero);
				twoDListData.Set_vector4_2dlist();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				subList.RemoveAt(indexToRemove);
				twoDListData.Set_vector4_2dlist();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<subList.Count;  i++)
			{
				if (i > 0)
				{
					NewLine();
					Indent(2);
				}
				
				Vector4 oldVal = subList[i];
				subList[i] = DrawVector4(subList[i]);
				
				needsSave = (subList[i] != oldVal) | needsSave;
			}
			
			NewLine(2);
			Indent();
		}
		
		if (needsSave)
			twoDListData.Set_vector4_2dlist();
	}
	
	void DrawColorData()
	{
		bool needsSave = false;
		DrawLabel("2D Color List");
		
		NewLine();
		Indent();
		
		if (DrawButton(SetDataSceneStrings.AddNewList))
			twoDListData.color_2dlist.Add(new List<Color>());
		
		indexToRemove = DrawInt(indexToRemove);
		if (DrawButton(SetDataSceneStrings.RemoveList))
		{
			twoDListData.color_2dlist.RemoveAt(indexToRemove);
			twoDListData.Set_color_2dlist();
		}
		
		if (DrawButton(SetDataSceneStrings.Reset))
			twoDListData.Reset_color_2dlist();
		
		NewLine(2);
		Indent();
		
		foreach(var subList in twoDListData.color_2dlist)
		{
			if (DrawButton(SetDataSceneStrings.AddNewColor))
			{
				subList.Add(Color.black);
				twoDListData.Set_color_2dlist();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				subList.RemoveAt(indexToRemove);
				twoDListData.Set_color_2dlist();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<subList.Count;  i++)
			{
				if (i > 0)
				{
					NewLine();
					Indent(2);
				}
				
				Color oldVal = subList[i];
				subList[i] = DrawColor(subList[i]);
				
				needsSave = (subList[i] != oldVal) | needsSave;
			}
			
			NewLine(2);
			Indent();
		}
		
		if (needsSave)
			twoDListData.Set_color_2dlist();
	}
	
	void DrawCustomData()
	{
		bool needsSave = false;
		DrawLabel("2D Custom List");
		
		NewLine();
		Indent();
		
		if (DrawButton(SetDataSceneStrings.AddNewList))
			twoDListData.custom_2dlist.Add(new List<GDESetCustomData>());
		
		indexToRemove = DrawInt(indexToRemove);
		if (DrawButton(SetDataSceneStrings.RemoveList))
		{
			twoDListData.custom_2dlist.RemoveAt(indexToRemove);
			twoDListData.Set_custom_2dlist();
		}
		
		if (DrawButton(SetDataSceneStrings.Reset))
			twoDListData.Reset_custom_2dlist();
		
		NewLine(2);
		Indent();

		foreach(var subList in twoDListData.custom_2dlist)
		{
			if (DrawButton(SetDataSceneStrings.AddNewCustom))
			{
				GDESetCustomData newCustom = new GDESetCustomData(Random.value.ToString());
				newCustom.description = SetDataSceneStrings.KeyLbl+" \""+newCustom.Key+"\"";
				
				subList.Add(newCustom);
				twoDListData.Set_custom_2dlist();
			}
			Indent();
			indexToRemove = DrawInt(indexToRemove);
			if (DrawButton(SetDataSceneStrings.RemoveIndexLbl))
			{
				subList.RemoveAt(indexToRemove);
				twoDListData.Set_custom_2dlist();
			}
			
			NewLine();
			Indent(2);
			
			for(int i=0;  i<subList.Count;  i++)
			{
				if (i > 0)
				{
					NewLine();
					Indent(2);
				}
				
				DrawLabel(subList[i].Key+":");
				
				string oldKey = subList[i].Key;
				newKey = DrawString(newKey);
				
				string oldVal = subList[i].description;
				subList[i].description = DrawString(subList[i].description);
				
				if (oldKey != newKey && DrawButton(SetDataSceneStrings.SetRefBtn))
				{
					GDESetCustomData newCustomRef;
					GDEDataManager.DataDictionary.TryGetCustom(newKey, out newCustomRef);
					subList[i] = newCustomRef;
					
					needsSave = true;
				}
				
				needsSave = (subList[i].description != oldVal) | needsSave;
			}
			
			NewLine(2);
			Indent();
		}
		
		if (needsSave)
			twoDListData.Set_custom_2dlist();
	}
}