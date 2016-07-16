using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class StandardStriplight : Striplight {

    [ClientRpc]
    protected override void RpcSetPreCalamityLighting( ) {
        Material light = LightsHandler.GetLightOnMaterial( );
        SetLight( light.color, Mathf.LinearToGammaSpace( 3.0f ), light );
    }

    [ClientRpc]
    protected override void RpcSetCalamityLighting( ) {
        Material light = LightsHandler.GetLightStandardMaterial( );
        SetLight( light.color, Mathf.LinearToGammaSpace( 0.4f ), light );
    }
}
