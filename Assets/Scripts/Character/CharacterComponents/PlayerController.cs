using UnityEngine;
using UnitySampleAssets.CrossPlatformInput;
using UnityEngine.Networking;

public class PlayerController : Subject {
    private GameHandler gameHandler;
    private CharacterState currentPlayerState;
    private PlayerInventory inventory;
    private CharacterStateHandler stateHandler;

    private void Start( ) {
        stateHandler = GetComponent<CharacterStateHandler>( );
        currentPlayerState = stateHandler.currentState;
        inventory = GetComponent<PlayerInventory>( );
        gameHandler = FindObjectOfType<GameHandler>( );
        AddUnityObservers( gameHandler.gameObject );
        gameHandler.AddPlayerController( this );
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

    public void InputHandler( float movementSpeed ) {
        if (!isLocalPlayer) {
            return;
        }
        float horizontal = CrossPlatformInputManager.GetAxisRaw( "Horizontal" );
        float vertical = CrossPlatformInputManager.GetAxisRaw( "Vertical" );
        bool leftMouseButtonActivated = CrossPlatformInputManager.GetButtonDown( "Fire1" );
        bool rightMouseButtonActivated = CrossPlatformInputManager.GetButtonDown( "Fire2" );
        
        if ( inventory.itemForFirstSlot != null ) {
            Item itemInFirstSlot = inventory.itemForFirstSlot.GetComponent<Item>( );
            if ( itemInFirstSlot && !itemInFirstSlot.itemInPlayerHands ) {
                CmdPlaceItemInHands( );
            }
        }
        if (leftMouseButtonActivated) {
            UseItem( );
        }
        if (rightMouseButtonActivated) {
            DisableItem( );
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

    [Command]
    private void CmdPlaceItemInHands( ) {
        if (!inventory.itemForFirstSlot) {
            return;
        }
        Item inventoryItem = inventory.itemForFirstSlot.GetComponent<Item>( );
        inventoryItem.CmdPlaceItemInHand( gameObject );
    }

    private void UseItem( ) {
        if (!inventory.itemForFirstSlot) {
            return;
        }
        inventory.itemForFirstSlot.GetComponent<Item>( ).UseItem( gameObject );
    }

    private void DisableItem( ) {
        if (!inventory.itemForFirstSlot) {
            return;
        }
        inventory.itemForFirstSlot.GetComponent<Item>( ).DisableItem( );
    }

}
