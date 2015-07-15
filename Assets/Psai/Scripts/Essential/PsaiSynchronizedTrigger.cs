using UnityEngine;
using System.Collections;
using psai.net;

/// <summary>
/// This is the Base Class for synchronized One-Shot and continuous Trigger Events.
/// </summary>
public abstract class PsaiSynchronizedTrigger : PsaiTriggerBase
{

    /// <summary>
    /// if set to false the trigger will only fire once in each first tick when the condition was met
    /// </summary> 
    public bool fireContinuously = false;

    /// <summary>
    /// The interval by which the firing condition of this trigger will be evaluated.
    /// </summary>
    /// <remarks>
    /// If this script is synchronized by the PsaiCoreManager, this value will be ignored and the tick interval of the PsaiCoreManager will be used instead.
    /// </remarks>
    public float tickIntervalInSeconds = 0.25f;
    private float _tickCounter;


    /// <summary>
    /// Enable this to have the PsaiCoreManager filter and synchronize this trigger script, to avoid problems caused by overlapping triggers.
    /// </summary>
    /// <remarks>
    /// Set this to true to have PsaiCoreManager evaluate, synchronize and filter all Trigger scripts that are currently firing,
    /// to prevent wild jumps of intensity caused by overlapping Triggers.
    ///</remarks>
    public bool synchronizeByPsaiCoreManager = true;


    /// <summary>
    /// Defines if the PsaiCoreManager shall filter out or add up overlapping Trigger scripts.
    /// </summary>
    /// <remarks>
    /// If set to true, the PsaiCoreManager sum up intensities of overlapping triggers of the same kind.
    /// Use this for instance if you have multiple minor enemies closing in, that add up to a bigger threat.
    /// Set this to false if you wish to only execute the trigger with the highest intensity.    
    /// </remarks>
    public bool addUpIntensities = false;

    /// <summary>
    /// Sets the maximum intensity when adding up intensities from this Trigger:
    /// </summary>
    public float limitIntensitySum = 1.0f;

    public bool overrideMusicDurationInSeconds = false;
    public int musicDurationInSeconds = 10;

    /// <summary>
    /// Setting this to true will disable the Trigger script after firing a single time.
    /// </summary>
    public bool deactivateAfterFiringOnce = false;
    public bool resetHasFiredStateOnDisable = false;

    public bool hasFiredOnce = false;

    /// <summary>
    /// Set this to true if you want to make sure that this Theme will interrupt all the music that might be playing.
    /// </summary>
    /// <remarks>
    /// Please note that even if this is set to true, this Theme may still be interrupted by Triggers firing afterwards.
    /// </remarks>
    public bool interruptAnyTheme = false;


    /// <summary>
    /// Calculates an intensity value that will be used to trigger a psai Theme.
    /// </summary>
    /// <remarks>
    /// Override this method for any custom ContinuousTriggers you may wish to implement to map
    /// the intensity of a game situation to the intensity of the music.   
    /// If the trigger condition failed, the return value must be 0.
    /// </remarks>
    /// <returns>
    /// The trigger intensity between 0.01f and 1.0f.
    /// </returns>
    public abstract float CalculateTriggerIntensity();


    public virtual void OnEnable()
    {
        if (synchronizeByPsaiCoreManager && fireContinuously)
        {
            PsaiCoreManager pcm = PsaiCoreManager.Instance;
            if (pcm != null)
            {
                pcm.RegisterContinuousTrigger(this);
            }
        }

        _triggerConditionsInGameObject = this.gameObject.GetComponents<PsaiTriggerCondition>();
    }

    public virtual void OnDisable()
    {
        if (synchronizeByPsaiCoreManager && fireContinuously)
        {
            PsaiCoreManager pcm = PsaiCoreManager.Instance;
            if (pcm != null)
            {
                pcm.UnregisterContinuousTrigger(this);
            }
        }

        if (resetHasFiredStateOnDisable)
        {
            hasFiredOnce = false;
        }        
    }


    public void Update()
    {
        if (this.fireContinuously && !synchronizeByPsaiCoreManager)
        {
            _tickCounter += Time.deltaTime;
            if (_tickCounter > tickIntervalInSeconds)
            {
                _tickCounter -= tickIntervalInSeconds;

                if (EvaluateAllTriggerConditions())
                {                   
                    float intensity = CalculateTriggerIntensity();
                    if (intensity > 0)
                    {
                        FireDirectOneShotTrigger(intensity);
                    }
                }
            }
        }
    }


    protected void TryToFireOneShotTrigger(float intensity)
    {
        if (!(deactivateAfterFiringOnce && hasFiredOnce))
        {
            if (EvaluateAllTriggerConditions())
            {
                if (this.synchronizeByPsaiCoreManager)
                {
                    FireSynchronizedOneShotTrigger(intensity);
                }
                else
                {
                    FireDirectOneShotTrigger(intensity);
                }
                hasFiredOnce = true;
            }
        }
        else
        {
            if (PsaiCoreManager.Instance.logTriggerScripts)
            {
                Debug.Log("FireSynchronizedOneShotTrigger  (" + this.gameObject.name + ") has been skipped, as it is set to deactivateAfterFiringOnce");
            }
        }
        
    }


    private void FireDirectOneShotTrigger(float intensity)
    {
        if (this.overrideMusicDurationInSeconds && this.musicDurationInSeconds > 0)
        {
            PsaiCore.Instance.TriggerMusicTheme(themeId, intensity, musicDurationInSeconds);
        }
        else
        {
            PsaiCore.Instance.TriggerMusicTheme(themeId, intensity);
        }

        if (PsaiCoreManager.Instance.logTriggerScripts)
        {
            Debug.Log("psai: unsynchronized One-Shot Trigger fired: " + this);
        }
    }


    private void FireSynchronizedOneShotTrigger(float intensity)
    {
        PsaiCoreManager.TriggerCall triggerCall = new PsaiCoreManager.TriggerCall(this, this.themeId, intensity, 0);
        triggerCall.forceImmediateStopBeforeTriggering = this.interruptAnyTheme;
        if (overrideMusicDurationInSeconds && musicDurationInSeconds > 0)
        {
            triggerCall.musicDurationInSeconds = this.musicDurationInSeconds;
        }
        PsaiCoreManager.Instance.RegisterOneShotTriggerCall(triggerCall);
    }
}
