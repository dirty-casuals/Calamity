using UnityEngine;

public class CalamityState : GameState {

    private GameHandler gameHandler;
    private float endTime;

    public CalamityState( GameHandler handler ) {
        gameHandler = handler;
        AddUnityObservers( handler.gameObject );
    }

    public override void InitializeGameState( ) {
        if (!gameHandler.IsFirstRound()) {
            gameHandler.UpdateCharacterStates( );
        }
        gameHandler.StartMonsterSpawners( );
        endTime = gameHandler.calamityLengthSeconds;
        gameHandler.countdownLabel.text = "The Calamity";
    }

    public override void GameUpdate( ) {
        EndCalamityWhenCountdownReached( );
        SetCountdownTime( );
        gameTimer += Time.deltaTime;
    }

    private void EndCalamityWhenCountdownReached( ) {
        if (gameTimer >= endTime) {
            gameTimer = 0.0f;
            Notify( GameHandler.SET_CALAMITY_END_ROUND );
        }
    }

    private void SetCountdownTime( ) {
        float timeToCalamity = endTime - gameTimer;
        gameHandler.countdownTime.text = Mathf.Floor( timeToCalamity ).ToString( );
    }
}