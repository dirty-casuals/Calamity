using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
    [Category(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.GetStringCustomActionTooltip)]
    public class GDEGetCustomString : GDEActionBase
    {   
        [Tooltip(GDMConstants.StringCustomFieldTooltip)]
        public FsmString CustomField;

        public FsmString StoreResult;
        
        public override void OnEnter()
        {
			try
			{
				Dictionary<string, object> data;
				string customKey;
				string val;
				
				if (GDEDataManager.DataDictionary.ContainsKey(ItemName.Value))
				{
					GDEDataManager.Get(ItemName.Value, out data);
					data.TryGetString(FieldName.Value, out customKey);
					customKey = GDEDataManager.GetString(FieldKey, customKey);
					
					Dictionary<string, object> customData;
					GDEDataManager.Get(customKey, out customData);
					
					customData.TryGetString(CustomField.Value, out val);
					StoreResult.Value = val;
				}
				else
				{
					// New item case
					customKey = GDEDataManager.GetString(FieldKey, string.Empty);
					
					if (GDEDataManager.Get(customKey, out data))
					{
						data.TryGetString(CustomField.Value, out val);
						StoreResult.Value = val;
					}
				}
				
				StoreResult.Value = GDEDataManager.GetString(customKey+"_"+CustomField.Value, StoreResult.Value);
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
