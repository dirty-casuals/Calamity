using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetCustomFloatActionTooltip)]
	public class GDESetCustomFloat : GDEActionBase
	{   
		[UIHint(UIHint.FsmString)]
		[Tooltip(GDMConstants.FloatCustomFieldTooltip)]
		public FsmString CustomField;
		
		[UIHint(UIHint.FsmFloat)]
		public FsmFloat FloatValue;
		
		public override void Reset()
		{
			base.Reset();
			FloatValue = null;
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
				
				GDEDataManager.SetFloat(customKey+"_"+CustomField.Value, FloatValue.Value);
			}
			catch(UnityException ex)
			{
				LogError(string.Format(GDMConstants.ErrorSettingCustomValue, GDMConstants.FloatType, ItemName.Value, FieldName.Value, CustomField.Value));
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

