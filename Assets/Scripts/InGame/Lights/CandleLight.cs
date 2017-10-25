using UnityEngine;
using System.Collections;

public class CandleLight : CalamityLight {

    protected ParticleSystem candleParticleSystem;
    protected Light candleLight;

    public void Start( ) {
        base.Start();
        candleParticleSystem = GetComponentInChildren<ParticleSystem>( );
        candleLight = GetComponentInChildren<Light>( );
    }

    protected override void SetPreCalamityLighting( ) {
        candleParticleSystem.Stop( );
        if (candleLight != null) {
            candleLight.enabled = false;
        }
    }

    protected override void SetCalamityLighting( ) {
        candleParticleSystem.Play( );
        if (candleLight != null) {
            candleLight.enabled = true;
        }
    }
}
