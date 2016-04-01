﻿using UnityEngine;

public enum PlayerType {
    PLAYER,
    MONSTER,
    AI_MONSTER,
    AI_PLAYER
}

public class CharacterStateHandler : MonoBehaviour {

    public PlayerType playerType;
    public CharacterState currentState;

    private void Awake( ) {
        SetPlayerState( playerType );
    }

    private void FixedUpdate( ) {
        currentState.PlayerPhysicsUpdate( );
    }

    private void Update( ) {
        currentState.PlayerUpdate( );
    }

    private void OnCollisionEnter( Collision collision ) {
        currentState.PlayerCollisionEnter( collision );
    }

    private void SetPlayerState( PlayerType type ) {
        switch ( type ) {
            case PlayerType.PLAYER:
                currentState = new NormalState( gameObject );
                break;
            case PlayerType.MONSTER:
                currentState = new MonsterState( gameObject );
                break;
            case PlayerType.AI_MONSTER:
                currentState = new AIMonsterState( gameObject );
                break;
            case PlayerType.AI_PLAYER:
                currentState = new AIState( gameObject );
                break;
        }
    }
}