using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
	[Category(GDMConstants.ActionCategory)]
	[Tooltip(GDMConstants.GetTexture2DCustomActionTooltip)]
	public class GDEGetCustomTexture2D : GDEActionBase
	{   
		[Tooltip(GDMConstants.Texture2DCustomFieldTooltip)]
		public FsmString CustomField;
		
		public FsmObject StoreResult;
		
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
