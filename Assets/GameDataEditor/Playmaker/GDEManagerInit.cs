using UnityEngine;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(GDMConstants.ActionCategory)]
    [Tooltip(GDMConstants.InitActionTooltip)]
    public class GDEManagerInit : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip(GDMConstants.GDEDataFilenameTooltip)]
        public FsmString GDEDataFileName;

		[UIHint(UIHint.FsmBool)]
		[Tooltip(GDMConstants.EncryptedCheckboxTooltip)]
		public FsmBool Encrypted;

        public override void Reset()
        {
            GDEDataFileName = null;
        }
        
        public override void OnEnter()
        {
            try
            {
                if (!GDEDataManager.Init(GDEDataFileName.Value, Encrypted.Value))
                    LogError(GDMConstants.ErrorNotInitialized + " " + GDEDataFileName.Value);
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
