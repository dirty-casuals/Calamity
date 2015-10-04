using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.SetVec3ActionTooltip)]
	public class GDESetVector3 : GDEActionBase
	{   
		public FsmVector3 Vector3Value;
		
		public override void OnEnter()
		{
			try
			{
				GDEDataManager.SetVector3(FieldKey, Vector3Value.Value);
			}
			catch(UnityException ex)
			{
				Debug.LogError(string.Format(GDMConstants.ErrorSettingValue, GDMConstants.Vec3Type, ItemName.Value, FieldName.Value));
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

