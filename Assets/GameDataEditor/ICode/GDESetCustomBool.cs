using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetCustomBoolActionTooltip)]
	public class GDESetCustomBool : GDEActionBase
	{   
		[Tooltip(GDMConstants.BoolCustomFieldTooltip)]
		public FsmString CustomField;

		public FsmBool BoolValue;
	
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
				
				GDEDataManager.SetBool(customKey+"_"+CustomField.Value, BoolValue.Value);
			}
			catch(UnityException ex)
			{
				Debug.LogError(string.Format(GDMConstants.ErrorSettingCustomValue, GDMConstants.BoolType, ItemName.Value, FieldName.Value, CustomField.Value));
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

