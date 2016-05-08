using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndState : GameState {

    private GameHandler gameHandler;

    public GameEndState( GameHandler handler ) {
        gameHandler = handler;
    }

    public override void InitializeGameState( ) {
        if (gameHandler.DidAllLose( )) {
            //set something to show all lost on the game end scene?
            if (gameHandler.DidAllDie( )) {
                //set it to show that everyone died
            } else {
                //set it to show that too many survived
            }
        } else {
            //set something to show the winner on the game end scene?
            gameHandler.GetWinner( );
        }
        SceneManager.LoadScene( "GameEndMenu" );
    }

}