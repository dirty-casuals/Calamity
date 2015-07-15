using UnityEngine;
using psai.net;

[System.Serializable]
public class PsaiStopMusic : PsaiPlaybackControl
{
    /// <summary>
    /// Set this to false if you want psai to wake up automatically with the last Basic Mood again, after a period of rest.
    /// </summary>
    /// <remarks>
    /// The duration of the resting period is based on the settings for the last Basic Mood, as defined in the psai Editor.
    /// </remarks>
    public bool keepSilentUntilNextTrigger = true;


    public bool overrideDefaultRestTime = false;
    public int restTimeOverrideSecondsMin = 10;
    public int restTimeOverrideSecondsMax = 30;

    /// <summary>
    /// Call this method execute the StopMusic command.
    /// </summary>
    public override void OnSignal()
    {
        //Debug.LogWarning("PSAI STOP MUSIC - OnSignal()");

        if (PsaiCoreManager.Instance.logTriggerScripts)
        {
            Debug.Log("[" + Time.timeSinceLevelLoad + " ]PsaiCoreManager executing StopMusic() " + this.ToString());
        }

        if (keepSilentUntilNextTrigger)
        {
            PsaiCoreManager.Instance.SynchronizedStopMusic(this);            
        }
        else
        {
            //PsaiCoreManager.Instance.SynchronizedGoToRest(this);
            PsaiCoreManager pcm = PsaiCoreManager.Instance;
            pcm.SynchronizedGoToRest(this);
        }        
    }
}

