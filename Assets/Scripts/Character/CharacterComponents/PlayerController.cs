using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnitySampleAssets.CrossPlatformInput;
using UnityStandardAssets.ImageEffects;

public class PlayerController : NetworkSubject {
    [SyncVar( hook = "OnAliveChange" )]
    public bool alive = true;
    public Vector3 startPosition;
    public Quaternion startRotation;
    public bool isAMonster = false;

    private GameHandler gameHandler;
    private CharacterState currentPlayerState;
    private PlayerInventory inventory;
    private CharacterStateHandler stateHandler;
    private bool setupRun = false;
    private Animator characterAnimator;
    private ScreenOverlay screenOverlay;

    public override void OnStartLocalPlayer( ) {
        Setup( );

        Camera playerCamera = GetComponentInChildren<Camera>( );
        playerCamera.enabled = true;
    }

    public override void OnStartServer( ) {
        Setup( );
    }

    public void ControllerPause( ) {
        bool pauseMenuToggle = Input.GetKeyDown( KeyCode.Escape );

        if (pauseMenuToggle) {
            currentPlayerState.ToggleControllerInput( );
            characterAnimator.SetFloat( "Speed", 0.0f );
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

    public void ResetPosition( ) {
        if (CompareTag( "PlayerAI" )) {
            transform.position = startPosition;
            transform.rotation = startRotation;
        } else {
            RpcResetPosition( );
        }
    }

    [Server]
    public void Revive( ) {
        gameObject.SetActive( true );
        stateHandler.currentState.RevivePlayer( );
    }

    public void SetNextStateToMonster( PlayerType type ) {
        stateHandler.SetNextStateToMonster( type );
    }

    [Server]
    public void MakeMonsterIfRequired( ) {
        if (stateHandler == null) {
            stateHandler = GetComponent<CharacterStateHandler>( );
        }
        stateHandler.MakeMonsterIfRequired( );
    }

    public void MakeNormal( ) {
        stateHandler.MakeNormal( );
    }

    public bool isMonster( ) {
        return stateHandler.playerType == PlayerType.MONSTER || stateHandler.playerType == PlayerType.AI_MONSTER;
    }

    [ClientRpc]
    public void RpcSetStartPosition( Vector3 position, Quaternion rotation ) {
        if (isLocalPlayer) {
            transform.position = position;
            transform.rotation = rotation;
            startPosition = position;
            startRotation = rotation;
        }
    }

    [Server]
    public void SetDead( ) {
        alive = false;
        NotifySendObject( this, GameHandler.CHARACTER_DIED );
    }

    private void OnAliveChange( bool isAlive ) {
        if (isAMonster || !isLocalPlayer) {
            return;
        }

        alive = isAlive;
        if (isAlive) {
            characterAnimator.SetFloat( "Speed", 1.0f );
            characterAnimator.SetBool( "Die", false );
            if (screenOverlay != null) {
                screenOverlay.enabled = false;
            }
        } else {
            characterAnimator.SetFloat( "Speed", 0.0f );
            characterAnimator.SetBool( "Die", true );
            if (screenOverlay != null) {
                screenOverlay.enabled = true;
            }
        }
    }

    [Server]
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
            characterAnimator.SetFloat( "Speed", 1.0f );
        } else
        if (PlayerIsMovingBackward( vertical )) {
            movingHorizontal = true;
            characterAnimator.SetFloat( "Speed", -1.0f );
        } else {
            characterAnimator.SetFloat( "Speed", 0.0f );
        }

        characterAnimator.SetFloat( "Direction", 0.0f );
        if (!movingHorizontal) {
            if (PlayerIsMovingRight( horizontal )) {
                characterAnimator.SetFloat( "Direction", 1.0f );
            } else
            if (PlayerIsMovingLeft( horizontal )) {
                characterAnimator.SetFloat( "Direction", -1.0f );
            }
        }
    }

    [ClientRpc]
    private void RpcResetPosition( ) {
        if (isLocalPlayer) {
            transform.position = startPosition;
            transform.rotation = startRotation;
        }
    }

    private void Setup( ) {
        characterAnimator = GetComponent<Animator>( );
        stateHandler = GetComponent<CharacterStateHandler>( );
        currentPlayerState = stateHandler.currentState;
        if (!isAMonster) {
            inventory = GetComponent<PlayerInventory>( );
        }
        StartCoroutine( AssignObserverWhenReady( ) );
        if (isLocalPlayer) {
            screenOverlay = GetComponentInChildren<ScreenOverlay>( );
        }
        setupRun = true;
    }

    private IEnumerator AssignObserverWhenReady( ) {
        gameHandler = FindObjectOfType<GameHandler>( );
        while (gameHandler == null) {
            yield return new WaitForEndOfFrame( );
            gameHandler = FindObjectOfType<GameHandler>( );
        }

        if (gameHandler.IsFirstRound( ) && CompareTag( "PlayerAI" )) {
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        AddUnityObservers( gameHandler.gameObject );
        if (!isAMonster) {
            if (!isLocalPlayer && isServer) {
                NotifySendObject( this, GameHandler.NEW_PLAYER );
            } else
            if (isLocalPlayer) {
                NotifySendObject( this, GameHandler.LOCAL_PLAYER );
            }
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
