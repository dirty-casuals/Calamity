using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.GetBoolActionTooltip)]
    public class GDEGetBool : GDEActionBase
    {   
        [UIHint(UIHint.FsmBool)]
        public FsmBool StoreResult;
        
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
                    bool val;
                    data.TryGetBool(FieldName.Value, out val);
                    StoreResult.Value = val;
                }
				
				// Override from saved data if it exists
				StoreResult.Value = GDEDataManager.GetBool(FieldKey, StoreResult.Value);
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

