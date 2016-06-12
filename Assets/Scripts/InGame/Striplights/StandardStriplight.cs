using UnityEngine;
using System.Collections;

public class StandardStriplight : Striplight {
    
    protected override void SetPreCalamityLighting( ) {
        Material light = LightsHandler.GetLightOnMaterial( );
        SetLight( light.color, Mathf.LinearToGammaSpace( 3.0f ), light );
    }

    protected override void SetCalamityLighting( ) {
        Material light = LightsHandler.GetLightStandardMaterial( );
        SetLight( light.color, Mathf.LinearToGammaSpace( 0.4f ), light );
    }
}
