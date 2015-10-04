using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.GetTexture2DCustomActionTooltip)]
	public class GDEGetCustomTexture2D : GDEActionBase
	{   
		[UIHint(UIHint.FsmString)]
		[Tooltip(GDMConstants.GOCustomFieldTooltip)]
		public FsmString CustomField;
		
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
				string customKey;
				Texture2D val;
				
				if (GDEDataManager.DataDictionary.ContainsKey(ItemName.Value))
				{
					GDEDataManager.Get(ItemName.Value, out data);
					data.TryGetString(FieldName.Value, out customKey);
					customKey = GDEDataManager.GetString(ItemName.Value+"_"+FieldName.Value, customKey);
					
					Dictionary<string, object> customData;
					GDEDataManager.Get(customKey, out customData);
					
					customData.TryGetTexture2D(CustomField.Value, out val);
					StoreResult.Value = val;
				}
				else
				{
					// New item case
					customKey = GDEDataManager.GetString(ItemName.Value+"_"+FieldName.Value, string.Empty);
					
					if (GDEDataManager.Get(customKey, out data))
					{
						data.TryGetTexture2D(CustomField.Value, out val);
						StoreResult.Value = val;
					}
				}
				
				StoreResult.Value = GDEDataManager.GetTexture2D(customKey+"_"+CustomField.Value, StoreResult.Value as Texture2D);
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