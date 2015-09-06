using UnityEngine;
using System.Collections;
using UnitySampleAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour {

    private CharacterState playerState;
    private Vector3 normalizedMovementDirection;

    private void Start( ) {
        playerState = this.GetComponent<CharacterStateHandler>( ).currentState;
    }

    public void InputHandler( float movementSpeed ) {
        float horizontal = CrossPlatformInputManager.GetAxisRaw( "Horizontal" );
        float vertical = CrossPlatformInputManager.GetAxisRaw( "Vertical" );

        Vector3 movementDirection = GetNormalizedMovementDirection( 
            horizontal, vertical, movementSpeed );
        MovePlayerAlongAxis( movementDirection );

        if ( PlayerIsMoving( horizontal, vertical ) ) {
            playerState.characterAnimator.SetFloat( "Speed", 1.0f );
            RotatePlayer( movementDirection );
        } else {
            playerState.characterAnimator.SetFloat( "Speed", 0.0f );
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
        playerState.characterRigidbody.MovePosition( playerState.character.transform.position + movementDirection );
    }

    private bool PlayerIsMoving( float horizontal, float vertical ) {
        bool walking = horizontal != 0.0f || vertical != 0.0f;
        return walking;
    }

    private void RotatePlayer( Vector3 rotationDirection ) {
        Quaternion rotateTowards = Quaternion.LookRotation( normalizedMovementDirection );
        playerState.character.transform.rotation = Quaternion.RotateTowards( 
            playerState.character.transform.rotation,
            rotateTowards,
            1000 * Time.deltaTime );
    }

}
