using UnityEngine;
using System.Collections;
using UnitySampleAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour {

    private CharacterState currentPlayerState;
    private PlayerInventory inventory;
    private Vector3 normalizedMovementDirection;

    private void Start( ) {
        currentPlayerState = this.GetComponent<CharacterStateHandler>( ).currentState;
        inventory = this.GetComponent<PlayerInventory>( );
    }

    public void InputHandler( float movementSpeed ) {
        float horizontal = CrossPlatformInputManager.GetAxisRaw( "Horizontal" );
        float vertical = CrossPlatformInputManager.GetAxisRaw( "Vertical" );
        bool leftMouseButtonActivated = CrossPlatformInputManager.GetButtonDown( "Fire1" );

        Vector3 movementDirection = GetNormalizedMovementDirection(
            horizontal, vertical, movementSpeed );
        MovePlayerAlongAxis( movementDirection );

        if ( PlayerIsMoving( horizontal, vertical ) ) {
            currentPlayerState.characterAnimator.SetFloat( "Speed", 1.0f );
            RotatePlayerTowardsMovementDirection( movementDirection );
        } else {
            currentPlayerState.characterAnimator.SetFloat( "Speed", 0.0f );
        }
        if ( leftMouseButtonActivated ) {
            UseItemInFirstSlot( );
        }
    }

    private Vector3 GetNormalizedMovementDirection( float horizontal, float vertical, float speed ) {
        normalizedMovementDirection.Set( horizontal, 0f, vertical );
        normalizedMovementDirection = normalizedMovementDirection.normalized
            * speed
            * Time.deltaTime;
        return normalizedMovementDirection;
    }

    private void MovePlayerAlongAxis( Vector3 movementDirection ) {
        currentPlayerState.characterRigidbody.MovePosition( currentPlayerState.character.transform.position + movementDirection );
    }

    private bool PlayerIsMoving( float horizontal, float vertical ) {
        bool walking = horizontal != 0.0f || vertical != 0.0f;
        return walking;
    }

    private void RotatePlayerTowardsMovementDirection( Vector3 rotationDirection ) {
        Quaternion rotateTowards = Quaternion.LookRotation( normalizedMovementDirection );
        currentPlayerState.character.transform.rotation = Quaternion.RotateTowards(
            currentPlayerState.character.transform.rotation,
            rotateTowards,
            1000 * Time.deltaTime );
    }

    private void UseItemInFirstSlot( ) {
        if ( !inventory.itemForFirstSlot ) {
            return;
        }
        inventory.itemForFirstSlot.UseItem( );
    }

}
