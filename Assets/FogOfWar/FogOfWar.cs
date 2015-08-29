using UnityEngine;
using System.Collections;

public class FogOfWar : MonoBehaviour {

    public Transform fogOfWarObject;

	void Update () {
        Vector4 test = new Vector4(this.transform.position.x, this.transform.position.y, this.transform.position.z, 0 );
        fogOfWarObject.GetComponent<Renderer>( ).material.SetVector( "_Player1_Pos", test );
	}
}
