using UnityEngine;
using System.Collections;

public abstract class CharacterState {

    public GameObject character;
    public Animator characterAnimator;
    public Rigidbody characterRigidbody;

    protected PlayerController controller;

    public CharacterState( GameObject body ) {
        controller = body.GetComponent<PlayerController>( );
    }

    public virtual void PlayerPhysicsUpdate( ) {

    }

    public virtual void PlayerUpdate( ) {

    }

    public virtual void PlayerCollisionEnter( Collision collision ) {

    }

    public virtual void ToggleControllerInput( ) {

    }

    public virtual void KnockoutPlayer( ) {
        controller.SetDead( );
        characterAnimator.SetBool( "KnockOut", true );
    }

    public virtual void SetupNetworkConfig( bool isLocalPlayer ) {

    }

    public virtual void RevivePlayer( ) {
        characterAnimator.SetBool( "KnockOut", false );
        characterAnimator.SetBool( "Idle", true );
    }
}