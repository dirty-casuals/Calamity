using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.ResetCustomActionTooltip)]
	public class GDEResetCustomField : GDEActionBase
	{   
		[UIHint(UIHint.FsmString)]
		[Tooltip(GDMConstants.ResetFieldNameTooltip)]
		public FsmString CustomField;

		public override void OnEnter()
		{
			try
			{
				Dictionary<string, object> data;
				if (GDEDataManager.Get(ItemName.Value, out data))
				{
					string customKey;
					data.TryGetString(FieldName.Value, out customKey);
					customKey = GDEDataManager.GetString(ItemName.Value+"_"+FieldName.Value, customKey);

					GDEDataManager.ResetToDefault(customKey, CustomField.Value);
				}
			}
			catch(UnityException ex)
			{
				LogError(string.Format(GDMConstants.ErrorResettingCustomValue, ItemName.Value, FieldName.Value, CustomField.Value));
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

