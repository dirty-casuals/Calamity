using UnityEngine;
using UnityEngine.Networking;

public class EmergencyLight : Striplight {

    [ClientRpc]
    protected override void RpcSetPreCalamityLighting( ) {
        Material light = LightsHandler.GetLightOffMaterial( );
        SetLight( light.color, 0.0f, light );
    }

    [ClientRpc]
    protected override void RpcSetCalamityLighting( ) {
        Material light = LightsHandler.GetLightOnMaterial( );
        SetLight( light.color, Mathf.LinearToGammaSpace( 3.0f ), light );
    }

}
