﻿using UnityEngine;
using System.Collections;

public class AIMonsterState : CharacterState {

    public AIMonsterState( GameObject AIBody ) : base( AIBody ) {
        AIBody.tag = "Monster";
    }

    public override void PlayerPhysicsUpdate( ) { }

    public override void PlayerCollisionEnter( Collision collider ) {
        KnockoutPlayerIfInRange( collider.gameObject );
    }

    private void KnockoutPlayerIfInRange( GameObject collision ) {
        if (collision.tag == "Player") {
            NormalState state = (NormalState)collision.GetComponent<CharacterStateHandler>( ).currentState;
            state.KnockoutPlayer( );
        }
        if (collision.tag == "PlayerAI") {
            AIState state = (AIState)collision.GetComponentInParent<CharacterStateHandler>( ).currentState;
            state.KnockoutPlayer( );
        }
    }
}