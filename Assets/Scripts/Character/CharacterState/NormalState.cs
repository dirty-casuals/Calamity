using UnityEngine;
using System.Collections;
using UnitySampleAssets.CrossPlatformInput;

public class NormalState : CharacterState {

    public float characterMovementSpeed = 5.0f;
    private PlayerController controller;
    private bool playerControllerDisabled;

    public NormalState( GameObject playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        controller = character.GetComponent<PlayerController>( );
        character.tag = "Player";
    }

    public override void PlayerPhysicsUpdate( ) {
        if ( playerControllerDisabled ) {
            return;
        }
        controller.InputHandler( characterMovementSpeed );
    }

    public void KnockoutPlayer( ) {
        character.tag = "Monster";
        characterAnimator.SetBool( "KnockOut", true );
        playerControllerDisabled = true;
    }

}