using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class MonsterState : PlayerState {

    public MonsterState( GameObject playerBody ) : base( playerBody ) {
        movementSpeed = 6.0f;
        character.tag = "Monster";
    }

    // [Server]
    public override void PlayerCollisionEnter( Collision collider ) {
        KnockoutPlayer( collider.gameObject );
    }
}