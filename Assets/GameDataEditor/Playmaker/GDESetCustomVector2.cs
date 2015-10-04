using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetCustomVec2ActionTooltip)]
	public class GDESetCustomVector2 : GDEActionBase
	{   
		[UIHint(UIHint.FsmString)]
		[Tooltip(GDMConstants.Vec2CustomFieldTooltip)]
		public FsmString CustomField;
		
		[UIHint(UIHint.FsmVector2)]
		public FsmVector2 Vector2Value;
		
		public override void Reset()
		{
			base.Reset();
			Vector2Value = null;
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
				
				GDEDataManager.SetVector2(customKey+"_"+CustomField.Value, Vector2Value.Value);
			}
			catch(UnityException ex)
			{
				LogError(string.Format(GDMConstants.ErrorSettingCustomValue, GDMConstants.Vec2Type, ItemName.Value, FieldName.Value, CustomField.Value));
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

