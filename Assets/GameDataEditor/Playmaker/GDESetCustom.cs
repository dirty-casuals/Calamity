using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetCustomActionTooltip)]
	public class GDESetCustom : GDEActionBase
	{   
		[UIHint(UIHint.FsmString)]
		public FsmString CustomValue;
		
		public override void Reset()
		{
			base.Reset();
			CustomValue = null;
		}
		
		public override void OnEnter()
		{
			try
			{
				GDEDataManager.SetString(FieldKey, CustomValue.Value);
			}
			catch(UnityException ex)
			{
				LogError(string.Format(GDMConstants.ErrorSettingValue, GDMConstants.CustomType, ItemName.Value, FieldName.Value));
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

