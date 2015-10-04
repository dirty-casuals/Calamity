using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.ResetCustomActionTooltip)]
	public class GDEResetCustomField : GDEActionBase
	{   
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
				Debug.LogError(string.Format(GDMConstants.ErrorResettingCustomValue, ItemName.Value, FieldName.Value, CustomField.Value));
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

