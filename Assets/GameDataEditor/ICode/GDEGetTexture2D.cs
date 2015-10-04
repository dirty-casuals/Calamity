using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.GetTexture2DActionTooltip)]
	public class GDEGetTexture2D : GDEActionBase
	{   
		public FsmObject StoreResult;
		
		public override void OnEnter()
		{
			try
			{
				Dictionary<string, object> data;
				if (GDEDataManager.Get(ItemName.Value, out data))
				{
					Texture2D val;
					data.TryGetTexture2D(FieldName.Value, out val);
					StoreResult.Value = val;
				}
				
				StoreResult.Value = GDEDataManager.GetTexture2D(FieldKey, StoreResult.Value as Texture2D);
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
