using UnityEngine;
using GameDataEditor;

#if GDE_ICODE_SUPPORT

namespace ICode.Actions
{
    [Category(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.InitActionTooltip)]
    public class GDEManagerInit : StateAction
    {
		[Tooltip(GDMConstants.GDEDataFilenameTooltip)]
        public FsmString GDEDataFileName;

		[Tooltip(GDMConstants.EncryptedCheckboxTooltip)]
		public FsmBool Encrypted;

        public override void OnEnter()
        {
			try
			{
				if (!GDEDataManager.Init(GDEDataFileName.Value, Encrypted.Value))
					Debug.LogError(GDMConstants.ErrorNotInitialized + " " + GDEDataFileName.Value);
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
