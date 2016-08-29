using UnityEngine;
using System.Collections;

public class AIMonsterState : CharacterState {

    public AIMonsterState( GameObject AIBody ) : base( AIBody ) {
        character = AIBody;
        AIBody.tag = "MonsterAI";
    }

    public override void PlayerPhysicsUpdate( ) { }

    // [Server]
    public override void PlayerCollisionEnter( Collision collider ) {
        KnockoutPlayer( collider.gameObject );
    }
}