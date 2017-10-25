using UnityEngine;

public class CalamityState : GameState {

    public CalamityState( GameHandler handler ) : base( handler ) {
        gameHandler = handler;
        AddUnityObservers( handler.gameObject );
    }

    public override void InitializeGameState( ) {
        if (!gameHandler.IsFirstRound( )) {
            gameHandler.MakeMonstersIfRequired( );
        }
        gameHandler.StartMonsterSpawners( );
        endTime = gameHandler.calamityLengthSeconds;
        gameHandler.SetCalamityLabelText( "The Calamity" );
        SetLighting( );
        gameHandler.RunCameraEffects( );
    }

    public override void GameUpdate( ) {
        EndCalamityWhenCountdownReached( );
        SetCountdownTime( );
        gameTimer += Time.deltaTime;
    }

    private void SetLighting( ) {
        Notify( LightsHandler.SET_CALAMITY_LIGHTING );
    }

    private void EndCalamityWhenCountdownReached( ) {
        if (gameTimer >= endTime) {
            gameTimer = 0.0f;
            Notify( GameHandler.SET_CALAMITY_END_ROUND );
        }
    }
}