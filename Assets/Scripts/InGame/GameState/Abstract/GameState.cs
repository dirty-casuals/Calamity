using UnityEngine;
using System.Collections;

public class GameState : SubjectObject {

    public GameObject playerCharacter;
    public float gameTimer;

    public virtual void InitializeGameState( ) { }

    public virtual void GamePhysicsUpdate( ) { }

    public virtual void GameUpdate( ) { }

}