using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    public Transform cameraTarget;
    public float cameraSmoothing = 5.0f;
    private Vector3 offset;

    public void Start( ) {
        offset = transform.position - cameraTarget.position;
    }

    public void FixedUpdate( ) {
        Vector3 targetCameraPosition = cameraTarget.position + offset;
        transform.position = Vector3.Lerp( transform.position, targetCameraPosition,
                                           cameraSmoothing * Time.deltaTime );
    }
}