using UnityEngine;
using psai.net;
using System.Collections;

public class PsaiReturnToLastBasicMood : PsaiPlaybackControl
{
    public override void OnSignal()
    {
        // we do not call the Psai API function directly, but have it called by the PsaiCoreManager
        // at the end of the next Tick-loop. This way we make sure that the call is not overridden
        // by any other Trigger hat might still be firing from the same GameObject.
        PsaiCoreManager.Instance.SynchronizedReturnToLastBasicMood(this);
    }
}

