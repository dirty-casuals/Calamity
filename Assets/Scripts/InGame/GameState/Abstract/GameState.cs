using UnityEngine;
using System.Collections;

public class GameState : SubjectObject {

    public GameObject playerCharacter;
    protected float gameTimer;
    protected float endTime;
    protected GameHandler gameHandler;

    public GameState( GameHandler handler ) {
        gameHandler = handler;
    }

    public virtual void InitializeGameState( ) { }

    public virtual void GamePhysicsUpdate( ) { }

    public virtual void GameUpdate( ) { }

    protected void SetCountdownTime( ) {
        float timeToCalamity = endTime - gameTimer;
        gameHandler.SetRoundTimeRemaining( timeToCalamity );
    }
}