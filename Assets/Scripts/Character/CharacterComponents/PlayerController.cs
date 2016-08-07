using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnitySampleAssets.CrossPlatformInput;
using UnityStandardAssets.ImageEffects;

public class PlayerController : NetworkSubject {
    public bool alive = true;
    public Vector3 startPosition;
    public bool isAMonster = false;

    private GameHandler gameHandler;
    private CharacterState currentPlayerState;
    private PlayerInventory inventory;
    private CharacterStateHandler stateHandler;
    private bool setupRun = false;

    private void Start( ) {
        Setup( );
    }

    public void ControllerPause( ) {
        bool pauseMenuToggle = CrossPlatformInputManager.GetButtonDown( "Pause" );

        if (pauseMenuToggle) {
            currentPlayerState.ToggleControllerInput( );
            currentPlayerState.characterAnimator.SetFloat( "Speed", 0.0f );
            Notify( GameHandler.TOGGLE_GAME_PAUSE );
        }
    }

    public void RunCameraEffects( ) {
        Camera camera = GetComponentInChildren<Camera>( );

        if (camera != null) {
            BlurOptimized blur = camera.GetComponent<BlurOptimized>( );
            if (blur != null) {
                blur.enabled = true;
                blur.blurSize = 10.0f;
                StartCoroutine( DecreaseAndRemoveBlur( blur ) );
            }
        }
    }

    public void Revive( ) {
        if(!setupRun) {
            Setup( );
        }

        if (isLocalPlayer) {
            GetComponentInChildren<ScreenOverlay>( ).enabled = false;
        }
        gameObject.SetActive( true );
        stateHandler.currentState.RevivePlayer( );
    }

    public void Die( ) {
        if (isLocalPlayer) {
            GetComponentInChildren<ScreenOverlay>( ).enabled = true;
        }
    }

    public void SetNextStateToMonster( PlayerType type ) {
        stateHandler.SetNextStateToMonster( type );
    }

    public void MakeMonsterIfRequired( ) {
        if (!setupRun) {
            Setup( );
        }

        if (stateHandler == null) {
            stateHandler = GetComponent<CharacterStateHandler>( );
        }
        stateHandler.MakeMonsterIfRequired( );
    }

    public void MakeNormal( ) {
        if (!setupRun) {
            Setup( );
        }

        stateHandler.MakeNormal( );
    }

    public bool isMonster( ) {
        return stateHandler.playerType == PlayerType.MONSTER || stateHandler.playerType == PlayerType.AI_MONSTER;
    }

    public void SetDead( ) {
        alive = false;
        NotifySendObject( this, GameHandler.CHARACTER_DIED );
    }

    public void SetAlive( ) {
        alive = true;
    }

    public bool IsDead( ) {
        return alive == false;
    }

    public void InputHandler( float movementSpeed ) {
        float horizontal = CrossPlatformInputManager.GetAxisRaw( "Horizontal" );
        float vertical = CrossPlatformInputManager.GetAxisRaw( "Vertical" );
        bool leftMouseButtonActivated = CrossPlatformInputManager.GetButtonDown( "Fire1" );
        bool rightMouseButtonActivated = CrossPlatformInputManager.GetButtonDown( "Fire2" );

        if (gameObject.tag == "Player" && leftMouseButtonActivated) {
            if (inventory.firstItem.Length <= 0) {
                return;
            }
            inventory.RemoveItemFromInventoryUI( );
            inventory.CmdUseItemInInventory( );
        }
        if (gameObject.tag == "Player" && rightMouseButtonActivated) {
            inventory.CmdRemoveItemFromInventory( );
        }
        bool movingHorizontal = false;
        if (PlayerIsMovingForward( vertical )) {
            movingHorizontal = true;
            currentPlayerState.characterAnimator.SetFloat( "Speed", 1.0f );
        } else
        if (PlayerIsMovingBackward( vertical )) {
            movingHorizontal = true;
            currentPlayerState.characterAnimator.SetFloat( "Speed", -1.0f );
        } else {
            currentPlayerState.characterAnimator.SetFloat( "Speed", 0.0f );
        }

        currentPlayerState.characterAnimator.SetFloat( "Direction", 0.0f );
        if (!movingHorizontal) {
            if (PlayerIsMovingRight( horizontal )) {
                currentPlayerState.characterAnimator.SetFloat( "Direction", 1.0f );
            } else
            if (PlayerIsMovingLeft( horizontal )) {
                currentPlayerState.characterAnimator.SetFloat( "Direction", -1.0f );
            }
        }
    }

    private void Setup( ) {
        stateHandler = GetComponent<CharacterStateHandler>( );
        currentPlayerState = stateHandler.currentState;
        if (!isAMonster) {
            inventory = GetComponent<PlayerInventory>( );
        }
        StartCoroutine( AssignObserverWhenReady( ) );
        setupRun = true;
    }

    private IEnumerator AssignObserverWhenReady( ) {
        gameHandler = FindObjectOfType<GameHandler>( );
        while (gameHandler == null) {
            yield return new WaitForEndOfFrame( );
            gameHandler = FindObjectOfType<GameHandler>( );
        }

        if( gameHandler.IsFirstRound( ) ) {
            startPosition = transform.position;
        }

        AddUnityObservers( gameHandler.gameObject );
        if (!isAMonster) {
            NotifySendObject( this, GameHandler.NEW_PLAYER );
        }
    }

    private IEnumerator DecreaseAndRemoveBlur( BlurOptimized blur ) {
        while (blur.blurSize > 0.0f) {
            yield return new WaitForSeconds( 0.5f );
            blur.blurSize = blur.blurSize - 1.0f;
        }
        blur.enabled = false;
    }

    private bool PlayerIsMovingForward( float vertical ) {
        bool walking = vertical > 0.0f;
        return walking;
    }

    private bool PlayerIsMovingBackward( float vertical ) {
        bool walking = vertical < 0.0f;
        return walking;
    }

    private bool PlayerIsMovingLeft( float horizontal ) {
        bool walking = horizontal < 0.0f;
        return walking;
    }

    private bool PlayerIsMovingRight( float horizontal ) {
        bool walking = horizontal > 0.0f;
        return walking;
    }
}
