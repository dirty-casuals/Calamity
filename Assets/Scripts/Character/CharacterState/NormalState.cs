using UnityEngine;
using RAIN.Entities;
using UnityStandardAssets.Characters.FirstPerson;
using System;

public class NormalState : CharacterState {

    public float characterMovementSpeed = 5.0f;
    private PlayerController controller;
    private CalamityFirstPersonController firstPersonController;
    private SkinnedMeshRenderer playerMesh;
    private EntityRig playerRig;
    private bool playerControllerDisabled;

    public NormalState( GameObject playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        controller = character.GetComponent<PlayerController>( );
        firstPersonController = character.GetComponent<CalamityFirstPersonController>( );
        playerMesh = character.GetComponentInChildren<SkinnedMeshRenderer>( );
        playerRig = playerBody.GetComponentInChildren<EntityRig>( );
        character.tag = "Player";
    }

    public override void PlayerUpdate( ) {
        controller.ControllerPause( );
        if (playerControllerDisabled) {
            return;
        }
        firstPersonController.UpdateCalamityLookRotation( );
        if (!alive) {
            return;
        }
        firstPersonController.UpdateCalamityController( );
    }

    public override void PlayerPhysicsUpdate( ) {
        if (playerControllerDisabled || !alive) {
            return;
        }
        controller.InputHandler( characterMovementSpeed );
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
    
    public override void KnockoutPlayer( ) {
        base.KnockoutPlayer( );
        controller.SetNextState( PlayerType.MONSTER );
        playerRig.Entity.IsActive = false;
    }

    public override void RevivePlayer( ) {
        base.RevivePlayer( );
    }
}