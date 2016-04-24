using UnityEngine;
using System.Collections;

public class CharacterState {

    public GameObject character;
    public Animator characterAnimator;
    public Rigidbody characterRigidbody;
    public bool alive = true;

    public virtual void PlayerPhysicsUpdate( ) { }

    public virtual void PlayerUpdate( ) { }

    public virtual void PlayerCollisionEnter( Collision collision ) { }

    public virtual void ToggleControllerInput( ) { }

    public virtual void KnockoutPlayer( ) {
        alive = false;
        characterAnimator.SetBool( "KnockOut", true );
    }
    
    public virtual void SetupNetworkConfig( bool isLocalPlayer ) { }
    
    public virtual void RevivePlayer( ) {
    }
}