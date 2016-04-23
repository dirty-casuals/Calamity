using UnityEngine;
using RAIN.Entities;
using UnityStandardAssets.Characters.FirstPerson;

public class NormalState : CharacterState {

    public float characterMovementSpeed = 5.0f;
    private PlayerController controller;
    private CalamityFirstPersonController firstPersonController;
    private SkinnedMeshRenderer playerMesh;
    private EntityRig playerRig;
    private bool playerControllerDisabled;
    private bool playerDied;

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

    public override void SetupNetworkConfig( bool isLocalPlayer ) {
        if (isLocalPlayer) {
            playerMesh.gameObject.layer = LayerMask.NameToLayer( "Player" );
        }
    }

    public void KnockoutPlayer( ) {
        character.tag = "Monster";
        characterAnimator.SetBool( "KnockOut", true );
        playerRig.Entity.IsActive = false;
        playerDied = true;
    }

}