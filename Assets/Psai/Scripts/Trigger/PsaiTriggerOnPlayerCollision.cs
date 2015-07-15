//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using UnityEngine;
using psai.net;
using System.Collections.Generic;

public class PsaiTriggerOnPlayerCollision : PsaiColliderBasedTrigger
{
    public float intensity = 1.0f;

    private void OnTriggerEnter(Collider other) 
    {
        if (other == PlayerCollider)
        {
            TryToFireOneShotTrigger(intensity);
        }
    }

    private void OnTriggerExit(Collider other)
    {
    }

    public override float CalculateTriggerIntensity()
    {
        return this.intensity;
    }

}