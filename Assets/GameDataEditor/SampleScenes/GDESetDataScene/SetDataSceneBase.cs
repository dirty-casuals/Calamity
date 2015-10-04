using UnityEngine;
using System;
using System.Collections;

public class SetDataSceneBase : MonoBehaviour {

	protected GUIContent content;
	protected Vector2 size;
	protected GUISkin skin;

	float x = 0;
	float y = 0;
	float lineHeight;
	float spacer;
	float indent;

	protected enum DataType
	{
		Single,
		List,
		Bool2D,
		Int2D,
		String2D,
		Float2D,
		Vec2_2D,
		Vec3_2D,
		Vec4_2D,
		Color_2D,
		Custom_2D
	}
	
	protected DataType selectedType;

	virtual protected void InitGDE() {}

	protected void Update()
	{
		// This is only here so I don't have to stop and start each time Unity recompiles
		InitGDE();
			
		lineHeight = 20f;
		spacer = 5f;
		indent = 10f;
	}

	protected void NewLine(float numLines = 1)
	{
		x = 0;
		y += numLines*lineHeight + 2f;
	}
	
	protected void Indent(float numIndents = 1)
	{
		x += indent*numIndents;
	}
	
	protected void ResetToTop()
	{
		x = 0;
		y = 0;
	}
	
	protected bool DrawButton(string label)
	{
		content.text = label;
		size = skin.button.CalcSize(content);
		bool result = GUI.Button(new Rect(x, y, size.x, lineHeight), content);
		
		x += size.x + spacer;
		
		return result;
	}
	
	protected void DrawLabel(string label)
	{
		content.text = label;
		size = skin.label.CalcSize(content);
		
		GUI.Label(new Rect(x, y, size.x, size.y), content);
		x += size.x + spacer;
	}
	
	protected string DrawString(string val, float minFieldSize = 80f)
	{
		content.text = val;
		size = skin.textField.CalcSize(content);
		size.x = Mathf.Max(size.x, minFieldSize);
		
		val = GUI.TextField(new Rect(x, y, size.x, lineHeight), val);
		x += size.x + spacer;
		
		return val;
	}
	
	protected bool DrawBool(bool val)
	{
		content.text = string.Empty;
		size = skin.toggle.CalcSize(content);
		
		val = GUI.Toggle(new Rect(x, y, size.x, lineHeight), val, content);
		x += size.x + spacer;
		
		return val;
	}
	
	protected float DrawFloat(float val)
	{
		string floatString = Convert.ToString(val);
		if (!floatString.Contains("."))
			floatString += ".0";
		
		floatString = DrawString(floatString);
		
		return Convert.ToSingle(floatString);
	}
	
	protected int DrawInt(int val)
	{
		string intString = Convert.ToString(val);
		intString = DrawString(intString);
		return Convert.ToInt32(intString);
	}
	
	protected Vector2 DrawVector2(Vector2 val)
	{
		DrawLabel("X:");
		val.x = DrawFloat(val.x);
		
		DrawLabel("Y:");
		val.y = DrawFloat(val.y);
		
		return val;
	}
	
	protected Vector3 DrawVector3(Vector3 val)
	{
		DrawLabel("X:");
		val.x = DrawFloat(val.x);
		
		DrawLabel("Y:");
		val.y = DrawFloat(val.y);
		
		DrawLabel("Z:");
		val.z = DrawFloat(val.z);
		
		return val;
	}
	
	protected Vector4 DrawVector4(Vector4 val)
	{
		DrawLabel("X:");
		val.x = DrawFloat(val.x);
		
		DrawLabel("Y:");
		val.y = DrawFloat(val.y);
		
		DrawLabel("Z:");
		val.z = DrawFloat(val.z);
		
		DrawLabel("W:");
		val.w = DrawFloat(val.w);
		
		return val;
	}
	
	protected Color DrawColor(Color val)
	{
		DrawLabel("R:");
		val.r = DrawFloat(val.r);
		
		DrawLabel("G:");
		val.g = DrawFloat(val.g);
		
		DrawLabel("B:");
		val.b = DrawFloat(val.b);
		
		DrawLabel("A:");
		val.a = DrawFloat(val.a);
		
		return val;
	}
}
