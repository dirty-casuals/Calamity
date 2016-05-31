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
    }

    public void SetNextState( PlayerType type ) {
        nextState = GetPlayerStateFromType( type );
        nextType = type;
    }

    public void UpdateState() {
        currentState = nextState;
        PlayerType lastType = playerType;
        playerType = nextType;

        if( lastType != playerType ) {
            GameObject newInstance = GetPrefabInstanceFromType( playerType );
            newInstance.transform.position = gameObject.transform.position;
            newInstance.transform.rotation = gameObject.transform.rotation;

            Destroy( gameObject );
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
                objectName = "PlayerNormal";
                break;
            case PlayerType.AI_MONSTER:
                objectName = "Toothy";
                break;
            case PlayerType.AI_PLAYER:
                objectName = "AIPlayerNormal";
                break;
            default:
                objectName = "AIPlayerNormal";
                break;
        }

        string localPath = "Assets/Resources/Prefabs/Characters/" + objectName + ".prefab";
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