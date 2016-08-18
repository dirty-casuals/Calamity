using UnityEngine;
using System.Collections;

public class AIMonsterState : CharacterState {

    public AIMonsterState( GameObject AIBody ) : base( AIBody ) {
        character = AIBody;
        characterAnimator = character.GetComponent<Animator>( );
        AIBody.tag = "MonsterAI";
    }

    public override void PlayerPhysicsUpdate( ) { }

    public override void PlayerCollisionEnter( Collision collider ) {
        KnockoutPlayer( collider.gameObject );
    }
}