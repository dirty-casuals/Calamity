using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.GetStringCustomActionTooltip)]
    public class GDEGetCustomString : GDEActionBase
    {   
        [UIHint(UIHint.FsmString)]
        [Tooltip(GDMConstants.StringCustomFieldTooltip)]
        public FsmString CustomField;

        [UIHint(UIHint.FsmString)]
        public FsmString StoreResult;
        
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
