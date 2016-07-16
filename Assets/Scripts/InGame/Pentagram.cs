using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Pentagram : NetworkObserver {

    private Renderer pentagramRenderer;

    public override void OnStartServer( ) {
        pentagramRenderer = GetComponent<MeshRenderer>( );
        GameHandler.RegisterForStateEvents( this.gameObject );
    }

    [Server]
    public override void OnNotify( UnityEngine.Object sender, EventArguments e ) {
        switch (e.eventMessage) {
            case LightsHandler.SET_PRE_CALAMITY_LIGHTING:
                RpcSetPreCalamityLighting( );
                break;

            case LightsHandler.SET_CALAMITY_LIGHTING:
                RpcSetCalamityLighting( );
                break;
        }
    }

    [ClientRpc]
    private void RpcSetPreCalamityLighting( ) {
        SetPentagramLight( Color.white, Mathf.LinearToGammaSpace( 2.0f ) );
    }

    [ClientRpc]
    private void RpcSetCalamityLighting( ) {
        SetPentagramLight( Color.red, Mathf.LinearToGammaSpace( 4.0f ) );
    }

    private void SetPentagramLight( Color color, float emissionIntensity ) {
        Material[ ] pentagramMaterials = pentagramRenderer.materials;
        pentagramMaterials[ 0 ].color = color;
        pentagramMaterials[ 0 ].SetColor( "_EmissionColor", color  * emissionIntensity );
        pentagramRenderer.materials = pentagramMaterials;
        DynamicGI.SetEmissive( pentagramRenderer, color * emissionIntensity );
    }
}
