using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InvisibleWalls : MonoBehaviour {

    public float detectionDistance = 3;
    private List<GameObject> invisibleObjects = new List<GameObject>( );
    private GameObject currentObject;

    public void Update( ) {
        MakeWallsBehindPlayerInvisible( );
        MakeRightWallsInvisible( );
    }

    private void MakeWallsBehindPlayerInvisible( ) {
        GameObject hitObject = RaycastDetectingOccluder(-Vector3.forward);
        if ( hitObject ) {
            if ( !invisibleObjects.Contains( hitObject ) && hitObject.tag == "Invisibility" ) {
                SetObjectAlphaTransparency( hitObject, 0.2f );
                invisibleObjects.Add( hitObject );
            }
            RaycastTargetHasSwitched( hitObject );
        } else {
            SetAllTransparentObjectsToOpaque( );
        }
    }

    private void MakeRightWallsInvisible( ) {

    }

    private GameObject RaycastDetectingOccluder( Vector3 direction ) {
        RaycastHit hit;
        if ( Physics.Raycast( transform.position, -Vector3.forward, out hit, detectionDistance ) ) {
            return hit.collider.gameObject;
        }
        return null;
    }

    private void RaycastTargetHasSwitched( GameObject target ) {
        if ( currentObject != target ) {
            currentObject = target;
            SetAllTransparentObjectsToOpaque( );
        }
    }

    private void SetAllTransparentObjectsToOpaque( ) {
        if ( invisibleObjects.Count <= 0 ) {
            return;
        }
        foreach ( GameObject occluder in invisibleObjects ) {
            SetObjectAlphaTransparency( occluder, 1.0f );
        }
        invisibleObjects.Clear( );
    }

    private void SetObjectAlphaTransparency( GameObject occluder, float alpha ) {
        Color color = occluder.GetComponent<Renderer>( ).material.color;
        color.a = alpha;
        occluder.GetComponent<Renderer>( ).material.color = color;
    }
}