using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AudioControl : MonoBehaviour {

    [SerializeField]
    private GameObject offObject;

    private static bool audioEnabled = true;

    private void Awake( ) {
        audioEnabled = true;

    }

    private void OnMouseDown( ) {
        OnToggle( );
    }

    public void OnToggle( ) {
        audioEnabled = !audioEnabled;

        offObject.SetActive( !audioEnabled );
    }

    public static bool Muted( ) {
        return !audioEnabled;
    }
}
