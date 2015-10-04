using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetVec3ActionTooltip)]
	public class GDESetVector3 : GDEActionBase
	{   
		[UIHint(UIHint.FsmVector3)]
		public FsmVector3 Vector3Value;
		
		public override void Reset()
		{
			base.Reset();
			Vector3Value = null;
		}
		
		public override void OnEnter()
		{
			try
			{
				GDEDataManager.SetVector3(FieldKey, Vector3Value.Value);
			}
			catch(UnityException ex)
			{
				LogError(string.Format(GDMConstants.ErrorSettingValue, GDMConstants.Vec3Type, ItemName.Value, FieldName.Value));
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

