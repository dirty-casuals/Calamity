using UnityEngine;
using psai.net;

public class PsaiGenericTrigger : PsaiSynchronizedTrigger
{
    public float intensity = 1.0f;

    public void OnSignal()
    {
        if (EvaluateAllTriggerConditions())
        {
            TryToFireOneShotTrigger(intensity);
        }
        else
        {
            Debug.Log("OneShoteTrigger has been ignored, because at least one Trigger Condition has failed.");
        }
    }

    public override float CalculateTriggerIntensity()
    {
        return this.intensity;
    }
}
