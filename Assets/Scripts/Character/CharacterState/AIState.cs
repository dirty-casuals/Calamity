using UnityEngine;
using System.Collections;

public class AIState : CharacterState {

    public AIState( GameObject AIBody ) { }

    public override void PlayerPhysicsUpdate( ) { }

    public override void PlayerCollisionEnter( Collision collider ) {
        InRangeOfPlayer( collider.gameObject );
    }

    private void InRangeOfPlayer( GameObject collision ) {
        if ( collision.tag == "Player" ) {
            MonsterTransformation transformHandler = collision.GetComponentInParent<MonsterTransformation>( );
            transformHandler.TransformPlayerIntoMonster( );
        }
    }
}