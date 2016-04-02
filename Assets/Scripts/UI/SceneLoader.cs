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

    public void LoadingScene( ) {
        SceneManager.LoadScene("GameLoading");
    }

    public void LoadHelpMenu( ) {
        Debug.Log("Help");
    }

    public void LoadSchoolScene( ) {
        SceneManager.LoadScene( "1_school" );
    }

    public void LoadDevScene( ) {
        SceneManager.LoadScene("MainDevScene");
    }
}
