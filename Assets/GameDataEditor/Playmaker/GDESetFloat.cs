using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetFloatActionTooltip)]
	public class GDESetFloat : GDEActionBase
	{   
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
				GDEDataManager.SetFloat(FieldKey, FloatValue.Value);
			}
			catch(UnityException ex)
			{
				LogError(string.Format(GDMConstants.ErrorSettingValue, GDMConstants.FloatType, ItemName.Value, FieldName.Value));
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

