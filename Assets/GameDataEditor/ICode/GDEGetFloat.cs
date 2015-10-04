using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
    [Category(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.GetFloatActionTooltip)]
    public class GDEGetFloat : GDEActionBase
    {   
        public FsmFloat StoreResult;
        
        public override void OnEnter()
        {
			try
			{
				Dictionary<string, object> data;
				if (GDEDataManager.Get(ItemName.Value, out data))
				{
					float val;
					data.TryGetFloat(FieldName.Value, out val);
					StoreResult.Value = val;
				}
				
				StoreResult.Value = GDEDataManager.GetFloat(FieldKey, StoreResult.Value);
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
