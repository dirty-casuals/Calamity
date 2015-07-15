using UnityEngine;
using System.Collections;
using psai.net;

public class PsaiTriggerOnButtonDown : PsaiSynchronizedTrigger
{
    public KeyCode triggerKeyCode = KeyCode.Mouse0;
    public float minimumIntensity = 0.1f;
    public float maximumIntensity = 1.0f;
    public float intensityGainPerTick = 0.1f;


    public override bool EvaluateTriggerCondition()
    {
        return (Input.GetKey(this.triggerKeyCode));
    }


	
	public override float CalculateTriggerIntensity() 
    {
        if (PsaiCore.IsInstanceInitialized())
        {
            PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();
            float newIntensity = Mathf.Min(psaiInfo.currentIntensity + intensityGainPerTick, maximumIntensity);
            if (newIntensity < minimumIntensity)
            {
                newIntensity = minimumIntensity;
            }

            Debug.Log("CalculateTriggerInstensity() returns " + newIntensity);

            return newIntensity;
        }

        return 0;
	}

}
