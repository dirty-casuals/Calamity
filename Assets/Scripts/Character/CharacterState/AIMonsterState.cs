using UnityEngine;
using System.Collections;

public class AIMonsterState : CharacterState {

    public AIMonsterState( GameObject AIBody ) { }

    public override void PlayerPhysicsUpdate( ) { }

    public override void PlayerCollisionEnter( Collision collider ) {
        KnockoutPlayerIfInRange( collider.gameObject );
    }

    private void KnockoutPlayerIfInRange( GameObject collision ) {
        if ( collision.tag == "Player" ) {
            NormalState state = ( NormalState )collision.GetComponent<CharacterStateHandler>( ).currentState;
            state.KnockoutPlayer( );

        }
    }
}