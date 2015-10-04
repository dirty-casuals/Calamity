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
		public static bool GetBool(string name)
		{
			return PlayerPrefs.GetInt(name) == 1;
		}
		
		public static bool GetBool(string name, bool defaultValue)
		{
			return (1==PlayerPrefs.GetInt(name, defaultValue?1:0));
		}
		
		public static long GetLong(string key, long defaultValue)
		{
			int lowBits, highBits;
			SplitLong(defaultValue, out lowBits, out highBits);
			lowBits = PlayerPrefs.GetInt(key+"_lowBits", lowBits);
			highBits = PlayerPrefs.GetInt(key+"_highBits", highBits);
			
			// unsigned, to prevent loss of sign bit.
			ulong ret = (uint)highBits;
			ret = (ret << 32);
			return (long)(ret | (ulong)(uint)lowBits);
		}
		
		public static long GetLong(string key)
		{
			int lowBits = PlayerPrefs.GetInt(key+"_lowBits");
			int highBits = PlayerPrefs.GetInt(key+"_highBits");
			
			// unsigned, to prevent loss of sign bit.
			ulong ret = (uint)highBits;
			ret = (ret << 32);
			return (long)(ret | (ulong)(uint)lowBits);
		}

		static Vector2 GetVector2(string key)
		{
			var list = GetFloatList(key);
			if (list.Count < 2)
			{
				return Vector2.zero;
			}
			return new Vector2(list[0], list[1]);
		}
		
		public static Vector2 GetVector2(string key, Vector2 defaultValue)
		{
			Vector2 retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetVector2(key);

			return retVal;
		}

		public static Color GetColor(string key)
		{
			var floatList = GetFloatList(key);
			if (floatList.Count < 4)
				return Color.black;

			return new Color(floatList[0], floatList[1], floatList[2], floatList[3]);
		}
		
		public static Color GetColor(string key , Color defaultValue)
		{
			Color retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetColor(key);

			return retVal;
		}

		public static Vector3 GetVector3(string key)
		{
			var floatList = GetFloatList(key);
			if (floatList.Count < 3)
			{
				return Vector3.zero;
			}
			return new Vector3(floatList[0], floatList[1], floatList[2]);
		}
		
		public static Vector3 GetVector3(string key, Vector3 defaultValue)
		{
			Vector3 retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetVector3(key);

			return retVal;
		}
		
		public static Vector4 GetVector4(string key)
		{
			var floatList = GetFloatList(key);

			if (floatList.Count < 4)
			{
				return Vector4.zero;
			}
			return new Vector4(floatList[0], floatList[1], floatList[2], floatList[3]);
		}
		
		public static Vector4 GetVector4(string key, Vector4 defaultValue)
		{
			Vector4 retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetVector4(key);

			return retVal;
		}

		public static List<bool> GetBoolList(string key)
		{
			List<bool> retVal = new List<bool>();

			if (PlayerPrefs.HasKey(key))
			{
				var bytes = System.Convert.FromBase64String (PlayerPrefs.GetString(key));
				if (bytes.Length < 5)
				{
					Debug.LogError (string.Format(GDMConstants.ErrorCorruptPrefFormat, key));
					return retVal;
				}
				if ((ListType)bytes[0] != ListType.Bool)
				{
					Debug.LogError (string.Format(GDMConstants.ErrorNotBoolArrayFormat, key));
					return retVal;
				}
				Initialize();
				
				// Make a new bytes array that doesn't include the number of entries + identifier (first 5 bytes) and turn that into a BitArray
				var bytes2 = new byte[bytes.Length-5];
				Array.Copy(bytes, 5, bytes2, 0, bytes2.Length);
				var bits = new BitArray(bytes2);
				// Get the number of entries from the first 4 bytes after the identifier and resize the BitArray to that length, then convert it to a boolean array
				bits.Length = ConvertBytesToInt32 (bytes);
				var boolArray = new bool[bits.Count];
				bits.CopyTo (boolArray, 0);

				retVal.AddRange(boolArray);
			}

			return retVal;
		}
		
		public static List<bool> GetBoolList(string key, List<bool> defaultValue) 
		{
			List<bool> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetBoolList(key);

			return retVal;
		}

		public static List<List<bool>> Get2DBoolList(string key)
		{
			List<List<bool>> retVal = new List<List<bool>>();

			if (PlayerPrefs.HasKey(key))
			{
				byte[] bytes = System.Convert.FromBase64String (PlayerPrefs.GetString(key));

				// Make sure the min header length is intact
				if (bytes.Length < 5)
				{
					Debug.LogError(string.Format(GDMConstants.ErrorCorruptPrefFormat, key));
					return retVal;
				}
				
				if ((ListType)bytes[0] != ListType.Bool_2D)
				{
					Debug.LogError (string.Format(GDMConstants.ErrorNotBoolArrayFormat, key));
					return retVal;
				}
				
				int listCount = BitConverter.ToInt32(bytes, 1);
				int dataIndex = listCount * 4 + 5;

				// Copy data to its own array and create a BitArray
				byte[] data = new byte[bytes.Length - dataIndex];
				Array.Copy(bytes, dataIndex, data, 0, data.Length);
				BitArray bits = new BitArray(data);

				// Construct the sublists
				int headerIndex = 5;
				int currentListCount = 0;
				int itr = 0;
				List<bool> subList;

				for (int x=0;  x<listCount;  x++)
				{
					// Read the sublist count
					currentListCount = BitConverter.ToInt32(bytes, headerIndex);
					headerIndex += 4;
					
					subList = new List<bool>();
					for(int y=0;  y<currentListCount;  y++)
						subList.Add(bits.Get(itr++));
					
					retVal.Add(subList);
				}
			}
			
			return retVal;
		}

		public static List<List<bool>> Get2DBoolList(string key, List<List<bool>> defaultValue)
		{
			List<List<bool>> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = Get2DBoolList(key);
			
			return retVal;
		}

		public static List<string> GetStringList(string key)
		{
			if (PlayerPrefs.HasKey(key))
			{
				string json = PlayerPrefs.GetString(key);
				List<object> rawList = Json.Deserialize(json) as List<object>;
				return rawList.ConvertAll(obj => obj.ToString());
			}
			
			return new List<string>();
		}
		
		public static List<string> GetStringList(string key, List<string> defaultValue)
		{
			List<string> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetStringList(key);

			return retVal;
		}

		public static List<List<string>> Get2DStringList(string key)
		{
			if (PlayerPrefs.HasKey(key))
			{
				var json = PlayerPrefs.GetString(key);
				List<object> rawList = Json.Deserialize(json) as List<object>;

				List<List<string>> result = new List<List<string>>();
				foreach(object temp in rawList)
				{
					List<object> rawSubList = temp as List<object>;
					result.Add(rawSubList.ConvertAll(obj => obj.ToString()));
				}

				return result;
			}

			return new List<List<string>>();
		}

		public static List<List<string>> Get2DStringList(string key, List<List<string>> defaultValue)
		{
			List<List<string>> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = Get2DStringList(key);

			return retVal;
		}

		public static List<int> GetIntList(string key)
		{
			List<int> retVal = new List<int>();
			GetValue (key, retVal, ListType.Int32, 1, ConvertToInt);
			return retVal;
		}
		
		public static List<int> GetIntList(string key, List<int> defaultValue)
		{
			List<int> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetIntList(key);

			return retVal;
		}

		public static List<List<int>> Get2DIntList(string key)
		{
			List<List<int>> retVal = new List<List<int>>();
			Get2DValue(key, retVal, ListType.Int32_2D, 1, ConvertToInt);
			return retVal;
		}

		public static List<List<int>> Get2DIntList(string key, List<List<int>> defaultValue)
		{
			List<List<int>> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = Get2DIntList(key);

			return retVal;
		}
		
		public static List<float> GetFloatList(string key)
		{
			List<float> retVal = new List<float>();
			GetValue (key, retVal, ListType.Float, 1, ConvertToFloat);
			return retVal;
		}
		
		public static List<float> GetFloatList(string key, List<float> defaultValue)
		{
			List<float> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetFloatList(key);

			return retVal;
		}

		public static List<List<float>> Get2DFloatList(string key)
		{
			List<List<float>> retVal = new List<List<float>>();
			Get2DValue(key, retVal, ListType.Float_2D, 1, ConvertToFloat);
			return retVal;
		}
		
		public static List<List<float>> Get2DFloatList(string key, List<List<float>> defaultValue)
		{
			List<List<float>> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = Get2DFloatList(key);
			
			return retVal;
		}
		
		public static List<Vector2> GetVector2List(string key)
		{
			List<Vector2> retVal = new List<Vector2>();
			GetValue (key, retVal, ListType.Vector2, 2, ConvertToVector2);
			return retVal;
		}
		
		public static List<Vector2> GetVector2List(string key, List<Vector2> defaultValue)
		{
			List<Vector2> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetVector2List(key);

			return retVal;
		}

		public static List<List<Vector2>> Get2DVector2List(string key)
		{
			List<List<Vector2>> retVal = new List<List<Vector2>>();
			Get2DValue(key, retVal, ListType.Vector2_2D, 2, ConvertToVector2);
			return retVal;
		}
		
		public static List<List<Vector2>> Get2DVector2List(string key, List<List<Vector2>> defaultValue)
		{
			List<List<Vector2>> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = Get2DVector2List(key);
			
			return retVal;
		}
		
		public static List<Vector3> GetVector3List(string key)
		{
			List<Vector3> retVal = new List<Vector3>();
			GetValue (key, retVal, ListType.Vector3, 3, ConvertToVector3);
			return retVal;
		}
		
		public static List<Vector3> GetVector3List(string key, List<Vector3> defaultValue)
		{
			List<Vector3> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetVector3List(key);

			return retVal;
		}

		public static List<List<Vector3>> Get2DVector3List(string key)
		{
			List<List<Vector3>> retVal = new List<List<Vector3>>();
			Get2DValue(key, retVal, ListType.Vector3_2D, 3, ConvertToVector3);
			return retVal;
		}
		
		public static List<List<Vector3>> Get2DVector3List(string key, List<List<Vector3>> defaultValue)
		{
			List<List<Vector3>> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = Get2DVector3List(key);
			
			return retVal;
		}
		
		public static List<Vector4> GetVector4List(string key)
		{
			List<Vector4> retVal = new List<Vector4>();
			GetValue (key, retVal, ListType.Vector4, 4, ConvertToVector4);
			return retVal;
		}
		
		public static List<Vector4> GetVector4List(string key, List<Vector4> defaultValue)
		{
			List<Vector4> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetVector4List(key);

			return retVal;
		}

		public static List<List<Vector4>> Get2DVector4List(string key)
		{
			var result = new List<List<Vector4>>();
			Get2DValue(key, result, ListType.Vector4_2D, 4, ConvertToVector4);
			return result;
		}
		
		public static List<List<Vector4>> Get2DVector4List(string key, List<List<Vector4>> defaultValue)
		{
			List<List<Vector4>> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = Get2DVector4List(key);
			
			return retVal;
		}
		
		public static List<Quaternion> GetQuaternionList(string key)
		{
			List<Quaternion> retVal = new List<Quaternion>();
			GetValue (key, retVal, ListType.Quaternion, 4, ConvertToQuaternion);
			return retVal;
		}
		
		public static List<Quaternion> GetQuaternionList(string key, List<Quaternion> defaultValue)
		{
			List<Quaternion> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetQuaternionList(key);

			return retVal;
		}

		public static List<List<Quaternion>> Get2DQuaternionList(string key)
		{
			List<List<Quaternion>> retVal = new List<List<Quaternion>>();
			Get2DValue(key, retVal, ListType.Quaternion_2D, 4, ConvertToQuaternion);
			return retVal;
		}
		
		public static List<List<Quaternion>> Get2DQuaternionList(string key, List<List<Quaternion>> defaultValue)
		{
			List<List<Quaternion>> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = Get2DQuaternionList(key);
			
			return retVal;
		}
		
		public static List<Color> GetColorList(string key)
		{
			List<Color> retVal = new List<Color>();
			GetValue (key, retVal, ListType.Color, 4, ConvertToColor);
			return retVal;
		}
		
		public static List<Color> GetColorList(string key, List<Color> defaultValue)
		{
			List<Color> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetColorList(key);

			return retVal;
		}

		public static List<List<Color>> Get2DColorList(string key)
		{
			var retVal = new List<List<Color>>();
			Get2DValue(key, retVal, ListType.Color_2D, 4, ConvertToColor);
			return retVal;
		}
		
		public static List<List<Color>> Get2DColorList(string key, List<List<Color>> defaultValue)
		{
			List<List<Color>> retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = Get2DColorList(key);
			
			return retVal;
		}
		
		private static void GetValue<T>(string key, T list, ListType arrayType, int vectorNumber, Action<T, byte[]> convert) where T : IList
		{
			if (PlayerPrefs.HasKey(key))
			{
				var bytes = System.Convert.FromBase64String (PlayerPrefs.GetString(key));
				if ((bytes.Length-1) % (vectorNumber*4) != 0)
				{
					Debug.LogError(string.Format(GDMConstants.ErrorCorruptPrefFormat, key));
					return;
				}
				if ((ListType)bytes[0] != arrayType)
				{
					Debug.LogError(string.Format(GDMConstants.ErrorNotArrayFormat, key, arrayType.ToString()));
					return;
				}
				Initialize();
				
				var end = (bytes.Length-1) / (vectorNumber*4);
				for (var i = 0; i < end; i++)
				{
					convert (list, bytes);
				}
			}
		}

		private static void Get2DValue<T>(string key, T list, ListType arrayType, int vectorNumber, Action<IList, byte[]> convert) where T : IList
		{
			if (PlayerPrefs.HasKey(key))
			{
				var bytes = System.Convert.FromBase64String (PlayerPrefs.GetString(key));

				// Make sure the min header length is intact
				if (bytes.Length < 5)
				{
					Debug.LogError(string.Format(GDMConstants.ErrorCorruptPrefFormat, key));
					return;
				}

				int listCount = BitConverter.ToInt32(bytes, 1);
				byte[] headerBytes = new byte[listCount*4 + 5];
				Array.Copy(bytes, 0, headerBytes, 0, headerBytes.Length);

				List<byte> allBytes = new List<byte>(bytes);
				allBytes.RemoveRange(1, headerBytes.Length-1);
				bytes = allBytes.ToArray();

				if ((bytes.Length-1) % (vectorNumber*4) != 0)
				{
					Debug.LogError (string.Format(GDMConstants.ErrorCorruptPrefFormat, key));
					return;
				}

				if ((ListType)bytes[0] != arrayType)
				{
					Debug.LogError (string.Format(GDMConstants.ErrorCorruptPrefFormat, key, arrayType.ToString()));
					return;
				}
				Initialize();

				IList masterList;
				GetNewListForType(arrayType, out masterList);

				var end = (bytes.Length-1) / (vectorNumber*4);
				for (int i = 0; i < end; i++)
				{
					convert (masterList, bytes);
				}

				// Construct the sublists
				int currentListCount = 0;
				int itr = 0;
				int headerIndex = 5;
				IList subList;
				for (int x=0;  x<listCount;  x++)
				{
					currentListCount = BitConverter.ToInt32(headerBytes, headerIndex);
					headerIndex += 4;

					GetNewListForType(arrayType, out subList);
					for(int y=0;  y<currentListCount;  y++)
						subList.Add(masterList[itr++]);

					list.Add(subList);
				}
			}
		}

		private static void GetNewListForType(ListType arrayType, out IList list)
		{
			list = null;

			if (arrayType.Equals(ListType.Bool_2D))
				list = new List<bool>();
			else if (arrayType.Equals(ListType.Int32_2D))
				list = new List<int>();
			else if (arrayType.Equals(ListType.Float_2D))
				list = new List<float>();
			else if (arrayType.Equals(ListType.Vector2_2D))
				list = new List<Vector2>();
			else if (arrayType.Equals(ListType.Vector3_2D))
				list = new List<Vector3>();
			else if (arrayType.Equals(ListType.Vector4_2D))
				list = new List<Vector4>();
			else if (arrayType.Equals(ListType.Quaternion_2D))
				list = new List<Quaternion>();
			else if (arrayType.Equals(ListType.Color_2D))
				list = new List<Color>();
		}

		public static Quaternion GetQuaternion(string key)
		{
			var list = GetFloatList(key);
			if (list.Count < 4)
			{
				return Quaternion.identity;
			}
			return new Quaternion(list[0], list[1], list[2], list[3]);
		}
		
		public static Quaternion GetQuaternion(string key, Quaternion defaultValue )
		{
			Quaternion retVal = defaultValue;

			if (PlayerPrefs.HasKey(key))
				retVal = GetQuaternion(key);

			return retVal;
		}
	}
}
