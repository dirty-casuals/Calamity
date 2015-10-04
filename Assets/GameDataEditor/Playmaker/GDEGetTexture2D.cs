using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.GetTexture2DActionTooltip)]
	public class GDEGetTexture2D : GDEActionBase
	{   
		[UIHint(UIHint.FsmTexture)]
		public FsmTexture StoreResult;
		
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
					Texture2D val;
					data.TryGetTexture2D(FieldName.Value, out val);
					StoreResult.Value = val;
				}
				
				StoreResult.Value = GDEDataManager.GetTexture2D(FieldKey, StoreResult.Value as Texture2D);
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
