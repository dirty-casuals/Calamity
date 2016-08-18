using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CandleLight : CalamityLight {

    protected ParticleSystem candleParticleSystem;
    protected Light candleLight;

    private void Start( ) {
        candleParticleSystem = GetComponentInChildren<ParticleSystem>( );
        candleLight = GetComponentInChildren<Light>( );
    }

    [ClientRpc]
    protected override void RpcSetPreCalamityLighting( ) {
        candleParticleSystem.Stop( );
        if (candleLight != null) {
            candleLight.enabled = false;
        }
    }

    [ClientRpc]
    protected override void RpcSetCalamityLighting( ) {
        candleParticleSystem.Play( );
        if (candleLight != null) {
            candleLight.enabled = true;
        }
    }
}
