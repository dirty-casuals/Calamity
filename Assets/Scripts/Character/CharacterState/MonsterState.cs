using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class MonsterState : CharacterState {

    public float monsterMovementSpeed = 7.0f;
    private CalamityFirstPersonController firstPersonController;
    private bool playerControllerDisabled;

    public MonsterState( GameObject playerBody ) : base( playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        controller = playerBody.GetComponent<PlayerController>( );
        firstPersonController = playerBody.GetComponent<CalamityFirstPersonController>( );
        character.tag = "Monster";
    }

    public override void PlayerUpdate( ) {
        controller.ControllerPause( );
        if (playerControllerDisabled) {
            return;
        }
        firstPersonController.UpdateCalamityLookRotation( );
        if (controller.IsDead( )) {
            return;
        }
        firstPersonController.UpdateCalamityController( );
    }

    public override void PlayerPhysicsUpdate( ) {
        if (playerControllerDisabled || controller.IsDead( )) {
            return;
        }
        controller.InputHandler( monsterMovementSpeed );
        firstPersonController.FixedUpdateCalamityController( );
    }

    public override void ToggleControllerInput( ) {
        playerControllerDisabled = !playerControllerDisabled;
    }
}