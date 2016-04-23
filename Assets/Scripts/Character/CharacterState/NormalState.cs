using UnityEngine;
using RAIN.Entities;
using UnityStandardAssets.Characters.FirstPerson;

public class NormalState : CharacterState {

    public float characterMovementSpeed = 5.0f;
    private PlayerController controller;
    private CalamityFirstPersonController firstPersonController;
    private EntityRig playerRig;
    private bool playerControllerDisabled;
    private bool playerDied;

    public NormalState( GameObject playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        controller = character.GetComponent<PlayerController>( );
        firstPersonController = character.GetComponent<CalamityFirstPersonController>( );
        playerRig = playerBody.GetComponentInChildren<EntityRig>( );
        character.tag = "Player";
    }

    public override void PlayerUpdate( ) {
        controller.ControllerPause( );
        if (playerControllerDisabled) {
            return;
        }
        firstPersonController.UpdateCalamityLookRotation( );
        if (playerDied) {
            return;
        }
        firstPersonController.UpdateCalamityController( );
    }

    public override void PlayerPhysicsUpdate( ) {
        if (playerControllerDisabled || playerDied) {
            return;
        }
        controller.InputHandler( characterMovementSpeed );
        firstPersonController.FixedUpdateCalamityController( );
    }

    public override void ToggleControllerInput( ) {
        playerControllerDisabled = !playerControllerDisabled;
    }

    public void KnockoutPlayer( ) {
        character.tag = "Monster";
        characterAnimator.SetBool( "KnockOut", true );
        playerRig.Entity.IsActive = false;
        playerDied = true;
    }

}