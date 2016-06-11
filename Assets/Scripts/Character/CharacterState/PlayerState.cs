﻿using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using RAIN.Entities;

public class PlayerState : CharacterState {

    public float movementSpeed = 5.0f;
    private CalamityFirstPersonController firstPersonController;
    private SkinnedMeshRenderer playerMesh;
    private bool playerControllerDisabled;

    public PlayerState( GameObject playerBody ) : base( playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        firstPersonController = character.GetComponent<CalamityFirstPersonController>( );
        playerMesh = character.GetComponentInChildren<SkinnedMeshRenderer>( );
        controller = playerBody.GetComponent<PlayerController>( );
    }

    public override void CheckPaused( ) {
        controller.ControllerPause( );
    }

    public override void PlayerUpdate( ) {
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
        controller.InputHandler( movementSpeed );
        firstPersonController.FixedUpdateCalamityController( );
    }

    public override void ToggleControllerInput( ) {
        playerControllerDisabled = !playerControllerDisabled;
    }

    public override void SetupNetworkConfig( bool isLocalPlayer ) {
        if (isLocalPlayer) {
            playerMesh.gameObject.layer = LayerMask.NameToLayer( "Player" );
        }
    }
}