//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using UnityEngine;
using System.Collections;
using psai.net;

public class PsaiTriggerWhenInRange : PsaiColliderBasedTrigger
{    
    public float triggerRadius = 40;
    public float minimumIntensity = 0.5f;
    public float maximumIntensity = 1.0f;
    public bool scaleIntensityByDistance = true;

    /// <summary>
    /// The distance between the player and this.gameObject. We store this so we only need to calculate it once.
    /// </summary>
    private float _lastDistanceCalculated;


    private void CalculateDistance()
    {     
        if (LocalCollider != null && PlayerCollider != null)
        {            
            Vector3 closestPointOnThisCollider = LocalCollider.ClosestPointOnBounds(PlayerCollider.gameObject.transform.position);
            _lastDistanceCalculated = (closestPointOnThisCollider - PlayerCollider.ClosestPointOnBounds(closestPointOnThisCollider)).magnitude;
        }
        else
        {
            _lastDistanceCalculated = (gameObject.transform.position - PlayerCollider.ClosestPointOnBounds(gameObject.transform.position)).magnitude;
        }
    }

    public override bool EvaluateTriggerCondition()
    {
        CalculateDistance();
        return  (_lastDistanceCalculated < triggerRadius);
    }


    public override float CalculateTriggerIntensity()
    {
        if (scaleIntensityByDistance)
        {
            float distanceRatio = 1.0f - (_lastDistanceCalculated / triggerRadius);
            float triggerIntensity = Mathf.Lerp(minimumIntensity, maximumIntensity, distanceRatio);

            //Debug.Log("distance:" + _lastDistanceCalculated + " radius:" + triggerRadius + " distranceRadio:" + distanceRatio + "  returning triggerIntensity " + triggerIntensity);
            return triggerIntensity;
        }
        else
        {
            return maximumIntensity;
        }
    }


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, this.triggerRadius);
    }
}
