using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
    [Category(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.GetColorActionTooltip)]
    public class GDEGetColor : GDEActionBase
    {   
        public FsmColor StoreResult;
        
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

