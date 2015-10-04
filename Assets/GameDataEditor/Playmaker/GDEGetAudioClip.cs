using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.GetAudioClipActionTooltip)]
	public class GDEGetAudioClip : GDEActionBase
	{   
		[ObjectType(typeof(AudioClip))]
		[UIHint(UIHint.FsmObject)]
		public FsmObject StoreResult;
		
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
					AudioClip val;
					data.TryGetAudioClip(FieldName.Value, out val);
					StoreResult.Value = val;
				}
				
				StoreResult.Value = GDEDataManager.GetAudioClip(FieldKey, StoreResult.Value as AudioClip);
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
