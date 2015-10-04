using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using GameDataEditor;

public class GDE2DListScene : MonoBehaviour {

	GDETwoDListData data;
	GUIStyle labelStyle;
	float x = 0f;
	float y = 0f;
	const float lineHeight = 20f;

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

	void Start ()
	{
		if (!GDEDataManager.Init("2dlist_scene_data"))
			Debug.LogError(GDE2DListSceneStrings.ErrorInitializing);
		else
		{
			if (GDEDataManager.DataDictionary.TryGetCustom(GDE2DListItemKeys.TwoDList_DevItem, out data))
				LoadUnityObjects();
			else
				Debug.LogError(GDE2DListSceneStrings.ErrorLoadingData);
		}
	}

	void OnGUI()
	{
		Vector2 size;
		x = 0f;
		y = 0f;

		if (labelStyle == null)
			labelStyle = GUI.skin.label;

		content.text = GDE2DListSceneStrings.SceneDescription;
		size = labelStyle.CalcSize(content);
		GUI.Label(new Rect(x, y, size.x, lineHeight), content);
		y += lineHeight;

		// Bool
		DrawBasic(GDE2DListSceneStrings.BoolLbl, data.b);

		// Int
		DrawBasic(GDE2DListSceneStrings.IntLbl, data.i);

		// Float
		DrawBasic(GDE2DListSceneStrings.FloatLbl, data.f);

		// String
		DrawBasic(GDE2DListSceneStrings.StringLbl, data.s);

		// Vec2
		DrawBasic(GDE2DListSceneStrings.Vec2Lbl, data.v2);

		// Vec3
		DrawBasic(GDE2DListSceneStrings.Vec3Lbl, data.v3);

		// Vec4
		DrawBasic(GDE2DListSceneStrings.Vec4Lbl, data.v4);

		// Color
		DrawBasic(GDE2DListSceneStrings.ColorLbl, data.c);

		// GameObject
		// GameObjects are only instantiated once. Check LoadUnityObjects()

		// Texture2D
		// Texture2Ds are only loaded once. Check LoadUnityObjects()

		// Material
		// Materials are only loaded once. Check LoadUnityObjects()

		// AudioClip
		// AudioClips are loaded once. Check LoadUnityObjects()

		// Custom
		content.text = GDE2DListSceneStrings.CustomLbl;
		GUI.Label(new Rect(x, y, size.x, lineHeight), content);
		y += lineHeight;
		int curListNum = 1;
		content.text = string.Empty;
		foreach(var sublist in data.cus)
		{
			content.text += GDE2DListSceneStrings.ListLbl+curListNum+": ";
			foreach(var entry in sublist)
			{
				content.text += "[";
				if (entry.cust_string_list != null)
					entry.cust_string_list.ForEach(itr => content.text += itr + ", ");
				content.text += "] |";
			}
			content.text += "\n";
			curListNum++;
		}

		size = labelStyle.CalcSize(content);
		GUI.Label(new Rect(x, y, size.x, size.y), content);
		y += size.y;
	}

	void DrawBasic<T>(string typeLabel, T twodlist) where T : IList
	{
		int curListNum = 1;
		Vector2 size;

		GUIContent content = new GUIContent(typeLabel);
		size = labelStyle.CalcSize(content);
		GUI.Label(new Rect(x, y, size.x, lineHeight), content);
		y += lineHeight;

		content.text = string.Empty;
		foreach(IList list in twodlist) {
			content.text += GDE2DListSceneStrings.ListLbl+curListNum+": ";

			foreach(var entry in list) {
				content.text += entry+" | ";
			}

			content.text += "\n";
			curListNum++;
		}
		size = labelStyle.CalcSize(content);
		GUI.Label(new Rect(x, y, size.x, size.y), content);
		y += size.y;
	}

	void LoadUnityObjects()
	{
		// The cubes are in the first list
		// The light is in the second list
		// The globe is in the third list

		// Set the first cube's texture
		GameObject cube = Instantiate(data.go[0][0]) as GameObject;
		Renderer rendererCmp = cube.GetComponent<Renderer>();
		if (rendererCmp)
			rendererCmp.material.mainTexture = data.tex[0][0];


		// Load the sound fx
		AudioSource audioSource = cube.GetComponent<AudioSource>();
		if (audioSource == null)
			audioSource = cube.AddComponent<AudioSource>();
		audioSource.clip = data.aud[1][0];


		// Set the second cube's texture
		cube = Instantiate(data.go[0][1]) as GameObject;
		rendererCmp = cube.GetComponent<Renderer>();
		if (rendererCmp)
			rendererCmp.material.mainTexture = data.tex[0][1];


		// Set the globe's material
		GameObject globe = Instantiate(data.go[2][0]) as GameObject;
		rendererCmp = globe.GetComponent<Renderer>();
		if (rendererCmp)
			rendererCmp.material = data.mat[0][0];

		audioSource = globe.GetComponent<AudioSource>();
		if (audioSource == null)
			audioSource = globe.AddComponent<AudioSource>();
		audioSource.clip = data.aud[1][1];


		// Instantiate the light source
		Instantiate(data.go[1][0]);


		// Load the ambient track
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
			audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = data.aud[0][0];
		audioSource.loop = true;
		audioSource.Play();
	}
}
