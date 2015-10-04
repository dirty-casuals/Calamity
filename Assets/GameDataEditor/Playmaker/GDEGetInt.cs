using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.GetIntActionTooltip)]
    public class GDEGetInt : GDEActionBase
    {	
        [UIHint(UIHint.FsmInt)]
        public FsmInt StoreResult;

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
                    int val;
                    data.TryGetInt(FieldName.Value, out val);
                    StoreResult.Value = val;
				}
				
				StoreResult.Value = GDEDataManager.GetInt(FieldKey, StoreResult.Value);
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