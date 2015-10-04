/* 
 * ArrayPrefs2 v 1.4
 * http://wiki.unity3d.com/index.php/ArrayPrefs2
 *
 * Added functionality to save/load Vector4 type
 * Added functionality to support 2 Dimensional Lists
 * Split up into multiple files
 * Changed type from Array to List
 *
 * This File and its Content is available under Creative Commons Attribution Share Alike (http://creativecommons.org/licenses/by-sa/3.0/).
 * 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GameDataEditor 
{
	public partial class GDEPPX
	{
		public static bool SetBool(string name, bool value)
		{
			try
			{
				PlayerPrefs.SetInt(name, value? 1 : 0);
			}
			catch
			{
				return false;
			}
			return true;
		}

		public static void SetLong(string key, long value)
		{
			int lowBits, highBits;
			SplitLong(value, out lowBits, out highBits);
			PlayerPrefs.SetInt(key+"_lowBits", lowBits);
			PlayerPrefs.SetInt(key+"_highBits", highBits);
		}
		
		public static bool SetVector2(string key, Vector2 vector)
		{
			return SetFloatList(key, new List<float>(){vector.x, vector.y});
		}
		
		public static bool SetVector3(string key, Vector3 vector)
		{
			return SetFloatList(key, new List<float>(){vector.x, vector.y, vector.z});
		}
		
		public static bool SetVector4(string key, Vector4 vector)
		{
			return SetFloatList(key, new List<float>(){vector.x, vector.y, vector.z, vector.w});
		}
		
		public static bool SetQuaternion(string key, Quaternion vector)
		{
			return SetFloatList(key, new List<float>(){vector.x, vector.y, vector.z, vector.w});
		}
		
		public static bool SetColor(string key, Color color)
		{
			return SetFloatList(key, new List<float>(){color.r, color.g, color.b, color.a});
		}

		public static bool SetGameObject(string key, GameObject go)
		{
			return true;
		}

		public static bool SetTexture2D(string key, Texture2D tex)
		{
			return true;
		}

		public static bool SetMaterial(string key, Material mat)
		{
			return true;
		}

		public static bool SetAudioClip(string key, AudioClip aud)
		{
			return true;
		}
		
		public static bool SetBoolList(string key, List<bool> boolList)
		{
			// Make a byte array that's a multiple of 8 in length, plus 5 bytes to store the number of entries as an int32 (+ identifier)
			// We have to store the number of entries, since the boolArray length might not be a multiple of 8, so there could be some padded zeroes
			var bytes = new byte[(boolList.Count + 7)/8 + 5];
			bytes[0] = System.Convert.ToByte (ListType.Bool);	// Identifier
			var bits = new BitArray(boolList.ToArray());
			bits.CopyTo (bytes, 5);
			Initialize();
			ConvertInt32ToBytes (boolList.Count, bytes); // The number of entries in the boolArray goes in the first 4 bytes

			return SaveBytes (key, bytes);	
		}

		public static bool Set2DBoolList (string key, List<List<bool>> list)
		{
			// 5 is the min bytes: 1 byte for array type and 4 bytes for sublist count
			int byteCount = 5;

			var masterList = new List<bool>();

			// Gather all the bools in a master list
			// Determine how many bytes we need to save the sublist lengths
			foreach(var sublist in list)
			{
				byteCount += 4; //Add 4 bytes to save the sublist count as an int32
				masterList.AddRange(sublist);
			}

			// Add bytes needed to save bools as bits
			byteCount += (masterList.Count + 7)/8;


			int byteIndex = 0;
			byte[] bytes = new byte[byteCount];

			// Copy array type as a single byte
			bytes[0] = Convert.ToByte(ListType.Bool_2D);
			byteIndex++;

			// Copy number of sublists as an int32 (4 bytes)
			byte[] listCount = BitConverter.GetBytes(list.Count);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(listCount);

			listCount.CopyTo(bytes, byteIndex);
			byteIndex += listCount.Length;

			// Copy the lengths of each of the sublists
			foreach(var sublist in list)
			{
				listCount = BitConverter.GetBytes(sublist.Count);
				if (!BitConverter.IsLittleEndian)
					Array.Reverse(listCount);

				listCount.CopyTo(bytes, byteIndex);
				byteIndex += listCount.Length;
			}

			// Copy all the bools compacted as bits
			BitArray bits = new BitArray(masterList.ToArray());
			bits.CopyTo(bytes, byteIndex);

			return SaveBytes (key, bytes);
		}
		
		public static bool SetStringList(string key, List<string> list)
		{
			bool result = false;
			
			try
			{
				string json = Json.Serialize(list);
				PlayerPrefs.SetString (key, json);
				result = true;
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			
			return result;
		}

		public static bool Set2DStringList(string key, List<List<string>> list)
		{
			bool result = false;

			try
			{
				string json = Json.Serialize(list);
				PlayerPrefs.SetString (key, json);
				result = true;
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}

			return result;
		}
		
		public static bool SetIntList(string key, List<int> list)
		{
			return SetValue (key, list, ListType.Int32, 1, ConvertFromInt);
		}
		
		public static bool SetFloatList(string key, List<float> list)
		{
			return SetValue (key, list, ListType.Float, 1, ConvertFromFloat);
		}
		
		public static bool SetVector2List(string key, List<Vector2> list)
		{
			return SetValue (key, list, ListType.Vector2, 2, ConvertFromVector2);
		}
		
		public static bool SetVector3List(string key, List<Vector3> list)
		{
			return SetValue (key, list, ListType.Vector3, 3, ConvertFromVector3);
		}
		
		public static bool SetVector4List(string key, List<Vector4> list)
		{
			return SetValue (key, list, ListType.Vector4, 4, ConvertFromVector4);
		}
		
		public static bool SetQuaternionList(string key, List<Quaternion> list)
		{
			return SetValue (key, list, ListType.Quaternion, 4, ConvertFromQuaternion);
		}
		
		public static bool SetColorList(string key, List<Color> list)
		{
			return SetValue (key, list, ListType.Color, 4, ConvertFromColor);
		}

		public static bool SetGameObjectList(string key, List<GameObject> list)
		{
			return true;
		}

		public static bool SetTexture2DList(string key, List<Texture2D> list)
		{
			return true;
		}

		public static bool SetMaterialList(string key, List<Material> List)
		{
			return true;
		}

		public static bool SetAudioClipList(string key, List<AudioClip> list)
		{
			return true;
		}
		
		private static bool SetValue<T>(string key, T array, ListType arrayType, int vectorNumber, Action<T, byte[], int> convert) where T : IList
		{
			var bytes = new byte[(4*array.Count)*vectorNumber + 1];
			bytes[0] = System.Convert.ToByte (arrayType);	// Identifier
			Initialize();
			
			for (var i = 0; i < array.Count; i++) {
				convert (array, bytes, i);	
			}
			return SaveBytes (key, bytes);
		}

		public static bool Set2DIntList(string key, List<List<int>> int2DList)
		{
			return Set2DValue(key, int2DList, ListType.Int32_2D, 1, ConvertFromInt);
		}

		public static bool Set2DFloatList(string key, List<List<float>> float2DList)
		{
			return Set2DValue(key, float2DList, ListType.Float_2D, 1, ConvertFromFloat);
		}

		public static bool Set2DVector2List(string key, List<List<Vector2>> vec2_2DList)
		{
			return Set2DValue(key, vec2_2DList, ListType.Vector2_2D, 2, ConvertFromVector2);
		}

		public static bool Set2DVector3List(string key, List<List<Vector3>> vec3_2DList)
		{
			return Set2DValue(key, vec3_2DList, ListType.Vector3_2D, 3, ConvertFromVector3);
		}

		public static bool Set2DVector4List(string key, List<List<Vector4>> vec4_2DList)
		{
			return Set2DValue(key, vec4_2DList, ListType.Vector4_2D, 4, ConvertFromVector4);
		}

		public static bool Set2DGameObjectList(string key, List<List<GameObject>> go_2DList)
		{
			return true;
		}

		public static bool Set2DTexture2DList(string key, List<List<Texture2D>> tex_2DList)
		{
			return true;
		}

		public static bool Set2DMaterialList(string key, List<List<Material>> mat_2DList)
		{
			return true;
		}

		public static bool Set2DAudioClipList(string key, List<List<AudioClip>> aud_2DList)
		{
			return true;
		}

		public static bool Set2DQuaternionList(string key, List<List<Quaternion>> quaternion_2DList)
		{
			return Set2DValue(key, quaternion_2DList, ListType.Quaternion_2D, 4, ConvertFromQuaternion);
		}

		public static bool Set2DColorList(string key, List<List<Color>> color_2DList)
		{
			return Set2DValue(key, color_2DList, ListType.Color_2D, 4, ConvertFromColor);
		}

		private static bool Set2DValue(string key, IList list, ListType arrayType, int vectorNumber, Action<IList, byte[], int> convert)
		{
			var bytes = new List<byte>();

			// Add the arraytype as the first byte
			bytes.Add(System.Convert.ToByte(arrayType));

			// Copy number of sublists as an int32 (4 bytes)
			byte[] listCount = BitConverter.GetBytes(list.Count);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(listCount);
			bytes.AddRange(listCount);

			foreach(IList sublist in list) {
				// Copy sublist counts as int32 (4 bytes)
				listCount = BitConverter.GetBytes(sublist.Count);
				if (!BitConverter.IsLittleEndian)
					Array.Reverse(listCount);

				bytes.AddRange(listCount);
			}

			for(int i=0;  i<list.Count;  i++) {
				IList subList = list[i] as IList;
				byte[] subListBytes = new byte[(4*subList.Count)*vectorNumber];
				Initialize(true);

				for (int x=0;  x<subList.Count;  x++) {
					convert(subList, subListBytes, x); 
				}

				bytes.AddRange(subListBytes);
			}

			return SaveBytes (key, bytes.ToArray());
		}
	}
}
