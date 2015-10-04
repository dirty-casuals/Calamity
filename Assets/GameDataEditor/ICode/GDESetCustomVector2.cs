using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetCustomVec2ActionTooltip)]
	public class GDESetCustomVector2 : GDEActionBase
	{   
		[Tooltip(GDMConstants.Vec2CustomFieldTooltip)]
		public FsmString CustomField;
		
		public FsmVector2 Vector2Value;
		
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
				
				GDEDataManager.SetVector2(customKey+"_"+CustomField.Value, Vector2Value.Value);
			}
			catch(UnityException ex)
			{
				Debug.LogError(string.Format(GDMConstants.ErrorSettingCustomValue, GDMConstants.Vec2Type, ItemName.Value, FieldName.Value, CustomField.Value));
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

