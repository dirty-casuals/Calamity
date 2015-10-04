/// <summary>
/// Sounds used:
/// http://www.freesound.org/people/THE_bizniss/sounds/39459/
/// http://www.freesound.org/people/suonho/sounds/3176/
/// http://www.looperman.com/loops/detail/84579/distorted-reality-by-40a-free-80bpm-ambient-piano-loop
///
/// CC by 3.0: http://creativecommons.org/licenses/by/3.0/
/// </summary>

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor;


public class ReadDataScene : MonoBehaviour
{
    public GUIText BoolField;
    public GUIText BoolListField;
    public GUIText IntField;
    public GUIText IntListField;
    public GUIText FloatField;
    public GUIText FloatListField;
    public GUIText StringField;
    public GUIText StringListField;
    public GUIText Vector2Field;
    public GUIText Vector2LisField;
    public GUIText Vector3Field;
    public GUIText Vector3ListField;
    public GUIText Vector4Field;
    public GUIText Vector4ListField;
    public GUIText ColorField;
    public GUIText ColorListField;

    public GUIText GetAllDataBySchema;
    public GUIText GetAllKeysBySchema;

    public GUIText Status;
	public GUIText SceneHeader;
	public GUIText SceneDescription;

	GDEReadSceneData data;

	void Start ()
    {
		SceneHeader.text = ReadDataSceneStrings.HeaderLbl;
		SceneDescription.text = ReadDataSceneStrings.DescriptionLbl;

		// Initialize GDE
        if (!GDEDataManager.Init("read_scene_data")) {
			Status.text = ReadDataSceneStrings.StatusErrorInitiliazing;
			return;
		}

		// Load the data for the scene into the GDEReadSceneData object
		if (!GDEDataManager.DataDictionary.TryGetCustom(GDEReadSceneItemKeys.ReadScene_test_data, out data)) {
			Status.text = ReadDataSceneStrings.StatusErrorLoadingData;
			return;
		}

		try
	    {
	        // Bool
	        BoolField.text = ReadDataSceneStrings.BoolFieldLbl + " ";
	        BoolField.text += data.bool_field;

	        // Bool List
			BoolListField.text = ReadDataSceneStrings.BoolListFieldLbl + " ";
            foreach(bool boolVal in data.bool_list_field)
	            BoolListField.text += string.Format("{0} ", boolVal);

	        // Int
			IntField.text = ReadDataSceneStrings.IntFieldLbl + " ";
	        IntField.text += data.int_field;

	        // Int List
			IntListField.text = ReadDataSceneStrings.IntListFieldLbl + " ";
            foreach(int intVal in data.int_list_field)
	            IntListField.text += string.Format("{0} ", intVal);

	        // Float
			FloatField.text = ReadDataSceneStrings.FloatFieldLbl + " ";
            FloatField.text += data.float_field;

	        // Float List
			FloatListField.text = ReadDataSceneStrings.FloatListFieldLbl + " ";
            foreach(float floatVal in data.float_list_field)
	            FloatListField.text += string.Format("{0} ", floatVal);

	        // String
			StringField.text = ReadDataSceneStrings.StringFieldLbl + " ";
            StringField.text += data.string_field;

	        // String List
			StringListField.text = ReadDataSceneStrings.StringListFieldLbl + " ";
            foreach(string stringVal in data.string_list_field)
	            StringListField.text += string.Format("{0} ", stringVal);

	        // Vector2
			Vector2Field.text = ReadDataSceneStrings.Vec2FieldLbl + " ";
            Vector2Field.text += string.Format("({0}, {1})", data.vector2_field.x, data.vector2_field.y);

	        // Vector2 List
			Vector2LisField.text = ReadDataSceneStrings.Vec2ListFieldLbl + " ";
            foreach(Vector2 vec2Val in data.vector2_list_field)
	            Vector2LisField.text += string.Format("({0}, {1}) ", vec2Val.x, vec2Val.y);

	        // Vector3
			Vector3Field.text = ReadDataSceneStrings.Vec3FieldLbl + " ";
            Vector3Field.text += string.Format("({0}, {1}, {2})", data.vector3_field.x, data.vector3_field.y, data.vector3_field.z);

	        // Vector3 List
			Vector3ListField.text = ReadDataSceneStrings.Vec3ListFieldLbl + " ";
            foreach(Vector3 vec3Val in data.vector3_list_field)
	            Vector3ListField.text += string.Format("({0}, {1}, {2}) ", vec3Val.x, vec3Val.y, vec3Val.z);

	        // Vector4
			Vector4Field.text = ReadDataSceneStrings.Vec4FieldLbl + " ";
            Vector4Field.text += string.Format("({0}, {1}, {2}, {3})", data.vector4_field.x, data.vector4_field.y, data.vector4_field.z, data.vector4_field.w);

	        // Vector4 List
			Vector4ListField.text = ReadDataSceneStrings.Vec4ListFieldLbl + " ";
            foreach(Vector4 vec4Val in data.vector4_list_field)
	            Vector4ListField.text += string.Format("({0}, {1}, {2}, {3}) ", vec4Val.x, vec4Val.y, vec4Val.z, vec4Val.w);

	        // Color
			ColorField.text = ReadDataSceneStrings.ColorFieldLbl + " ";
            ColorField.text += data.color_field.ToString();

	        // Color List
			ColorListField.text = ReadDataSceneStrings.ColorListFieldLbl + " ";
            foreach(Color colVal in data.color_list_field)
	            ColorListField.text += string.Format("{0}   ", colVal);

	        // Custom
			// See LoadUnityTypes for a Custom field example

			LoadUnityTypes();

			/**
			 *
			 * The following two methods (GetAllDataBySchema and GetAllKeysBySchema) are part of the old version of the GDE API.
			 * They still work, but require a little more code to use.
			 *
			 **/

	        // Get All Data By Schema
            GetAllDataBySchema.text = ReadDataSceneStrings.GetAllBySchemaLbl + Environment.NewLine;
	        Dictionary<string, object> allDataByCustomSchema;
	        GDEDataManager.GetAllDataBySchema("ReadSceneCustom", out allDataByCustomSchema);
	        foreach(KeyValuePair<string, object> pair in allDataByCustomSchema)
	        {
	            string description;
	            Dictionary<string, object> customData = pair.Value as Dictionary<string, object>;
	            customData.TryGetString("description", out description);
	            GetAllDataBySchema.text += string.Format("     {0}{1}", description, Environment.NewLine);
	        }

	        // Get All Keys By Schema
	        GetAllKeysBySchema.text = ReadDataSceneStrings.GetAllKeysBySchemaLbl + " ";
	        List<string> customKeys;
	        GDEDataManager.GetAllDataKeysBySchema("ReadSceneCustom", out customKeys);
	        foreach(string key in customKeys)
	            GetAllKeysBySchema.text += string.Format("{0} ", key);

	        Status.text = ReadDataSceneStrings.StatusSuccess;
	    }
	    catch(Exception ex)
	    {
	        Status.text = ReadDataSceneStrings.StatusError;
	        Debug.LogError(ex);
	    }
	}

	void LoadUnityTypes()
	{
		// Game Object
		// This is the light
		if (data.custom_field.go_field != null)
			Instantiate(data.custom_field.go_field);

		// Game Object List
		// This list contains the cube and sphere
		GameObject cube = Instantiate(data.custom_field.go_list_field[0]) as GameObject;
		GameObject sphere = Instantiate(data.custom_field.go_list_field[1]) as GameObject;

		// Texture2D
		Renderer rendererCmp = cube.GetComponent<Renderer>();
		if (rendererCmp)
			rendererCmp.material.mainTexture = data.custom_field.tex_field;

		// Material
		rendererCmp = sphere.GetComponent<Renderer>();
		if (rendererCmp)
			rendererCmp.material = data.custom_field.mat_field;

		// AudioClip
		AudioSource audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
			audioSource = gameObject.AddComponent<AudioSource>();

		audioSource.loop = true;
		audioSource.clip = data.custom_field.aud_field;
		audioSource.PlayDelayed(1);

		// Material, Texture2D, and AudioClip lists work exactly like the GameObject list above
	}
}
