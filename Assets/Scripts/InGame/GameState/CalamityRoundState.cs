using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;

public class CalamityRoundState : GameState {

    public CalamityRoundState( GameHandler handler ) : base( handler ) {
        AddUnityObservers( handler.gameObject );
    }

    public override void InitializeGameState( ) {
        if (gameHandler.currentRound >= gameHandler.numberOfRounds) {
            Notify( GameHandler.SET_END_GAME );
            return;
        }
        gameHandler.DisableMonsters( );

        endTime = gameHandler.roundLengthSeconds;
        gameHandler.SetCalamityLabelText( "Next Round" );
        gameHandler.roundCount.text = gameHandler.currentRound.ToString( );

        PlayerController playerController = GameObject.FindObjectOfType<PlayerController>( );
        if (playerController.alive && !playerController.isAMonster) {
            gameHandler.nextRoundTextState.text = "Player";
        } else {
            gameHandler.nextRoundTextState.text = "Monster";
        }

        int numAlivePlayers = gameHandler.GetNumberAlivePlayersLeft( );
        int numDeadPlayers = gameHandler.GetNumberDeadPlayers( );
        gameHandler.TotUpPlayers( numAlivePlayers, numDeadPlayers );

        gameHandler.ResetAllThePlayers( );
        gameHandler.SetShowEndOfRoundScreen( true );
    }

    public override void GameUpdate( ) {
        StartNewRound( );
        SetCountdownTime( );
        gameTimer += Time.deltaTime;
    }

    private void StartNewRound( ) {
        if (gameTimer >= endTime) {
            gameHandler.currentRound += 1;
            gameTimer = 0.0f;
            gameHandler.DestroyEndOfRoundIcons( );
            gameHandler.SetShowEndOfRoundScreen( false );
            Notify( GameHandler.SET_PRE_CALAMITY_STATE );
        }
    }
}
