using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.GetMaterialActionTooltip)]
	public class GDEGetMaterial : GDEActionBase
	{   
		[UIHint(UIHint.FsmMaterial)]
		public FsmMaterial StoreResult;
		
		public override void Reset()
		{
			base.Reset();
			StoreResult = null;
		}
		
		public override void OnEnter()
		{
			try
			{
				Dictionary<string, object> data;
				if (GDEDataManager.Get(ItemName.Value, out data))
				{
					Material val;
					data.TryGetMaterial(FieldName.Value, out val);
					StoreResult.Value = val;
				}
				
				StoreResult.Value = GDEDataManager.GetMaterial(FieldKey, StoreResult.Value);
			}
			catch(UnityException ex)
			{
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
