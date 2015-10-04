using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.GetStringActionTooltip)]
    public class GDEGetString : GDEActionBase
    {
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
                if (GDEDataManager.Get(ItemName.Value, out data))
                {
                    string val;
                    data.TryGetString(FieldName.Value, out val);
                    StoreResult.Value = val;
				}

				// Override with saved data value if it exists
				StoreResult.Value = GDEDataManager.GetString(FieldKey, StoreResult.Value);
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