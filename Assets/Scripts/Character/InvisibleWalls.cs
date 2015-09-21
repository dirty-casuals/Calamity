using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InvisibleWalls : MonoBehaviour {

    public float detectionDistance = 3;
    private List<GameObject> invisibleObjects = new List<GameObject>( );
    private GameObject currentObject;
    private GameObject previousObject;

    public void Update( ) {
        GameObject playerBackAgainstWall = RaycastDetectingOccluder(-Vector3.forward);
        GameObject playerNextToWallOnRight = RaycastDetectingOccluder(Vector3.right);
        if(!playerNextToWallOnRight && !playerBackAgainstWall)
        {
            SetAllTransparentObjectsToOpaque();
            return;
        }
        if (playerBackAgainstWall)
        {
            if (!invisibleObjects.Contains(playerBackAgainstWall) 
                && playerBackAgainstWall.tag == "Invisibility")
            {
                SetObjectAlphaTransparency(playerBackAgainstWall, 0.2f);
                invisibleObjects.Add(playerBackAgainstWall);
            }
        }
        if (playerNextToWallOnRight) {
            if (!invisibleObjects.Contains(playerNextToWallOnRight)
                && playerNextToWallOnRight.tag == "Invisibility")
            {
                SetObjectAlphaTransparency(playerNextToWallOnRight, 0.2f);
                invisibleObjects.Add(playerNextToWallOnRight);
            }
        }
    }

    private GameObject RaycastDetectingOccluder( Vector3 direction ) {
        RaycastHit hit;
        if ( Physics.Raycast( transform.position, direction, out hit, detectionDistance ) ) {
            return hit.collider.gameObject;
        }
        return null;
    }

    private void RaycastTargetHasSwitched( GameObject target ) {
        if ( currentObject != target ) {
            previousObject = currentObject;
            currentObject = target;
            SetObjectAlphaTransparency(currentObject, 1.0f);
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