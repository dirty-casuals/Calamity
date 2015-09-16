using UnityEngine;
using System.Collections;

public class GamePreCalamityState : GameState {

    private GameHandler gameHandler;
    private float endTime;

    public GamePreCalamityState( GameHandler handler ) {
        gameHandler = handler;
        endTime = handler.startTimeSeconds;
    }

    public override void GameUpdate( ) {
        StartCalamityWhenCountdownReached( );
        gameTimer += Time.deltaTime;
    }

    private void StartCalamityWhenCountdownReached( ) {
        if ( gameTimer >= endTime ) {
            gameTimer = 0.0f;
            gameHandler.SetCalamityState( );
        }
    }
}