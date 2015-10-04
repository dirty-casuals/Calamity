using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetVec2ActionTooltip)]
	public class GDESetVector2 : GDEActionBase
	{   
		public FsmVector2 Vector2Value;
		
		public override void OnEnter()
		{
			try
			{
				GDEDataManager.SetVector2(FieldKey, Vector2Value.Value);
			}
			catch(UnityException ex)
			{
				Debug.LogError(string.Format(GDMConstants.ErrorSettingValue, GDMConstants.Vec2Type, ItemName.Value, FieldName.Value));
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

