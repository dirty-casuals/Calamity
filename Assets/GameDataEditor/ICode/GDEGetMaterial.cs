using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.GetMaterialActionTooltip)]
	public class GDEGetMaterial : GDEActionBase
	{   
		public FsmObject StoreResult;
		
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
				
				StoreResult.Value = GDEDataManager.GetMaterial(FieldKey, StoreResult.Value as Material);
			}
			catch(UnityException ex)
			{
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