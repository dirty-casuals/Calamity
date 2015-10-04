using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.ResetActionTooltip)]
	public class GDEResetField : GDEActionBase
	{   
		public override void OnEnter()
		{
			try
			{
				GDEDataManager.ResetToDefault(ItemName.Value, FieldName.Value);
			}
			catch(UnityException ex)
			{
				LogError(string.Format(GDMConstants.ErrorResettingValue, ItemName.Value, FieldName.Value));
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

