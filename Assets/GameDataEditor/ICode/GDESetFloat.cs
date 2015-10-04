using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetFloatActionTooltip)]
	public class GDESetFloat : GDEActionBase
	{   
		public FsmFloat FloatValue;
		
		public override void OnEnter()
		{
			try
			{
				GDEDataManager.SetFloat(FieldKey, FloatValue.Value);
			}
			catch(UnityException ex)
			{
				Debug.LogError(string.Format(GDMConstants.ErrorSettingValue, GDMConstants.FloatType, ItemName.Value, FieldName.Value));
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

