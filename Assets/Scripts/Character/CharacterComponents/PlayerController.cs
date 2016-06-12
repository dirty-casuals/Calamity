using UnitySampleAssets.CrossPlatformInput;

public class PlayerController : Subject {
    private GameHandler gameHandler;
    private CharacterState currentPlayerState;
    private PlayerInventory inventory;
    private CharacterStateHandler stateHandler;
    public bool alive = true;

    private void Start( ) {
        stateHandler = GetComponent<CharacterStateHandler>( );
        currentPlayerState = stateHandler.currentState;
        inventory = GetComponent<PlayerInventory>( );
        gameHandler = FindObjectOfType<GameHandler>( );
        AddUnityObservers( gameHandler.gameObject );
        NotifySendObject( this, GameHandler.NEW_PLAYER );
    }

    public void ControllerPause( ) {
        bool pauseMenuToggle = CrossPlatformInputManager.GetButtonDown( "Pause" );

        if (pauseMenuToggle) {
            currentPlayerState.ToggleControllerInput( );
            currentPlayerState.characterAnimator.SetFloat( "Speed", 0.0f );
            Notify( GameHandler.TOGGLE_GAME_PAUSE );
        }
    }

    public void SetNextState( PlayerType type ) {
        stateHandler.SetNextState( type );
    }

    public void UpdateState( ) {
        stateHandler.UpdateState( );
    }

    public bool isMonster( ) {
        return stateHandler.playerType == PlayerType.MONSTER || stateHandler.playerType == PlayerType.AI_MONSTER;
    }

    public void SetDead( ) {
        alive = false;
        NotifySendObject( this, GameHandler.CHARACTER_DIED );
    }

    public bool IsDead( ) {
        return alive == false;
    }

    public void InputHandler( float movementSpeed ) {
        if (!isLocalPlayer) {
            return;
        }
        float horizontal = CrossPlatformInputManager.GetAxisRaw( "Horizontal" );
        float vertical = CrossPlatformInputManager.GetAxisRaw( "Vertical" );
        bool leftMouseButtonActivated = CrossPlatformInputManager.GetButtonDown( "Fire1" );
        bool rightMouseButtonActivated = CrossPlatformInputManager.GetButtonDown( "Fire2" );
        
        if (leftMouseButtonActivated) {
            if (inventory.firstItem.Length <= 0) {
                return;
            }
            inventory.RemoveItemFromInventoryUI( );
            inventory.CmdUseItemInInventory( );
        }
        if (rightMouseButtonActivated) {
            inventory.CmdRemoveItemFromInventory( );
        }
        if (PlayerIsMoving( horizontal, vertical )) {
            currentPlayerState.characterAnimator.SetFloat( "Speed", 1.0f );
        } else {
            currentPlayerState.characterAnimator.SetFloat( "Speed", 0.0f );
        }
    }

    private bool PlayerIsMoving( float horizontal, float vertical ) {
        bool walking = horizontal != 0.0f || vertical != 0.0f;
        return walking;
    }
}
