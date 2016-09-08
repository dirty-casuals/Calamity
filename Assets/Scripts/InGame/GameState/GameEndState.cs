using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameEndState : GameState {

    public GameEndState( GameHandler handler ) : base( handler ) {
    }

    public override void InitializeGameState( ) {
        gameHandler.RpcSetGameResult( );
        NetworkManager.singleton.ServerChangeScene( "GameEndMenu" );
    }

}