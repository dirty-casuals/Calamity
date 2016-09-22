using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour {
    private static MusicManager instance = null;
    private AudioSource audioSource;

    public static MusicManager Instance
    {
        get { return instance; }
    }

    private void Awake( ) {
        if (instance != null && instance != this) {
            Destroy( this.gameObject );
            return;
        } else {
            instance = this;
        }

        audioSource = GetComponent<AudioSource>( );
        DontDestroyOnLoad( this.gameObject );
    }

    private void Update( ) {
        float volume = 1.0f;
        if(AudioControl.Muted()) {
            volume = 0.0f;
        }
        audioSource.volume = volume;
    }
}
