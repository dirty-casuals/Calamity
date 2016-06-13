﻿using UnityEngine;
using System.Collections;

public class Pentagram : UnityObserver {

    private Renderer pentagramRenderer;

    public void Awake( ) {
        pentagramRenderer = GetComponent<MeshRenderer>( );
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

    private void SetPreCalamityLighting( ) {
        SetPentagramLight( Color.black, Mathf.LinearToGammaSpace( 4.0f ) );
    }

    private void SetCalamityLighting( ) {
        SetPentagramLight( Color.red, Mathf.LinearToGammaSpace( 4.0f ) );
    }

    private void SetPentagramLight( Color color, float emissionIntensity ) {
        Material[ ] pentagramMaterials = pentagramRenderer.materials;
        pentagramMaterials[ 0 ].color = Color.red;
        pentagramMaterials[ 0 ].SetColor( "_EmissionColor", color  * emissionIntensity );
        pentagramRenderer.materials = pentagramMaterials;
        DynamicGI.SetEmissive( pentagramRenderer, color * emissionIntensity );
    }
}