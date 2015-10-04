using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetCustomActionTooltip)]
	public class GDESetCustom : GDEActionBase
	{   
		public FsmString CustomValue;
		
		public override void OnEnter()
		{
			try
			{
				GDEDataManager.SetString(FieldKey, CustomValue.Value);
			}
			catch(UnityException ex)
			{
				Debug.LogError(string.Format(GDMConstants.ErrorSettingValue, GDMConstants.CustomType, ItemName.Value, FieldName.Value));
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

