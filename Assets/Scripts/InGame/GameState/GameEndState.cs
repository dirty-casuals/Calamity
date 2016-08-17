using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndState : GameState {

    public GameEndState( GameHandler handler ) : base( handler ) {
    }

    public override void InitializeGameState( ) {
        GameResult gameResult = gameHandler.GetGameResultObject( );

        if (gameHandler.DidAllLose( )) {
            if (gameHandler.DidAllDie( )) {
                gameResult.SetEndResultAllDied( );
            } else {
                int numSurvivors = gameHandler.GetNumberAlivePlayersLeft( );
                gameResult.SetEndResultManySurvivors( numSurvivors );
            }
        } else {
            PlayerController winnerPlayerController = gameHandler.GetWinner( );
            if( winnerPlayerController.isLocalPlayer ) {
                gameResult.SetLocalPlayerWon( );
            } else {
                gameResult.SetOtherPlayerWon( );
            }
        }
        SceneManager.LoadScene( "GameEndMenu" );
    }

}