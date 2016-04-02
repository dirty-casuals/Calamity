using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndState : GameState {

    public GameEndState( GameHandler handler ) { }

    public override void InitializeGameState( ) {
        SceneManager.LoadScene("GameEndMenu");
    }

}