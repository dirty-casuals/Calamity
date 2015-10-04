using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetBoolActionTooltip)]
	public class GDESetBool : GDEActionBase
	{   
		public FsmBool BoolValue;
		
		public override void OnEnter()
		{
			try
			{
				GDEDataManager.SetBool(FieldKey, BoolValue.Value);
			}
			catch(UnityException ex)
			{
				Debug.LogError(string.Format(GDMConstants.ErrorSettingValue, GDMConstants.BoolType, ItemName.Value, FieldName.Value));
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

