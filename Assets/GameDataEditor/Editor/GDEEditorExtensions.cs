using UnityEngine;
using System.Collections;

namespace GameDataEditor
{
	public static class GUIStyleExtensions
	{
		public static bool IsNullOrEmpty(this GUIStyle variable)
		{
			return variable == null || string.IsNullOrEmpty(variable.name);
		}
	}
}