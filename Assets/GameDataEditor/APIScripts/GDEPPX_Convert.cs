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
		
		private static void ConvertToInt (List<int> list, byte[] bytes)
		{
			ConvertToInt((IList)list, bytes);
		}

		private static void ConvertToInt (IList list, byte[] bytes)
		{
			list.Add (ConvertBytesToInt32(bytes));
		}
		
		private static void ConvertToFloat (List<float> list, byte[] bytes)
		{
			ConvertToFloat((IList)list, bytes);
		}

		private static void ConvertToFloat (IList list, byte[] bytes)
		{
			list.Add (ConvertBytesToFloat(bytes));
		}
		
		private static void ConvertToVector2 (List<Vector2> list, byte[] bytes)
		{
			ConvertToVector2((IList)list, bytes);
		}

		private static void ConvertToVector2 (IList list, byte[] bytes)
		{
			list.Add (new Vector2(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));
		}

		private static void ConvertToVector3 (List<Vector3> list, byte[] bytes)
		{
			ConvertToVector3((IList)list, bytes);
		}

		private static void ConvertToVector3 (IList list, byte[] bytes)
		{
			list.Add (new Vector3(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));
		}

		private static void ConvertToVector4 (List<Vector4> list, byte[] bytes)
		{
			ConvertToVector4((IList)list, bytes);
		}
		
		private static void ConvertToVector4 (IList list, byte[] bytes)
		{
			list.Add (new Vector4(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));
		}

		private static void ConvertToQuaternion (List<Quaternion> list, byte[] bytes)
		{
			ConvertToQuaternion((IList)list, bytes);
		}
		
		private static void ConvertToQuaternion (IList list, byte[] bytes)
		{
			list.Add (new Quaternion(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));
		}

		private static void ConvertToColor (List<Color> list, byte[] bytes)
		{
			ConvertToColor((IList)list, bytes);
		}

		private static void ConvertToColor (IList list, byte[] bytes)
		{
			list.Add (new Color(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));
		}

		private static void ConvertFromInt (int[] array, byte[] bytes, int i)
		{
			ConvertInt32ToBytes (array[i], bytes);
		}

		private static void ConvertFromInt (IList list, byte[] bytes, int i)
		{
			ConvertInt32ToBytes ((int)list[i], bytes);
		}
		
		private static void ConvertFromFloat (float[] array, byte[] bytes, int i)
		{
			ConvertFloatToBytes (array[i], bytes);
		}

		private static void ConvertFromFloat (IList list, byte[] bytes, int i)
		{
			ConvertFloatToBytes ((float)list[i], bytes);
		}
		
		private static void ConvertFromVector2 (Vector2[] array, byte[] bytes, int i)
		{
			ConvertFloatToBytes (array[i].x, bytes);
			ConvertFloatToBytes (array[i].y, bytes);
		}

		private static void ConvertFromVector2 (IList list, byte[] bytes, int i)
		{
			ConvertFloatToBytes (((Vector2)list[i]).x, bytes);
			ConvertFloatToBytes (((Vector2)list[i]).y, bytes);
		}
		
		private static void ConvertFromVector3 (Vector3[] array, byte[] bytes, int i)
		{
			ConvertFloatToBytes (array[i].x, bytes);
			ConvertFloatToBytes (array[i].y, bytes);
			ConvertFloatToBytes (array[i].z, bytes);
		}

		private static void ConvertFromVector3 (IList list, byte[] bytes, int i)
		{
			ConvertFloatToBytes (((Vector3)list[i]).x, bytes);
			ConvertFloatToBytes (((Vector3)list[i]).y, bytes);
			ConvertFloatToBytes (((Vector3)list[i]).z, bytes);
		}
		
		private static void ConvertFromVector4 (Vector4[] array, byte[] bytes, int i)
		{
			ConvertFloatToBytes (array[i].x, bytes);
			ConvertFloatToBytes (array[i].y, bytes);
			ConvertFloatToBytes (array[i].z, bytes);
			ConvertFloatToBytes (array[i].w, bytes);
		}

		private static void ConvertFromVector4 (IList list, byte[] bytes, int i)
		{
			ConvertFloatToBytes (((Vector4)list[i]).x, bytes);
			ConvertFloatToBytes (((Vector4)list[i]).y, bytes);
			ConvertFloatToBytes (((Vector4)list[i]).z, bytes);
			ConvertFloatToBytes (((Vector4)list[i]).w, bytes);
		}
		
		private static void ConvertFromQuaternion (Quaternion[] array, byte[] bytes, int i)
		{
			ConvertFloatToBytes (array[i].x, bytes);
			ConvertFloatToBytes (array[i].y, bytes);
			ConvertFloatToBytes (array[i].z, bytes);
			ConvertFloatToBytes (array[i].w, bytes);
		}

		private static void ConvertFromQuaternion (IList list, byte[] bytes, int i)
		{
			ConvertFloatToBytes (((Quaternion)list[i]).x, bytes);
			ConvertFloatToBytes (((Quaternion)list[i]).y, bytes);
			ConvertFloatToBytes (((Quaternion)list[i]).z, bytes);
			ConvertFloatToBytes (((Quaternion)list[i]).w, bytes);
		}
		
		private static void ConvertFromColor (Color[] array, byte[] bytes, int i)
		{
			ConvertFloatToBytes (array[i].r, bytes);
			ConvertFloatToBytes (array[i].g, bytes);
			ConvertFloatToBytes (array[i].b, bytes);
			ConvertFloatToBytes (array[i].a, bytes);
		}

		private static void ConvertFromColor (IList list, byte[] bytes, int i)
		{
			ConvertFloatToBytes (((Color)list[i]).r, bytes);
			ConvertFloatToBytes (((Color)list[i]).g, bytes);
			ConvertFloatToBytes (((Color)list[i]).b, bytes);
			ConvertFloatToBytes (((Color)list[i]).a, bytes);
		}

		private static void ConvertFloatToBytes (float f, byte[] bytes)
		{
			byteBlock = System.BitConverter.GetBytes (f);
			ConvertTo4Bytes (bytes);
		}
		
		private static float ConvertBytesToFloat (byte[] bytes)
		{
			ConvertFrom4Bytes (bytes);
			return System.BitConverter.ToSingle (byteBlock, 0);
		}
		
		private static void ConvertInt32ToBytes (int i, byte[] bytes)
		{
			byteBlock = System.BitConverter.GetBytes (i);
			ConvertTo4Bytes (bytes);
		}
		
		private static int ConvertBytesToInt32 (byte[] bytes)
		{
			ConvertFrom4Bytes (bytes);
			return System.BitConverter.ToInt32 (byteBlock, 0);
		}
		
		private static void ConvertTo4Bytes (byte[] bytes)
		{
			bytes[idx  ] = byteBlock[    endianDiff1];
			bytes[idx+1] = byteBlock[1 + endianDiff2];
			bytes[idx+2] = byteBlock[2 - endianDiff2];
			bytes[idx+3] = byteBlock[3 - endianDiff1];
			idx += 4;
		}
		
		private static void ConvertFrom4Bytes (byte[] bytes)
		{
			byteBlock[    endianDiff1] = bytes[idx  ];
			byteBlock[1 + endianDiff2] = bytes[idx+1];
			byteBlock[2 - endianDiff2] = bytes[idx+2];
			byteBlock[3 - endianDiff1] = bytes[idx+3];
			idx += 4;
		}
	}
}
