using UnityEngine;
using psai.net;

public class PsaiOneShotTrigger : PsaiSynchronizedTrigger
{
    public float intensity = 1.0f;

    public PsaiOneShotTrigger()
    {
        this.fireContinuously = false;
    }

    public void OnSignal()
    {
        if (EvaluateAllTriggerConditions())
        {
            TryToFireOneShotTrigger(intensity);
        }
        else
        {
            Debug.Log("PsaiOneShotTrigger of '" + this.gameObject.name + "' has been ignored, because at least one Trigger Condition has failed.");
        }
    }

    public override float CalculateTriggerIntensity()
    {
        return this.intensity;
    }
}

