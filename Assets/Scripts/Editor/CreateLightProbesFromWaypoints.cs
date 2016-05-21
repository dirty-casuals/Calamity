using UnityEngine;
using System.Collections;
using UnityEditor;
using RAIN.Navigation.Waypoints;
using System.Collections.Generic;

public class CreateLightProbesFromWaypoints : EditorWindow {
    [MenuItem( "Calamity/Light Probes from Waypoints" )]

    private static void Init( ) {
        // Get existing open window or if none, make a new one:
        CreateLightProbesFromWaypoints window = (CreateLightProbesFromWaypoints)EditorWindow.GetWindow( typeof( CreateLightProbesFromWaypoints ), true, "Create Light Probes" );
        window.position = new Rect( (Screen.width / 2) - 125, Screen.height / 2 + 85, 300, 80 );
        window.Show( );
    }

    private void OnGUI( ) {
        GameObject playerRouteObject = GameObject.Find( "Player Route" );
        GameObject lightProbeObject = GameObject.Find( "Light Probes" );
        lightProbeObject.transform.position = playerRouteObject.transform.position;
        WaypointRig playerRoute = playerRouteObject.GetComponent<WaypointRig>( );
        LightProbeGroup lightProbes = lightProbeObject.GetComponent<LightProbeGroup>( );

        IList<Waypoint> waypoints = playerRoute.WaypointSet.Waypoints;
        Vector3[ ] probePositions = new Vector3[ waypoints.Count ];
        for (int i = 0; i < waypoints.Count; i += 1) {
            Waypoint waypoint = waypoints[ i ];
            Vector3 waypointPosition = waypoint.LocalPosition + new Vector3( 0.0f, 0.5f, 0.0f );
            probePositions[ i ] = waypointPosition;
        }

        lightProbes.probePositions = probePositions;
    }
}
