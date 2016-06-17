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
    public CharacterState currentState;
    private CharacterState nextState;
    private PlayerType nextType;

    private void Start( ) {
        currentState.SetupNetworkConfig( isLocalPlayer );
    }

    private void Awake( ) {
        currentState = GetPlayerStateFromType( playerType );
        nextState = currentState;
        nextType = playerType;
    }

    public void SetNextState( PlayerType type ) {
        nextState = GetPlayerStateFromType( type );
        nextType = type;
    }

    public void UpdateState( PlayerController playerController ) {
        currentState = nextState;
        PlayerType lastType = playerType;
        playerType = nextType;

        if (lastType != playerType) {
            GameObject newInstance = GetPrefabInstanceFromType( playerType );
            newInstance.transform.position = gameObject.transform.position;
            newInstance.transform.rotation = gameObject.transform.rotation;

            if (isLocalPlayer) {
                Camera playerCamera = playerController.GetComponentInChildren<Camera>( );
                playerCamera.enabled = false;
                //Camera playerCamera = GetComponentInChildren<Camera>( );
                Camera newCamera = newInstance.GetComponentInChildren<Camera>( );
                //playerCamera.transform.parent = newInstance.transform;
                //playerCamera.transform.position = newInstance.transform.localPosition;

                //playerCamera.enabled = false;
                newCamera.enabled = true;
            }
            GameHandler gameHandler = GameObject.FindObjectOfType<GameHandler>( );
            gameHandler.RemovePlayerController( playerController );
            gameHandler.AddPlayerController( newInstance.GetComponent<PlayerController>( ) );
            Destroy( playerController.gameObject );

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

    public GameObject GetPrefabInstanceFromType( PlayerType type ) {
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