using UnityEngine;
using System.Collections;

namespace GameDataEditor
{
	public class GDERotateGODemo : MonoBehaviour {

		public float Speed = 1f;
		public Vector3 rotateVec3;

		// Update is called once per frame
		void Update () {
			transform.Rotate(rotateVec3 * Time.deltaTime * Speed);
		}
	}
}
