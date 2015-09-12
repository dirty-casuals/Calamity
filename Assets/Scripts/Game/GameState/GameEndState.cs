using UnityEngine;
using System.Collections;

public class GameEndState : GameState {

    private GameHandler gameHandler;

    public GameEndState( GameHandler handler ) {
        gameHandler = handler;
    }

    public override void InitializeGameState( ) {
        gameHandler.StopMonsterSpawners( );
    }

}