using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;


#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
    [Category(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.GetIntActionTooltip)]
    public class GDEGetInt : GDEActionBase
    {	
        public FsmInt StoreResult;

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