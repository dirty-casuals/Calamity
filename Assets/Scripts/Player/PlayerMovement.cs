using UnityEngine;
using UnitySampleAssets.CrossPlatformInput;

public class PlayerMovement : MonoBehaviour {
    public float playerMovementSpeed = 6f;
    Vector3 playerMovementDirection;
    Animator playerAnimator;
    Rigidbody playerRigidbody;
#if !MOBILE_INPUT
    int floorMask;
    float cameraRayCastLength = 100f;
#endif

    void Awake( ) {
#if !MOBILE_INPUT
        floorMask = LayerMask.GetMask( "Floor" );
#endif
        playerAnimator = GetComponent<Animator>( );
        playerRigidbody = GetComponent<Rigidbody>( );
    }

    void FixedUpdate( ) {
        float horizontalInput = CrossPlatformInputManager.GetAxisRaw( "Horizontal" );
        float verticalInput = CrossPlatformInputManager.GetAxisRaw( "Vertical" );

        MovePlayerAlongAxis( horizontalInput, verticalInput );
        TurnPlayerTowardsMouseCursor( );

        // Animate the player.
        //Animating( h, v );
    }


    void MovePlayerAlongAxis( float horizontal, float vertical ) {
        playerMovementDirection.Set( horizontal, 0f, vertical );
        playerMovementDirection = playerMovementDirection.normalized * playerMovementSpeed * Time.deltaTime;
        playerRigidbody.MovePosition( transform.position + playerMovementDirection );
    }


    void TurnPlayerTowardsMouseCursor( ) {
#if !MOBILE_INPUT
        // Create a ray from the mouse cursor on screen in the direction of the camera.
        Ray camRay = Camera.main.ScreenPointToRay( Input.mousePosition );

        // Create a RaycastHit variable to store information about what was hit by the ray.
        RaycastHit floorHit;

        // Perform the raycast and if it hits something on the floor layer...
        if ( Physics.Raycast( camRay, out floorHit, cameraRayCastLength, floorMask ) ) {
            // Create a vector from the player to the point on the floor the raycast from the mouse hit.
            Vector3 playerToMouse = floorHit.point - transform.position;

            // Ensure the vector is entirely along the floor plane.
            playerToMouse.y = 0f;

            // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
            Quaternion newRotatation = Quaternion.LookRotation( playerToMouse );

            // Set the player's rotation to this new rotation.
            playerRigidbody.MoveRotation( newRotatation );
        }
#else

            Vector3 turnDir = new Vector3(CrossPlatformInputManager.GetAxisRaw("Mouse X") , 0f , CrossPlatformInputManager.GetAxisRaw("Mouse Y"));

            if (turnDir != Vector3.zero)
            {
                // Create a vector from the player to the point on the floor the raycast from the mouse hit.
                Vector3 playerToMouse = (transform.position + turnDir) - transform.position;

                // Ensure the vector is entirely along the floor plane.
                playerToMouse.y = 0f;

                // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
                Quaternion newRotatation = Quaternion.LookRotation(playerToMouse);

                // Set the player's rotation to this new rotation.
                playerRigidbody.MoveRotation(newRotatation);
            }
#endif
    }


    void Animating( float h, float v ) {
        // Create a boolean that is true if either of the input axes is non-zero.
        bool walking = h != 0f || v != 0f;

        // Tell the animator whether or not the player is walking.
        playerAnimator.SetBool( "IsWalking", walking );
    }
}