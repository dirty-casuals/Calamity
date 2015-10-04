using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
    [Category(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.GetBoolActionTooltip)]
    public class GDEGetBool : GDEActionBase
    {   
        public FsmBool StoreResult;
        
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

