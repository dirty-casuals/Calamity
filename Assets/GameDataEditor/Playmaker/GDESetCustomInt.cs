using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetCustomIntActionTooltip)]
	public class GDESetCustomInt : GDEActionBase
	{   
		[UIHint(UIHint.FsmString)]
		[Tooltip(GDMConstants.IntCustomFieldTooltip)]
		public FsmString CustomField;
		
		[UIHint(UIHint.FsmInt)]
		public FsmInt IntValue;
		
		public override void Reset()
		{
			base.Reset();
			IntValue = null;
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
					customKey = GDEDataManager.GetString(ItemName.Value+"_"+FieldName.Value, string.Empty);
				}
				
				GDEDataManager.SetInt(customKey+"_"+CustomField.Value, IntValue.Value);
			}
			catch(UnityException ex)
			{
				LogError(string.Format(GDMConstants.ErrorSettingCustomValue, GDMConstants.IntType, ItemName.Value, FieldName.Value, CustomField.Value));
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

