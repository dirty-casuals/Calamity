using UnityEngine;
using System.Collections;

public class AssignCamera : MonoBehaviour {

	private void OnLevelWasLoaded() {
        GetComponent<Canvas>( ).worldCamera = FindObjectOfType<Camera>( );
    }
}
