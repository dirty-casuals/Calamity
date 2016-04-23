using UnityEngine;
using UnityEngine.SceneManagement;

public class AsychLevelLoader : MonoBehaviour {

    private GameObject loadingUIPanel;

    private void Awake( ) {
        loadingUIPanel = GetComponentInChildren<CanvasRenderer>( true ).gameObject;
    }

    public void LoadRandomLevel( ) {
        loadingUIPanel.SetActive( true );
        LoadScene( );
    }

    private void LoadScene( ) {
        int randomCount = Random.Range( 0, 0 );
        switch (randomCount) {
            case 0:
                SceneManager.LoadSceneAsync( "1_school" );
                break;
        }
    }
}
