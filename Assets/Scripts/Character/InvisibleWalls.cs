using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InvisibleWalls : MonoBehaviour {

    [HideInInspector]
    public float detectionDistance = 3;
    public float fadeOutValue = 0.2f;
    private float fadeInValue = 1.0f;
    private float fadeTimer = 0.0f;
    private GameObject currentDetectedOccluder;
    private List<GameObject> allDetectedOccluders = new List<GameObject>( );
    private IEnumerator fadeOutCoroutine;
    private IEnumerator fadeInCoroutine;

    public void Update( ) {
        GameObject playerBackAgainstWall = RaycastDetectingOccluder( -Vector3.forward );
        GameObject playerNextToWallOnRight = RaycastDetectingOccluder( Vector3.right );

        if ( !playerNextToWallOnRight && !playerBackAgainstWall ) {
            currentDetectedOccluder = null;
            if ( allDetectedOccluders.Count <= 0 ) {
                return;
            }
            StartCoroutine( FadeInOccluder( allDetectedOccluders[ 0 ] ) );
            fadeTimer = 0.0f;
        }
        if ( allDetectedOccluders.Contains( currentDetectedOccluder )
            || !currentDetectedOccluder ) {
            return;
        }
        if ( fadeTimer < 0.2f ) {
            fadeTimer += Time.deltaTime;
            return;
        }
        allDetectedOccluders.Add( currentDetectedOccluder );
        if ( allDetectedOccluders[ 0 ] != currentDetectedOccluder ) {
            StartCoroutine( FadeInOccluder( allDetectedOccluders[ 0 ] ) );
            allDetectedOccluders.RemoveAt( 0 );
        }
        fadeOutCoroutine = FadeOutOccluder( currentDetectedOccluder );
        StartCoroutine( fadeOutCoroutine );
        fadeTimer = 0.0f;
    }

    private GameObject RaycastDetectingOccluder( Vector3 direction ) {
        RaycastHit hit;
        if ( Physics.Raycast( transform.position, direction, out hit, detectionDistance ) ) {
            if ( hit.collider.gameObject.tag == "Invisibility" ) {
                currentDetectedOccluder = hit.collider.gameObject;
                return hit.collider.gameObject;
            }
        }
        return null;
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
        StopCoroutine( fadeOutCoroutine );

        for ( float i = color.a; i < fadeInValue; ) {
            i += 0.09f;
            color.a = i;
            yield return new WaitForSeconds( 0.0009f );
            occluder.GetComponent<Renderer>( ).material.color = color;
        }
        allDetectedOccluders.Remove( occluder );
    }
}