using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetStringActionTooltip)]
	public class GDESetString : GDEActionBase
	{   
		[UIHint(UIHint.FsmString)]
		public FsmString StringValue;
		
		public override void Reset()
		{
			base.Reset();
			StringValue = null;
		}
		
		public override void OnEnter()
		{
			try
			{
				GDEDataManager.SetString(FieldKey, StringValue.Value);
			}
			catch(UnityException ex)
			{
				LogError(string.Format(GDMConstants.ErrorSettingValue, GDMConstants.StringType, ItemName.Value, FieldName.Value));
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

