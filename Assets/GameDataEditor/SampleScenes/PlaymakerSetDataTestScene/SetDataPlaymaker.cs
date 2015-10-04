#if GDE_PLAYMAKER_SUPPORT
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor;
using HutongGames.PlayMaker;

using Random = UnityEngine.Random;

public class SetDataPlaymaker : SetDataSceneBase {

	FsmBool isGDEInitialized;

	FsmString single_string;
	FsmString single_custom_string;
	FsmString single_custom;

	FsmBool single_bool;
	FsmBool single_custom_bool;

	FsmInt single_int;
	FsmInt single_custom_int;

	FsmColor single_color;
	FsmColor single_custom_color;

	FsmVector2 single_vec2;
	FsmVector2 single_custom_vec2;

	FsmVector3 single_vec3;
	FsmVector3 single_custom_vec3;

	FsmFloat single_float;
	FsmFloat single_custom_float;

	protected override void InitGDE()
	{
		FsmVariables vars = PlayMakerGlobals.Instance.Variables;
		if (isGDEInitialized == null)
			isGDEInitialized = vars.GetVariable("gde_initialized") as FsmBool;

		if (isGDEInitialized != null && isGDEInitialized.Value == true)
		{
			single_bool = vars.GetVariable("single_bool") as FsmBool;
			single_custom_bool = vars.GetVariable("single_custom_bool") as FsmBool;

			single_float = vars.GetVariable("single_float") as FsmFloat;
			single_custom_float = vars.GetVariable("single_custom_float") as FsmFloat;

			single_int = vars.GetVariable("single_int") as FsmInt;
			single_custom_int = vars.GetVariable("single_custom_int") as FsmInt;

			single_string = vars.GetVariable("single_string") as FsmString;
			single_custom_string = vars.GetVariable("single_custom_string") as FsmString;
			single_custom = vars.GetVariable("single_custom") as FsmString;

			single_vec2 = vars.GetVariable("single_vector2") as FsmVector2;
			single_custom_vec2 = vars.GetVariable("single_custom_vector2") as FsmVector2;

			single_vec3 = vars.GetVariable("single_vector3") as FsmVector3;
			single_custom_vec3 = vars.GetVariable("single_custom_vector3") as FsmVector3;

			single_color = vars.GetVariable("single_color") as FsmColor;
			single_custom_color = vars.GetVariable("single_custom_color") as FsmColor;
		}
	}
	
	void OnGUI()
	{
		skin = GUI.skin;
		size = Vector2.zero;
		if (content == null)
			content = new GUIContent();
		
		ResetToTop();
		
		DrawLabel(SetDataPlaymakerStrings.HeaderLbl);
		if (DrawButton(SetDataPlaymakerStrings.ResetAll))
			PlayMakerFSM.BroadcastEvent("ResetAll");
		
		NewLine();
		
		if (DrawButton(SetDataPlaymakerStrings.SingleDataLbl))
			selectedType = DataType.Single;
		if (DrawButton(SetDataPlaymakerStrings.ListDataLbl))
			selectedType = DataType.List;
		
		NewLine(2);
		
		if (selectedType.Equals(DataType.Single))
			DrawSingleData();
		else if (selectedType.Equals(DataType.List))
			DrawListData();
	}
	
	void DrawSingleData()
	{
		if (isGDEInitialized == null || isGDEInitialized.Value == false)
			return;

		Indent();
		
		// Draw string_field
		DrawSingleString("string_field:", single_string, "ResetSingleString", "SetSingleString");
		
		NewLine();
		Indent();
		
		// Draw bool_field
		DrawSingleBool("bool_field:", single_bool, "ResetSingleBool", "SetSingleBool");

		NewLine();
		Indent();
		
		// Draw float_field
		DrawSingleFloat("float_field:", single_float, "ResetSingleFloat", "SetSingleFloat");

		NewLine();
		Indent();
		
		// Draw int_field
		DrawSingleInt("int_field:", single_int, "ResetSingleInt", "SetSingleInt");
		
		NewLine();
		Indent();
		
		// Draw vector2_field
		DrawSingleVec2("vector2_field:", single_vec2, "ResetSingleVector2", "SetSingleVector2");
		
		NewLine();
		Indent();
		
		// Draw vector3_field
		DrawSingleVec3("vector3_field:", single_vec3, "ResetSingleVector3", "SetSingleVector3");
		
		NewLine();
		Indent();

		// Draw color_field
		DrawSingleColor("color_field:", single_color, "ResetSingleColor", "SetSingleColor");
		
		NewLine(3);
		Indent();
		
		// Draw custom fields
		DrawLabel("Custom Fields:");

		NewLine();
		Indent();

		DrawSingleString("Custom ID:", single_custom, "ResetSingleCustom", "SetSingleCustom");

		NewLine(2);
		Indent();

		DrawSingleString("custom_string:", single_custom_string, "ResetCustomSingleString", "SetCustomSingleString");

		NewLine();
		Indent();
		
		// Draw bool_field
		DrawSingleBool("custom_bool:", single_custom_bool, "ResetCustomSingleBool", "SetCustomSingleBool");
		
		NewLine();
		Indent();
		
		// Draw float_field
		DrawSingleFloat("float_field:", single_custom_float, "ResetCustomSingleFloat", "SetCustomSingleFloat");
		
		NewLine();
		Indent();
		
		// Draw int_field
		DrawSingleInt("int_field:", single_custom_int, "ResetCustomSingleInt", "SetCustomSingleInt");
		
		NewLine();
		Indent();
		
		// Draw vector2_field
		DrawSingleVec2("vector2_field:", single_custom_vec2, "ResetCustomSingleVector2", "SetCustomSingleVector2");
		
		NewLine();
		Indent();
		
		// Draw vector3_field
		DrawSingleVec3("vector3_field:", single_custom_vec3, "ResetCustomSingleVector3", "SetCustomSingleVector3");
		
		NewLine();
		Indent();
		
		// Draw color_field
		DrawSingleColor("color_field:", single_custom_color, "ResetCustomSingleColor", "SetCustomSingleColor");
	}

	void DrawSingleString(string label, FsmString val, string resetEvent, string setEvent)
	{
		DrawLabel(label);
		val.Value = DrawString(val.Value);

		Indent();

		if (DrawButton(SetDataPlaymakerStrings.SetLbl))
			PlayMakerFSM.BroadcastEvent(setEvent);

		if (DrawButton(SetDataSceneStrings.Reset))
			PlayMakerFSM.BroadcastEvent(resetEvent);
	}

	void DrawSingleBool(string label, FsmBool val, string resetEvent, string setEvent)
	{
		DrawLabel(label);
		val.Value = DrawBool(val.Value);

		Indent();

		if (DrawButton(SetDataPlaymakerStrings.SetLbl))
			PlayMakerFSM.BroadcastEvent(setEvent);

		if (DrawButton(SetDataSceneStrings.Reset))
			PlayMakerFSM.BroadcastEvent(resetEvent);
	}

	void DrawSingleFloat(string label, FsmFloat val, string resetEvent, string setEvent)
	{
		DrawLabel(label);
		val.Value = DrawFloat(val.Value);
		
		Indent();
		
		if (DrawButton(SetDataPlaymakerStrings.SetLbl))
			PlayMakerFSM.BroadcastEvent(setEvent);
		
		if (DrawButton(SetDataSceneStrings.Reset))
			PlayMakerFSM.BroadcastEvent(resetEvent);
	}

	void DrawSingleColor(string label, FsmColor val, string resetEvent, string setEvent)
	{
		DrawLabel(label);
		val.Value = DrawColor(val.Value);
		
		Indent();
		
		if (DrawButton(SetDataPlaymakerStrings.SetLbl))
			PlayMakerFSM.BroadcastEvent(setEvent);
		
		if (DrawButton(SetDataSceneStrings.Reset))
			PlayMakerFSM.BroadcastEvent(resetEvent);
	}

	void DrawSingleInt(string label, FsmInt val, string resetEvent, string setEvent)
	{
		DrawLabel(label);
		val.Value = DrawInt(val.Value);
		
		Indent();
		
		if (DrawButton(SetDataPlaymakerStrings.SetLbl))
			PlayMakerFSM.BroadcastEvent(setEvent);
		
		if (DrawButton(SetDataSceneStrings.Reset))
			PlayMakerFSM.BroadcastEvent(resetEvent);
	}

	void DrawSingleVec2(string label, FsmVector2 val, string resetEvent, string setEvent)
	{
		DrawLabel(label);
		val.Value = DrawVector2(val.Value);
		
		Indent();
		
		if (DrawButton(SetDataPlaymakerStrings.SetLbl))
			PlayMakerFSM.BroadcastEvent(setEvent);
		
		if (DrawButton(SetDataSceneStrings.Reset))
			PlayMakerFSM.BroadcastEvent(resetEvent);
	}

	void DrawSingleVec3(string label, FsmVector3 val, string resetEvent, string setEvent)
	{
		DrawLabel(label);
		val.Value = DrawVector3(val.Value);
		
		Indent();
		
		if (DrawButton(SetDataPlaymakerStrings.SetLbl))
			PlayMakerFSM.BroadcastEvent(setEvent);
		
		if (DrawButton(SetDataSceneStrings.Reset))
			PlayMakerFSM.BroadcastEvent(resetEvent);
	}
	
	void DrawListData()
	{
		DrawLabel(SetDataPlaymakerStrings.StayTunedMsg);
	}
}
#endif
