using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AsychLevelLoader : MonoBehaviour {

    private GameObject loadingUIPanel;
    [SerializeField]
    private LobbyManager lobbyManager;

    private void Awake( ) {
        loadingUIPanel = GetComponentInChildren<CanvasRenderer>( true ).gameObject;
    }

    public void LoadRandomLevel( ) {
        loadingUIPanel.SetActive( true );
        LoadScene( );
    }

    public void LoadSchoolScene( ) {
        MusicManager musicManager = FindObjectOfType<MusicManager>( );
        Destroy( musicManager.gameObject );

        loadingUIPanel.SetActive( true );
        lobbyManager.lobbyScene = SceneManager.GetActiveScene( ).name;
        lobbyManager.StartHost( );
        lobbyManager.ServerChangeScene( lobbyManager.playScene );
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
