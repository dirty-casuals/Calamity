using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CandleLight : CalamityLight {

    protected ParticleSystem candleParticleSystem;
    protected Light candleLight;

    public override void OnStartServer( ) {
        base.OnStartServer( );
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
