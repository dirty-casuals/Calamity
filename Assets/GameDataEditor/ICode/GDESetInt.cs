using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetIntActionTooltip)]
	public class GDESetInt : GDEActionBase
	{   
		public FsmInt IntValue;
		
		public override void OnEnter()
		{
			try
			{
				GDEDataManager.SetInt(FieldKey, IntValue.Value);
			}
			catch(UnityException ex)
			{
				Debug.LogError(string.Format(GDMConstants.ErrorSettingValue, GDMConstants.IntType, ItemName.Value, FieldName.Value));
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

