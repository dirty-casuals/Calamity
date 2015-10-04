using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.GetVec2ActionTooltip)]
    public class GDEGetVector2 : GDEActionBase
    {   
        [UIHint(UIHint.FsmVector2)]
        public FsmVector2 StoreResult;
        
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
                    Vector2 val;
                    data.TryGetVector2(FieldName.Value, out val);
                    StoreResult.Value = val;
				}

				StoreResult.Value = GDEDataManager.GetVector2(FieldKey, StoreResult.Value);
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


