using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetCustomBoolActionTooltip)]
	public class GDESetCustomBool : GDEActionBase
	{   
		[UIHint(UIHint.FsmString)]
		[Tooltip(GDMConstants.BoolCustomFieldTooltip)]
		public FsmString CustomField;

		[UIHint(UIHint.FsmBool)]
		public FsmBool BoolValue;
		
		public override void Reset()
		{
			base.Reset();
			BoolValue = null;
		}
		
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
				LogError(string.Format(GDMConstants.ErrorSettingCustomValue, GDMConstants.BoolType, ItemName.Value, FieldName.Value, CustomField.Value));
				LogError(ex.ToString());
			}
			finally
			{
				Finish();
			}
		}
	}
}

#endif

