using UnityEngine;
using System.Collections;

public abstract class CharacterState {

    public GameObject character;
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
        controller.gameObject.layer = LayerMask.NameToLayer( "Dead" );
    }

    public virtual void RevivePlayer( ) {
        controller.SetAlive( );
        controller.gameObject.layer = LayerMask.NameToLayer( character.tag );
    }

    protected void KnockoutPlayer( GameObject collision ) {
        if (collision.CompareTag( "Player" )) {
            CharacterState state = collision.GetComponent<CharacterStateHandler>( ).currentState;
            state.KnockoutPlayer( );
        }
        if (collision.CompareTag( "PlayerAI" )) {
            CharacterState state = collision.GetComponentInParent<CharacterStateHandler>( ).currentState;
            state.KnockoutPlayer( );
        }
    }
}