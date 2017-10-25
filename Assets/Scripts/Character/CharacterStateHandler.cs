using UnityEngine;

public enum PlayerType {
    PLAYER,
    MONSTER,
    AI_MONSTER,
    AI_PLAYER
}

public class CharacterStateHandler : MonoBehaviour {

    public PlayerType playerType;
    public PlayerType initialType;
    public CharacterState initialState;
    public bool hasBecomeMonster = false;

    private PlayerType nextType;
    public CharacterState currentState;
    private CharacterState nextState;
    private GameHandler gameHandler;

    private void Awake( ) {
        currentState = GetPlayerStateFromType( playerType );
        initialState = currentState;
        nextState = currentState;
        nextType = playerType;
        initialType = playerType;
        gameHandler = GameObject.FindObjectOfType<GameHandler>( );
    }

    public void SetNextStateToMonster( PlayerType type ) {
        nextState = GetPlayerStateFromType( type );
        nextType = type;
        hasBecomeMonster = true;
    }

    public void MakeMonsterIfRequired( ) {
        currentState = nextState;
        PlayerType lastType = playerType;
        playerType = nextType;

        if (hasBecomeMonster) {
            PlayerInventory playeInventory = GetComponent<PlayerInventory>( );
            if (playeInventory != null) {
                playeInventory.RemoveItemFromInventoryUI( );
            }
            ReplacePlayerController( );
        }
    }

    public void MakeNormal( ) {
        if (currentState != initialState) {

            currentState = initialState;
            playerType = initialType;

            ReplacePlayerController( );
        }
    }

    public static GameObject GetPrefabInstanceFromType( PlayerType type ) {
        string objectName;
        switch (type) {
            case PlayerType.PLAYER:
                objectName = "PlayerNormal";
                break;
            case PlayerType.MONSTER:
                objectName = "Toothy";
                break;
            case PlayerType.AI_MONSTER:
                objectName = "AIToothy";
                break;
            case PlayerType.AI_PLAYER:
                objectName = "AIPlayerNormal";
                break;
            default:
                objectName = "AIPlayerNormal";
                break;
        }

        string localPath = "Prefabs/Characters/" + objectName;
        Object localInstance = Resources.Load<GameObject>( localPath );
        GameObject instance = Instantiate( localInstance ) as GameObject;

        return instance;
    }

    public CharacterState GetPlayerStateFromType( PlayerType type ) {
        CharacterState state = null;
        switch (type) {
            case PlayerType.PLAYER:
                state = new NormalState( gameObject );
                break;
            case PlayerType.MONSTER:
                state = new MonsterState( gameObject );
                break;
            case PlayerType.AI_MONSTER:
                state = new AIMonsterState( gameObject );
                break;
            case PlayerType.AI_PLAYER:
                state = new AIState( gameObject );
                break;
        }

        return state;
    }

    private void ReplacePlayerController( ) {
        PlayerController oldPlayerController = GetComponent<PlayerController>( );
        GameObject newInstance = GetPrefabInstanceFromType( playerType );
        newInstance.transform.position = gameObject.transform.position;
        newInstance.transform.rotation = gameObject.transform.rotation;

        CharacterStateHandler newStateHandler = newInstance.GetComponent<CharacterStateHandler>( );
        newStateHandler.initialState = initialState;
        newStateHandler.initialType = initialType;
        newStateHandler.nextState = nextState;
        newStateHandler.nextType = nextType;
        newStateHandler.hasBecomeMonster = hasBecomeMonster;

        PlayerController newPlayerController = newInstance.GetComponent<PlayerController>( );
        newPlayerController.startPosition = oldPlayerController.startPosition;
        newPlayerController.startRotation = oldPlayerController.startRotation;

        gameHandler.ReplacePlayerController( newPlayerController, oldPlayerController );

        Destroy( oldPlayerController.gameObject );
        Instantiate( newInstance );
    }

    private void FixedUpdate( ) {
        currentState.PlayerPhysicsUpdate( );
    }

    private void Update( ) {
        currentState.PlayerUpdate( );
    }

    private void OnCollisionEnter( Collision collision ) {
        currentState.PlayerCollisionEnter( collision );
    }
}