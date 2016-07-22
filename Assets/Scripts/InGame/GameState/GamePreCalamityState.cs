using UnityEngine;

public class GamePreCalamityState : GameState {

    public GamePreCalamityState( GameHandler handler ) : base( handler ) {
        AddUnityObservers( handler.gameObject );
    }

    public override void InitializeGameState( ) {
        if (gameHandler.IsFirstRound( )) {
            gameHandler.SpawnAIPlayers( );
        } else {
            gameHandler.ResetAllThePlayers( );
        }
        gameHandler.RpcSetCalamityLabelText( "Time to Calamity" );
        endTime = gameHandler.startTimeSeconds;
        SetLighting( );
    }

    public override void GameUpdate( ) {
        StartCalamityWhenCountdownReached( );
        SetCountdownTime( );
        gameTimer += Time.deltaTime;
    }

    private void SetLighting( ) {
        Notify( LightsHandler.SET_PRE_CALAMITY_LIGHTING );
    }

    private void StartCalamityWhenCountdownReached( ) {
        if (gameTimer >= endTime) {
            gameTimer = 0.0f;
            Notify( GameHandler.SET_CALAMITY_STATE );
        }
    }
}