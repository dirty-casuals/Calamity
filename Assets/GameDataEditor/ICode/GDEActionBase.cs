using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
    [Category(GDMConstants.ActionCategory)]
    public abstract class GDEActionBase : StateAction
    {
        [Tooltip(GDMConstants.ItemNameTooltip)]
        public FsmString ItemName;
        
        [Tooltip(GDMConstants.FieldNameTooltip)]
        public FsmString FieldName;
        
        public abstract override void OnEnter();

		protected string FieldKey
		{
			get {
				return ItemName.Value+"_"+FieldName.Value;
			}

			private set {}
		}
    }
}

#endif
