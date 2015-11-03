using UnityEngine;

public class MonsterState : CharacterState {

    public float monsterMovementSpeed = 7.0f;
    private PlayerController controller;

    public MonsterState( GameObject playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        controller = playerBody.GetComponent<PlayerController>( );
        character.tag = "Monster";
    }

    public override void PlayerPhysicsUpdate( ) {
        controller.InputHandler( monsterMovementSpeed );
    }
}