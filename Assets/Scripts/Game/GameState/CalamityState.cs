using UnityEngine;
using System.Collections;

public class CalamityState : GameState {

    private GameHandler gameHandler;
    private float endTime;

    public CalamityState( GameHandler handler ) {
        gameHandler = handler;
        endTime = handler.calamityTimeLength;
    }

    public override void InitializeGameState( ) {
        gameHandler.StartMonsterSpawners( );
    }

    public override void GameUpdate( ) {
        EndCalamityWhenCountdownReached( );
        gameTimer += Time.deltaTime;
    }

    private void EndCalamityWhenCountdownReached( ) {
        if ( gameTimer >= endTime ) {
            gameTimer = 0.0f;
            gameHandler.SetEndCalamityState( );
        }
    }
}