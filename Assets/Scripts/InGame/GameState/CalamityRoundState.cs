using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class CalamityRoundState : GameState {

    private List<GameObject> icons = new List<GameObject>( );

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
        gameHandler.RpcSetCalamityLabelText( "Next Round" );
        gameHandler.roundCount.text = gameHandler.currentRound.ToString( );

        PlayerController playerController = gameHandler.GetLocalPlayer( );
        if (playerController.alive && !playerController.isAMonster) {
            gameHandler.nextRoundTextState.text = "Player";
        } else {
            gameHandler.nextRoundTextState.text = "Monster";
        }

        int numAlivePlayers = gameHandler.GetNumberAlivePlayersLeft( );
        int numDeadPlayers = gameHandler.GetNumberDeadPlayers( );
        gameHandler.StartCoroutine( TotUpPlayers( gameHandler.aliveIcon, numAlivePlayers ) );
        gameHandler.StartCoroutine( TotUpPlayers( gameHandler.deadIcon, numDeadPlayers ) );

        gameHandler.MakeNormals( );
        gameHandler.ResetAllThePlayers( );
        gameHandler.roundPanel.SetActive( true );
    }

    public override void GameUpdate( ) {
        StartNewRound( );
        SetCountdownTime( );
        gameTimer += Time.deltaTime;
    }

    private IEnumerator TotUpPlayers( GameObject icon, int num ) {
        int i = 0;
        if (num > 0) {
            icon.SetActive( true );
            i += 1;
        }

        float iconWidth = icon.GetComponent<RawImage>( ).rectTransform.rect.width;
        for (; i < num; i += 1) {
            yield return new WaitForSeconds( 0.25f );
            Vector3 position = icon.transform.position;
            position.Set( position.x + (iconWidth * i), position.y, position.z );
            GameObject newIcon = (GameObject)GameObject.Instantiate( icon, position, icon.transform.rotation );
            newIcon.transform.SetParent( icon.transform.parent );
            icons.Add( newIcon );
        }
    }

    private void StartNewRound( ) {
        if (gameTimer >= endTime) {
            gameHandler.currentRound += 1;
            gameTimer = 0.0f;
            for (int i = 0; i < icons.Count; i += 1) {
                GameObject icon = icons[ i ];
                GameObject.Destroy( icon );
            }
            icons.Clear( );
            gameHandler.roundPanel.SetActive( false );
            Notify( GameHandler.SET_PRE_CALAMITY_STATE );
        }
    }
}
