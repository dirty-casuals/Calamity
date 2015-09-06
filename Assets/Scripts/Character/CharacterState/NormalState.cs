using UnityEngine;
using System.Collections;
using UnitySampleAssets.CrossPlatformInput;

public class NormalState : CharacterState {

    public float characterMovementSpeed = 5.0f;
    private PlayerController controller;

    public NormalState( GameObject playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        controller = character.GetComponent<PlayerController>( );
        character.tag = "Player";
    }

    public override void PlayerPhysicsUpdate( ) {
        controller.InputHandler( characterMovementSpeed );
    }
}