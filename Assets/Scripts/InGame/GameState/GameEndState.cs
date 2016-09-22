using UnityEngine;
using UnityEngine.Networking;
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
                gameHandler.RpcSetGameResult( EndOfGameResult.ALL_DIED, 0 );
            } else {
                int numSurvivors = gameHandler.GetNumberAlivePlayersLeft( );
                gameHandler.RpcSetGameResult( EndOfGameResult.MANY_SURVIVORS, numSurvivors );
            }
        } else {
            PlayerController winnerPlayerController = gameHandler.GetWinner( );
            if (winnerPlayerController.isLocalPlayer) {
                gameHandler.RpcSetGameResult( EndOfGameResult.LOCAL_PLAYER_WON, 1 );
            } else {
                gameHandler.RpcSetGameResult( EndOfGameResult.OTHER_PLAYER_WON, 1 );
            }
        }
        NetworkManager.singleton.ServerChangeScene( "GameEndMenu" );
    }

}