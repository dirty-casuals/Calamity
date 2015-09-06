using UnityEngine;
using System.Collections;

public class FogOfWar : MonoBehaviour {

    public Transform fogOfWarObject;

	void Update () {
        Vector3 playerPosition = transform.position;
        Vector4 postitionToVector4 = new Vector4( playerPosition.x, playerPosition.y, playerPosition.z, 0 );
        fogOfWarObject.GetComponent<Renderer>( ).material.SetVector( "_Player1_Pos", postitionToVector4 );
	}
}
