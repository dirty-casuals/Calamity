using UnityEngine;
using System.Collections;

public class CalamityRoundState : GameState {

    private GameHandler gameHandler;
    private float endTime;

    public CalamityRoundState( GameHandler handler ) {
        gameHandler = handler;
        AddUnityObservers( handler.gameObject );
    }

    public override void InitializeGameState( ) {
        if (gameHandler.currentRound >= gameHandler.numberOfRounds) {
            Notify( GameHandler.SET_END_GAME );
            return;
        }
        endTime = gameHandler.roundLengthSeconds;
        gameHandler.countdownLabel.text = "Next Round";
        gameHandler.roundCount.text = gameHandler.currentRound.ToString( );
        gameHandler.nextRoundTextState.text = "Player";

        int numAlivePlayers = gameHandler.GetNumberAlivePlayersLeft( );
        int numDeadPlayers = gameHandler.GetNumberDeadPlayers( );
        gameHandler.aliveTextCount.text = "";
        gameHandler.deadTextCount.text = "";
        gameHandler.StartCoroutine( TotUpPlayers( gameHandler.aliveTextCount, numAlivePlayers ) );
        gameHandler.StartCoroutine( TotUpPlayers( gameHandler.deadTextCount, numDeadPlayers ) );

        gameHandler.roundPanel.SetActive( true );
    }

    public override void GameUpdate( ) {
        StartNewRound( );
        SetCountdownTime( );
        gameTimer += Time.deltaTime;
    }

    private IEnumerator TotUpPlayers( UnityEngine.UI.Text text, int num ) {
        for (int i = 0; i < num; i += 1) {
            text.text = text.text + "|";
            yield return new WaitForSeconds( 0.25f );
        }
    }

    private void StartNewRound( ) {
        if (gameTimer >= endTime) {
            gameHandler.currentRound += 1;
            gameTimer = 0.0f;
            gameHandler.roundPanel.SetActive( false );
            Notify( GameHandler.SET_PRE_CALAMITY_STATE );
        }
    }

    private void SetCountdownTime( ) {
        float timeToCalamity = endTime - gameTimer;
        gameHandler.countdownTime.text = Mathf.Floor( timeToCalamity ).ToString( );
    }
}
