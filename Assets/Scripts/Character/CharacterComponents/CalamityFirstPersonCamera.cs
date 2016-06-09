using UnityEngine;
using System.Collections;

public class CalamityFirstPersonCamera : MonoBehaviour {

    [SerializeField]
    private Transform headJoint;
    [SerializeField]
    private Transform cameraControl;
    private Vector3 originalCameraPosition;

    public void Start( ) {
        originalCameraPosition = cameraControl.localPosition;
    }

    public void FixedUpdate( ) {
        cameraControl.position = headJoint.position + (originalCameraPosition.z * headJoint.forward);
    }
}
