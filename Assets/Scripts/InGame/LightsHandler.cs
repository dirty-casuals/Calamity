using UnityEngine;
using System.Collections;

public class LightsHandler {
    private const string LIGHT_OFF_PATH = "Models/Materials/LightOff";
    private const string LIGHT_ON_PATH = "Models/Materials/LightFullOn";
    private const string LIGHT_PATH = "Models/Materials/Light";

    private GameObject gameObject;
    private MeshRenderer[ ] lightRenderers;
    private Material lightOff;
    private Material lightOn;
    private Material lightStandard;
    private Renderer pentagramRenderer;

    public LightsHandler( GameObject gameObj ) {
        gameObject = gameObj;

        GameObject[ ] striplights = GameObject.FindGameObjectsWithTag( "Striplight" );
        lightRenderers = new MeshRenderer[ striplights.Length ];
        for (int i = 0; i < striplights.Length; i += 1) {
            GameObject striplight = striplights[ i ];
            MeshRenderer renderer = striplight.GetComponent<MeshRenderer>( );
            lightRenderers[ i ] = renderer;
        }
        pentagramRenderer = GameObject.Find( "Pentagram" ).GetComponent<MeshRenderer>( );

        lightOff = Resources.Load<Material>( LIGHT_OFF_PATH );
        lightOn = Resources.Load<Material>( LIGHT_ON_PATH );
        lightStandard = Resources.Load<Material>( LIGHT_PATH );
    }

    public void SetLightsToFull( ) {
        for (int i = 0; i < lightRenderers.Length; i += 1) {
            MeshRenderer renderer = lightRenderers[ i ];
            Material[ ] materials = renderer.materials;
            materials[ 1 ] = lightOn;
            renderer.materials = materials;
            DynamicGI.SetEmissive( renderer, lightStandard.color * Mathf.LinearToGammaSpace( 3.0f ) );
        }
        Material[ ] pentagramMaterials = pentagramRenderer.materials;
        pentagramMaterials[ 0 ].color = Color.black;
        pentagramMaterials[ 0 ].SetColor( "_EmissionColor", Color.black );
        pentagramRenderer.materials = pentagramMaterials;
        DynamicGI.SetEmissive( pentagramRenderer, Color.black );
    }

    public void SetLightsToLow( ) {
        for (int i = 0; i < lightRenderers.Length; i += 1) {
            MeshRenderer renderer = lightRenderers[ i ];
            Material[ ] materials = renderer.materials;
            materials[ 1 ] = lightStandard;
            renderer.materials = materials;
            DynamicGI.SetEmissive( renderer, lightStandard.color * Mathf.LinearToGammaSpace( 0.4f ) );
        }
        Material[ ] pentagramMaterials = pentagramRenderer.materials;
        pentagramMaterials[ 0 ].color = Color.red;
        pentagramMaterials[ 0 ].SetColor( "_EmissionColor", Color.red * Mathf.LinearToGammaSpace( 4.0f ) );
        pentagramRenderer.materials = pentagramMaterials;
        DynamicGI.SetEmissive( pentagramRenderer, Color.red * Mathf.LinearToGammaSpace( 4.0f ) );
    }
}
