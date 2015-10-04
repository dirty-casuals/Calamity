using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetCustomVec3ActionTooltip)]
	public class GDESetCustomVector3 : GDEActionBase
	{   
		[Tooltip(GDMConstants.Vec3CustomFieldTooltip)]
		public FsmString CustomField;
		
		public FsmVector3 Vector3Value;
		
		public override void OnEnter()
		{
			try
			{	
				Dictionary<string, object> data;
				string customKey;
				
				if (GDEDataManager.DataDictionary.ContainsKey(ItemName.Value))
				{
					GDEDataManager.Get(ItemName.Value, out data);
					data.TryGetString(FieldName.Value, out customKey);
					customKey = GDEDataManager.GetString(FieldKey, customKey);
				}
				else
				{
					// New Item Case
					customKey = GDEDataManager.GetString(FieldKey, string.Empty);
				}
				
				GDEDataManager.SetVector3(customKey+"_"+CustomField.Value, Vector3Value.Value);
			}
			catch(UnityException ex)
			{
				Debug.LogError(string.Format(GDMConstants.ErrorSettingCustomValue, GDMConstants.Vec3Type, ItemName.Value, FieldName.Value, CustomField.Value));
				Debug.LogError(ex.ToString());
			}
			finally
			{
				Finish();
			}
		}
	}
}

#endif

