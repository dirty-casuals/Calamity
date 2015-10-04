using UnityEngine;
using System.Collections;

public class GDEPlaySound : MonoBehaviour {

	AudioSource audioSource;

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
	}

	void OnMouseDown()
	{
		if (Input.GetButton("Fire1"))
		{
			if (audioSource)
				audioSource.Play();
		}
	}
}
