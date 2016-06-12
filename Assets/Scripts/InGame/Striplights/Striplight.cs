using UnityEngine;
using System.Collections;

public abstract class Striplight : UnityObserver {

    protected MeshRenderer meshRenderer;

    public override void InitializeUnityObserver( ) {
        meshRenderer = GetComponent<MeshRenderer>( );
        GameHandler.RegisterForStateEvents( this.gameObject );
    }

    public override void OnNotify( UnityEngine.Object sender, EventArguments e ) {
        switch (e.eventMessage) {
            case LightsHandler.SET_PRE_CALAMITY_LIGHTING:
                SetPreCalamityLighting( );
                break;

            case LightsHandler.SET_CALAMITY_LIGHTING:
                SetCalamityLighting( );
                break;
        }
    }

    protected abstract void SetPreCalamityLighting( );

    protected abstract void SetCalamityLighting( );

    protected void SetLight( Color color, float emissionIntensity, Material light ) {
        Material[ ] materials = meshRenderer.materials;
        materials[ 1 ] = light;
        meshRenderer.materials = materials;
        DynamicGI.SetEmissive( meshRenderer, color * emissionIntensity );
    }
}
