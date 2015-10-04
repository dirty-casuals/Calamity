using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(GDMConstants.ActionCategory)]
    public abstract class GDEActionBase : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip(GDMConstants.ItemNameTooltip)]
        public FsmString ItemName;
        
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip(GDMConstants.FieldNameTooltip)]
        public FsmString FieldName;

        public override void Reset()
        {
            ItemName = null;
            FieldName = null;
        }
        
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
