using UnityEngine;
using UnityEngine.SceneManagement;

public enum EndOfGameResult {
    ALL_DIED,
    MANY_SURVIVORS,
    LOCAL_PLAYER_WON,
    OTHER_PLAYER_WON
}

public class GameEndState : GameState {

    public GameEndState( GameHandler handler ) : base( handler ) {
    }

    public override void InitializeGameState( ) {
        if (gameHandler.DidAllLose( )) {
            if (gameHandler.DidAllDie( )) {
                gameHandler.SetGameResult( EndOfGameResult.ALL_DIED, 0 );
            } else {
                int numSurvivors = gameHandler.GetNumberAlivePlayersLeft( );
                gameHandler.SetGameResult( EndOfGameResult.MANY_SURVIVORS, numSurvivors );
            }
        } else {
            PlayerController winnerPlayerController = gameHandler.GetWinner( );
            if (winnerPlayerController.alive) {
                gameHandler.SetGameResult( EndOfGameResult.LOCAL_PLAYER_WON, 1 );
            } else {
                gameHandler.SetGameResult( EndOfGameResult.OTHER_PLAYER_WON, 1 );
            }
        }
        SceneLoader.LoadGameEnd( );
    }

}