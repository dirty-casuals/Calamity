using UnityEngine;

public class GamePreCalamityState : GameState {

    private GameHandler gameHandler;
    private float endTime;

    public GamePreCalamityState( GameHandler handler ) {
        gameHandler = handler;
        AddUnityObservers( handler.gameObject );
    }

    public override void InitializeGameState( ) {
        if (gameHandler.IsFirstRound( )) {
            gameHandler.StartPlayerSpawners( );
        } else {
            gameHandler.ResetAllTheThings( );
        }
        gameHandler.countdownLabel.text = "Time to Calamity";
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

    private void SetCountdownTime( ) {
        float timeToCalamity = endTime - gameTimer;
        gameHandler.countdownTime.text = Mathf.Floor( timeToCalamity ).ToString( );
    }
}