using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
    [Category(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.GetVec2CustomActionTooltip)]
    public class GDEGetCustomVector2 : GDEActionBase
    {   
        [Tooltip(GDMConstants.Vec2CustomFieldTooltip)]
        public FsmString CustomField;
        
        public FsmVector2 StoreResult;
        
        public override void OnEnter()
        {
			try
			{
				Dictionary<string, object> data;
				string customKey;
				Vector2 val;
				
				if (GDEDataManager.DataDictionary.ContainsKey(ItemName.Value))
				{
					GDEDataManager.Get(ItemName.Value, out data);
					data.TryGetString(FieldName.Value, out customKey);
					customKey = GDEDataManager.GetString(ItemName.Value+"_"+FieldName.Value, customKey);
					
					Dictionary<string, object> customData;
					GDEDataManager.Get(customKey, out customData);
					
					customData.TryGetVector2(CustomField.Value, out val);
					StoreResult.Value = val;
				}
				else
				{
					// New item case
					customKey = GDEDataManager.GetString(ItemName.Value+"_"+FieldName.Value, string.Empty);
					
					if (GDEDataManager.Get(customKey, out data))
					{
						data.TryGetVector2(CustomField.Value, out val);
						StoreResult.Value = val;
					}
				}
				
				StoreResult.Value = GDEDataManager.GetVector2(customKey+"_"+CustomField.Value, StoreResult.Value);
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
