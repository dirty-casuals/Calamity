using UnityEngine;

public class EmergencyLight : Striplight {

    protected override void SetPreCalamityLighting( ) {
        Material light = LightsHandler.GetLightOffMaterial( );
        SetLight( light.color, 0.0f, light );
    }

    protected override void SetCalamityLighting( ) {
        Material light = LightsHandler.GetLightOnMaterial( );
        SetLight( light.color, Mathf.LinearToGammaSpace( 3.0f ), light );
    }

}
