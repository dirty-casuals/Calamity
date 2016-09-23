using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AudioControl : MonoBehaviour {

    [SerializeField]
    private GameObject offObject;

    private static AudioControl instance = null;
    private static bool audioEnabled = true;

    public static AudioControl Instance
    {
        get {
            return instance;
        }
    }

    private void Awake( ) {
        offObject.SetActive( !audioEnabled );
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
