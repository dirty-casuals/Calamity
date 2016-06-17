using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

    public void LoadMainMenu( ) {
        SceneManager.LoadScene("_MainMenu");
    }

    public void LoadGameModeSelect( ) {
        SceneManager.LoadScene("GameModeSelect");
    }

    public void LoadGameLevelSelect( ) {
        SceneManager.LoadScene( "GameLevelSelect" );
    }

    public void LoadMultiplayerLobby( ) {
        SceneManager.LoadScene( "MultiplayerLobby" );
    }

    public void LoadDevScene( ) {
        SceneManager.LoadScene("MainDevScene");
    }
}
