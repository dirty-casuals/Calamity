using UnityEngine;
using System.Collections;

public class LightsHandler {
    public const string SET_PRE_CALAMITY_LIGHTING = "SET_PRE_CALAMITY_LIGHTING";
    public const string SET_CALAMITY_LIGHTING = "SET_CALAMITY_LIGHTING";

    private static Material lightOff;
    private static Material lightOn;
    private static Material lightStandard;

    private const string LIGHT_OFF_PATH = "Models/Materials/LightOff";
    private const string LIGHT_ON_PATH = "Models/Materials/LightFullOn";
    private const string LIGHT_PATH = "Models/Materials/Light";

    public static Material GetLightOffMaterial( ) {
        if (lightOff == null) {
            lightOff = Resources.Load<Material>( LIGHT_OFF_PATH );
        }

        return lightOff;
    }

    public static Material GetLightOnMaterial( ) {
        if (lightOn == null) {
            lightOn = Resources.Load<Material>( LIGHT_ON_PATH );
        }

        return lightOn;
    }

    public static Material GetLightStandardMaterial( ) {
        if (lightStandard == null) {
            lightStandard = Resources.Load<Material>( LIGHT_PATH );
        }

        return lightStandard;
    }
}
