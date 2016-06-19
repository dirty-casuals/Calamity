using UnityEngine;
using System.Collections;

public abstract class Striplight : CalamityLight {

    protected MeshRenderer meshRenderer;

    public override void InitializeUnityObserver( ) {
        base.InitializeUnityObserver( );
        meshRenderer = GetComponent<MeshRenderer>( );
    }

    protected void SetLight( Color color, float emissionIntensity, Material light ) {
        Material[ ] materials = meshRenderer.materials;
        materials[ 1 ] = light;
        meshRenderer.materials = materials;
        DynamicGI.SetEmissive( meshRenderer, color * emissionIntensity );
    }
}
