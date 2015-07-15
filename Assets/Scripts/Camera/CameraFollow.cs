using UnityEngine;
using System.Collections;

namespace CompleteProject {
    public class CameraFollow : MonoBehaviour {
        public Transform cameraTarget;
        public float cameraSmoothing = 5.0f;
        Vector3 offset;

        void Start( ) {
            offset = transform.position - cameraTarget.position;
        }

        void FixedUpdate( ) {
            Vector3 targetCameraPosition = cameraTarget.position + offset;
            transform.position = Vector3.Lerp( transform.position, targetCameraPosition, 
                                               cameraSmoothing * Time.deltaTime );
        }
    }
}