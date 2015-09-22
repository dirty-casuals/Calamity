using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum OccluderDetectState {

    NO_OCCLUDER_DETECTED,
    OCCLUDER_DETECTED_RIGHT,
    OCCLUDER_DETECTED_BEHIND

}

public class InvisibleWalls : MonoBehaviour {

    [HideInInspector]
    public float detectionDistance = 3;
    public float fadeOutValue = 0.2f;
    private float fadeInValue = 1.0f;
    private GameObject previousDetectedOccluder;
    private GameObject currentDetectedOccluder;
    private bool coroutineStarted = false;

    private OccluderDetectState currentOccluderState;
    private OccluderDetectState previousOccluderState;

    public void Update( ) {
        GameObject playerBackAgainstWall = RaycastDetectingOccluder( -Vector3.forward );
        GameObject playerNextToWallOnRight = RaycastDetectingOccluder( Vector3.right );

        if ( !playerNextToWallOnRight && !playerBackAgainstWall ) {
            currentOccluderState = OccluderDetectState.NO_OCCLUDER_DETECTED;
        }
        if ( playerBackAgainstWall ) {
            currentOccluderState = OccluderDetectState.OCCLUDER_DETECTED_BEHIND;
        } else if ( playerNextToWallOnRight ) {
            currentOccluderState = OccluderDetectState.OCCLUDER_DETECTED_RIGHT;
        }
        if ( ( previousDetectedOccluder != currentDetectedOccluder ) ||
             ( currentOccluderState != previousOccluderState ) ) {
            UpdateOccluderState( );
            previousOccluderState = currentOccluderState;
        }
    }

    private GameObject RaycastDetectingOccluder( Vector3 direction ) {
        RaycastHit hit;
        if ( Physics.Raycast( transform.position, direction, out hit, detectionDistance ) ) {
            if ( hit.collider.gameObject.tag == "Invisibility" ) {
                previousDetectedOccluder = currentDetectedOccluder;
                currentDetectedOccluder = hit.collider.gameObject;
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    private void UpdateOccluderState( ) {
        switch ( currentOccluderState ) {
            case OccluderDetectState.NO_OCCLUDER_DETECTED:
                SetAllTransparentObjectsToOpaque( );
                break;
            case OccluderDetectState.OCCLUDER_DETECTED_BEHIND:
            case OccluderDetectState.OCCLUDER_DETECTED_RIGHT:
                if ( previousDetectedOccluder ) {
                    StartCoroutine( FadeInOccluder( previousDetectedOccluder ) );
                }
                StartCoroutine( FadeOutOccluder( currentDetectedOccluder ) );
                break;
        }
    }

    private void SetAllTransparentObjectsToOpaque( ) {
        if ( currentDetectedOccluder ) {
            StartCoroutine( FadeInOccluder( previousDetectedOccluder ) );
            currentDetectedOccluder = null;
        }
        if ( previousDetectedOccluder ) {
            StartCoroutine( FadeInOccluder( previousDetectedOccluder ) );
            previousDetectedOccluder = null;
        }
    }

    private IEnumerator FadeOutOccluder( GameObject occluder ) {
        Color color = occluder.GetComponent<Renderer>( ).material.color;

        for ( float i = color.a; i > fadeOutValue; ) {
            i -= 0.09f;
            color.a = i;
            yield return new WaitForSeconds( 0.0009f );
            occluder.GetComponent<Renderer>( ).material.color = color;
        }
    }

    private IEnumerator FadeInOccluder( GameObject occluder ) {
        Color color = occluder.GetComponent<Renderer>( ).material.color;

        for ( float i = color.a; i < fadeInValue; ) {
            i += 0.09f;
            color.a = i;
            yield return new WaitForSeconds( 0.0009f );
            occluder.GetComponent<Renderer>( ).material.color = color;
        }
    }
}