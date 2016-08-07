using UnityEngine;
using UnityEngine.Networking;

public enum PlayerType {
    PLAYER,
    MONSTER,
    AI_MONSTER,
    AI_PLAYER
}

public class CharacterStateHandler : NetworkBehaviour {

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

    private void Start( ) {
        currentState.SetupNetworkConfig( isLocalPlayer );
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

        gameHandler.ReplacePlayerController( newPlayerController, oldPlayerController );

        if (isLocalPlayer) {
            NetworkServer.ReplacePlayerForConnection( connectionToClient, newInstance, 0 );
            Camera playerCamera = oldPlayerController.GetComponentInChildren<Camera>( );
            playerCamera.enabled = false;

            Camera newCamera = newInstance.GetComponentInChildren<Camera>( );
            newCamera.enabled = true;
        } else {
            NetworkServer.Spawn( newInstance );
        }

        if (isServer) {
            NetworkServer.Destroy( oldPlayerController.gameObject );
        }
    }

    private void FixedUpdate( ) {
        if (!isLocalPlayer) {
            return;
        }
        currentState.PlayerPhysicsUpdate( );
    }

    private void Update( ) {
        if (!isLocalPlayer) {
            return;
        }
        currentState.PlayerUpdate( );
    }

    private void OnCollisionEnter( Collision collision ) {
        currentState.PlayerCollisionEnter( collision );
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
}