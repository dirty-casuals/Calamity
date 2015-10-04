using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.GetColorActionTooltip)]
    public class GDEGetColor : GDEActionBase
    {   
        [UIHint(UIHint.FsmColor)]
        public FsmColor StoreResult;
        
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
                    Color val;
                    data.TryGetColor(FieldName.Value, out val);
                    StoreResult.Value = val;
				}
				
				StoreResult.Value = GDEDataManager.GetColor(FieldKey, StoreResult.Value);
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

