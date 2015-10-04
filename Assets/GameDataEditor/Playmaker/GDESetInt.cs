using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetIntActionTooltip)]
	public class GDESetInt : GDEActionBase
	{   
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
				GDEDataManager.SetInt(FieldKey, IntValue.Value);
			}
			catch(UnityException ex)
			{
				LogError(string.Format(GDMConstants.ErrorSettingValue, GDMConstants.IntType, ItemName.Value, FieldName.Value));
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

