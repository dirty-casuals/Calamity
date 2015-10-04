using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetCustomIntActionTooltip)]
	public class GDESetCustomInt : GDEActionBase
	{   
		[Tooltip(GDMConstants.IntCustomFieldTooltip)]
		public FsmString CustomField;
		
		public FsmInt IntValue;
		
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
					customKey = GDEDataManager.GetString(ItemName.Value+"_"+FieldName.Value, string.Empty);
				}
				
				GDEDataManager.SetInt(customKey+"_"+CustomField.Value, IntValue.Value);
			}
			catch(UnityException ex)
			{
				Debug.LogError(string.Format(GDMConstants.ErrorSettingCustomValue, GDMConstants.IntType, ItemName.Value, FieldName.Value, CustomField.Value));
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

