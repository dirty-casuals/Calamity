using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class MonsterState : CharacterState {

    public float monsterMovementSpeed = 7.0f;
    private CalamityFirstPersonController firstPersonController;

    public MonsterState( GameObject playerBody ) : base( playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        controller = playerBody.GetComponent<PlayerController>( );
        firstPersonController = playerBody.GetComponent<CalamityFirstPersonController>( );
        character.tag = "Monster";
    }

    public override void PlayerUpdate( ) {
        firstPersonController.UpdateCalamityController( );
    }

    public override void PlayerPhysicsUpdate( ) {
        controller.InputHandler( monsterMovementSpeed );
        firstPersonController.FixedUpdateCalamityController( );
    }
}