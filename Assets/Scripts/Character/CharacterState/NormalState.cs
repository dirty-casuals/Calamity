using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class NormalState : CharacterState {

    public float characterMovementSpeed = 5.0f;
    private PlayerController controller;
    private CalamityFirstPersonController firstPersonController;

    private bool playerControllerDisabled;

    public NormalState( GameObject playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        controller = character.GetComponent<PlayerController>( );
        firstPersonController = character.GetComponent<CalamityFirstPersonController>( );
        character.tag = "Player";
    }

    public override void PlayerUpdate( ) {
        if (playerControllerDisabled) {
            return;
        }
        firstPersonController.UpdateCalamityController( );
    }

    public override void PlayerPhysicsUpdate( ) {
        if ( playerControllerDisabled ) {
            return;
        }
        controller.InputHandler( characterMovementSpeed );
        firstPersonController.FixedUpdateCalamityController( );
    }

    public void KnockoutPlayer( ) {
        character.tag = "Monster";
        characterAnimator.SetBool( "KnockOut", true );
        playerControllerDisabled = true;
    }

}