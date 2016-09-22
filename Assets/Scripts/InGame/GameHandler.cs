using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameHandler : NetworkObserver {
    public Text countdownLabel;
    public Text countdownTime;
    public Text roundCount;
    public Text nextRoundTextState;
    public GameObject aliveIcon;
    public GameObject deadIcon;
    public GameObject pauseMenu;
    public float startTimeSeconds = 30.0f;
    public float calamityLengthSeconds = 120.0f;
    public float roundLengthSeconds = 30.0f;
    public float numberOfRounds = 3.0f;
    public bool gamePaused;
    [HideInInspector]
    public float currentRound = 1.0f;
    [HideInInspector]
    public Spawner[ ] gameSpawnPoints;
    public const string SET_PRE_CALAMITY_STATE = "SET_PRE_CALAMITY_STATE";
    public const string SET_CALAMITY_STATE = "SET_NEW_CALAMITY_STATE";
    public const string SET_CALAMITY_END_ROUND = "SET_CALAMITY_END_ROUND";
    public const string SET_END_GAME = "SET_END_GAME ";
    public const string TOGGLE_GAME_PAUSE = "TOGGLE_GAME_PAUSE";
    public const string CHARACTER_DIED = "CHARACTER_DIED";
    public const string NEW_PLAYER = "NEW_PLAYER";
    public const string LOCAL_PLAYER = "LOCAL_PLAYER";
    [SyncVar]
    public bool isReadyForPlayerSpawns = false;
    public static GameState currentGameState;

    [SerializeField]
    private GameObject roundPanel;
    [SerializeField]
    private int numMonsters = 2;

    private List<Spawner> playerSpawnPoints;
    private List<Spawner> monsterSpawnPoints;
    private List<Spawner> selectedMonsterSpawnPoints = null;

    private List<PlayerController> characterPlayerControllers = new List<PlayerController>( );
    private PlayerController localPlayerController = null;

    private bool singlePlayer = false;
    private int clientsSpawned = 0;
    private List<GameObject> playerObjects = new List<GameObject>( );
    [SyncVar( hook = "OnRoundTimeRemainingChange" )]
    private float roundTimeRemaining = 0.0f;
    private GameResult gameResultObject = null;

    private static List<GameObject> stateEventObservers = new List<GameObject>( );

    private static GamePreCalamityState preCalamityState;
    private static CalamityState calamityState;
    private static CalamityRoundState nextRoundState;
    private static GameEndState gameEndState;

    private List<GameObject> endOfRoundIcons = new List<GameObject>( );

    [ServerCallback]
    private void Awake( ) {
        preCalamityState = new GamePreCalamityState( this );
        calamityState = new CalamityState( this );
        nextRoundState = new CalamityRoundState( this );
        gameEndState = new GameEndState( this );
    }

    public override void OnStartServer( ) {
        StartCoroutine( Setup( ) );
    }

    private IEnumerator Setup( ) {
        GetGameSpawnPoints( );
        isReadyForPlayerSpawns = true;

        while (clientsSpawned < NetworkManager.singleton.numPlayers) {
            yield return new WaitForEndOfFrame( );
        }

        SetFirstGameState( );
    }

    [ServerCallback]
    private void Update( ) {
        if (clientsSpawned < NetworkManager.singleton.numPlayers) {
            return;
        }

        currentGameState.GameUpdate( );
    }

    public void RequestPlayerSpawn( NetworkInstanceId netId ) {
        GameObject newPlayer = CharacterStateHandler.GetPrefabInstanceFromType( PlayerType.PLAYER );
        GameObject playerPlaceholder = NetworkServer.FindLocalObject( netId );
        NetworkServer.ReplacePlayerForConnection( playerPlaceholder.GetComponent<NetworkIdentity>( ).connectionToClient, newPlayer, 0 );
        playerObjects.Add( newPlayer );
        NetworkServer.Destroy( playerPlaceholder );
        clientsSpawned = clientsSpawned + 1;
    }

    public void SetRoundTimeRemaining( float timeRemaining ) {
        roundTimeRemaining = timeRemaining;
    }

    public void SetLocalPlayerController( PlayerController controller ) {
        localPlayerController = controller;
    }

    public void AddPlayerController( PlayerController controller ) {
        characterPlayerControllers.Add( controller );
    }

    public void RemovePlayerController( PlayerController controller ) {
        characterPlayerControllers.Remove( controller );
    }

    public void ReplacePlayerController( PlayerController newController, PlayerController oldController ) {
        if (newController.isLocalPlayer) {
            localPlayerController = newController;
        }

        int index = characterPlayerControllers.IndexOf( oldController );
        characterPlayerControllers[ index ] = newController;
    }

    [ClientRpc]
    public void RpcTotUpPlayers( int numAlive, int numDead ) {
        StartCoroutine( TotUpPlayers( aliveIcon, numAlive ) );
        StartCoroutine( TotUpPlayers( deadIcon, numDead ) );
    }

    [ClientRpc]
    public void RpcDestroyEndOfRoundIcons( ) {
        for (int i = 0; i < endOfRoundIcons.Count; i += 1) {
            GameObject icon = endOfRoundIcons[ i ];
            GameObject.Destroy( icon );
        }
        endOfRoundIcons.Clear( );
    }

    [ClientRpc]
    public void RpcRunCameraEffects( ) {
        if (localPlayerController != null) {
            localPlayerController.RunCameraEffects( );
        }
    }

    public static void AddObserverToStateEvents( GameObject observer ) {
        preCalamityState.AddUnityObservers( observer );
        calamityState.AddUnityObservers( observer );
        nextRoundState.AddUnityObservers( observer );
        gameEndState.AddUnityObservers( observer );
    }

    public static void RegisterForStateEvents( GameObject observer ) {
        stateEventObservers.Add( observer );
        AddObserverToStateEvents( observer );
    }

    [ClientRpc]
    public void RpcSetCalamityLabelText( string text ) {
        countdownLabel.text = text;
    }

    public override void OnNotify( UnityEngine.Object sender, EventArguments e ) {
        switch (e.eventMessage) {
            case SET_PRE_CALAMITY_STATE:
                currentGameState = preCalamityState;
                currentGameState.InitializeGameState( );
                break;
            case SET_CALAMITY_STATE:
                currentGameState = calamityState;
                currentGameState.InitializeGameState( );
                break;
            case SET_CALAMITY_END_ROUND:
                currentGameState = nextRoundState;
                currentGameState.InitializeGameState( );
                break;
            case SET_END_GAME:
                currentGameState = gameEndState;
                currentGameState.InitializeGameState( );
                break;
            case TOGGLE_GAME_PAUSE:
                TogglePauseMenu( );
                break;
            case CHARACTER_DIED:
                PlayerDied( (PlayerController)sender );
                break;
            case NEW_PLAYER:
                if (currentRound == 1) {
                    AddPlayerController( (PlayerController)sender );
                }
                break;
            case LOCAL_PLAYER:
                SetLocalPlayerController( (PlayerController)sender );
                break;
        }
    }

    public PlayerController GetLocalPlayer( ) {
        PlayerController playerController = null;
        for (int i = 0; i < characterPlayerControllers.Count; i += 1) {
            PlayerController controller = characterPlayerControllers[ i ];
            if (controller.isLocalPlayer) {
                playerController = controller;
                break;
            }
        }
        return playerController;
    }

    public void SpawnAIPlayers( ) {
        int humansCreated = 0;
        GameObject characterPrefab = (GameObject)Resources.Load( "Prefabs/Characters/AIPlayerNormal" );
        foreach (Spawner spawn in playerSpawnPoints) {
            if (humansCreated < NetworkManager.singleton.numPlayers) {
                GameObject playerObject = playerObjects[ humansCreated ];
                playerObject.transform.parent = spawn.transform;
                playerObject.transform.localPosition = Vector3.zero;
                playerObject.transform.localRotation = Quaternion.identity;
                humansCreated = humansCreated + 1;
                playerObject.GetComponent<PlayerController>( ).RpcSetStartPosition( spawn.transform.position, spawn.transform.rotation );
                continue;
            }

            spawn.characterPrefab = characterPrefab;
            spawn.StartSpawn( );
        }
    }

    public void StartMonsterSpawners( ) {
        if (selectedMonsterSpawnPoints == null) {
            selectedMonsterSpawnPoints = new List<Spawner>( );
            for (int i = 0; i < numMonsters; i += 1) {
                int numSpawnpoints = monsterSpawnPoints.Count;
                int index = (int)Mathf.Floor( Random.value * numSpawnpoints );
                Spawner spawner = monsterSpawnPoints[ index ];
                monsterSpawnPoints.RemoveAt( index );
                selectedMonsterSpawnPoints.Add( spawner );
            }
        }

        foreach (Spawner spawn in selectedMonsterSpawnPoints) {
            spawn.characterPrefab = (GameObject)Resources.Load( "Prefabs/Characters/AIToothy" );
            spawn.StartSpawn( );
        }
    }

    public bool IsFirstRound( ) {
        return currentRound == 1;
    }

    public void DisableMonsters( ) {
        for (int i = 0; i < monsterSpawnPoints.Count; i += 1) {
            Spawner spawner = monsterSpawnPoints[ i ];
            spawner.DisableCurrentCharacter( );
        }
    }

    [ClientRpc]
    public void RpcSetShowEndOfRoundScreen( bool enabled ) {
        roundPanel.SetActive( enabled );
    }

    [Server]
    public void ResetAllThePlayers( ) {
        for (int i = 0; i < characterPlayerControllers.Count; i += 1) {
            PlayerController playerController = characterPlayerControllers[ i ];
            playerController.ResetPosition( );
            playerController.Revive( );
        }
    }

    [Server]
    public void MakeMonstersIfRequired( ) {
        for (int i = 0; i < characterPlayerControllers.Count; i += 1) {
            PlayerController playerController = characterPlayerControllers[ i ];
            playerController.MakeMonsterIfRequired( );
        }
    }

    [Server]
    public void MakeNormals( ) {
        for (int i = 0; i < characterPlayerControllers.Count; i += 1) {
            PlayerController playerController = characterPlayerControllers[ i ];
            playerController.MakeNormal( );
        }
    }

    public bool DidAllLose( ) {
        return GetNumberAlivePlayersLeft( ) != 1;
    }

    public bool DidAllDie( ) {
        bool allDied = true;
        for (int i = 0; i < characterPlayerControllers.Count; i += 1) {
            PlayerController playerController = characterPlayerControllers[ i ];
            if (playerController.alive) {
                allDied = false;
                break;
            }
        }

        return allDied;
    }

    public PlayerController GetWinner( ) {
        PlayerController winner = null;
        for (int i = 0; i < characterPlayerControllers.Count; i += 1) {
            PlayerController playerController = characterPlayerControllers[ i ];
            if (playerController.alive) {
                winner = playerController;
                break;
            }
        }

        return winner;
    }

    public int GetNumberAlivePlayersLeft( ) {
        int count = 0;
        for (int i = 0; i < characterPlayerControllers.Count; i += 1) {
            PlayerController playerController = characterPlayerControllers[ i ];
            if (playerController.alive) {
                count = count + 1;
            }
        }
        return count;
    }

    public int GetNumberDeadPlayers( ) {
        return characterPlayerControllers.Count - GetNumberAlivePlayersLeft( );
    }

    public GameResult GetGameResultObject( ) {
        if (gameResultObject == null) {
            gameResultObject = FindObjectOfType<GameResult>( );
        }

        return gameResultObject;
    }

    [ClientRpc]
    public void RpcSetGameResult( EndOfGameResult endResult, int numSurvivors ) {
        GameResult gameResult = GetGameResultObject( );

        switch (endResult) {
            case EndOfGameResult.ALL_DIED:
                gameResult.SetEndResultAllDied( );
                break;

            case EndOfGameResult.MANY_SURVIVORS:
                gameResult.SetEndResultManySurvivors( numSurvivors );
                break;

            case EndOfGameResult.LOCAL_PLAYER_WON:
                gameResult.SetLocalPlayerWon( );
                break;

            case EndOfGameResult.OTHER_PLAYER_WON:
                gameResult.SetOtherPlayerWon( );
                break;
        }
    }

    private IEnumerator TotUpPlayers( GameObject icon, int num ) {
        int i = 0;
        if (num > 0) {
            icon.SetActive( true );
            i += 1;
        }

        float iconWidth = icon.GetComponent<RawImage>( ).rectTransform.rect.width;
        for (; i < num; i += 1) {
            yield return new WaitForSeconds( 0.25f );
            Vector3 position = icon.transform.position;
            position.Set( position.x + (iconWidth * i), position.y, position.z );
            GameObject newIcon = (GameObject)GameObject.Instantiate( icon, position, icon.transform.rotation );
            newIcon.transform.SetParent( icon.transform.parent );
            endOfRoundIcons.Add( newIcon );
        }
    }

    private void PlayerDied( PlayerController playerController ) {
        if (GetNumberAlivePlayersLeft( ) == 0) {
            currentGameState = gameEndState;
            currentGameState.InitializeGameState( );
        }
    }

    private void GetGameSpawnPoints( ) {
        gameSpawnPoints = FindObjectsOfType<Spawner>( );
        monsterSpawnPoints = new List<Spawner>( );
        playerSpawnPoints = new List<Spawner>( );
        for (int i = 0; i < gameSpawnPoints.Length; i += 1) {
            Spawner spawner = gameSpawnPoints[ i ];

            if (spawner.spawnType == SpawnpointType.PLAYER) {
                playerSpawnPoints.Add( spawner );
            } else {
                monsterSpawnPoints.Add( spawner );
            }
        }
    }

    private void SetFirstGameState( ) {
        currentGameState = preCalamityState;
        currentGameState.InitializeGameState( );
    }

    private void TogglePauseMenu( ) {
        gamePaused = !gamePaused;
        pauseMenu.SetActive( gamePaused );
        Cursor.visible = gamePaused;
        if (gamePaused) {
            Cursor.lockState = CursorLockMode.None;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnRoundTimeRemainingChange( float newTimeRemaining ) {
        countdownTime.text = Mathf.Floor( newTimeRemaining ).ToString( );
    }

    //[ClientRpc]
    //private void RpcSetPlayerStartPosition( Vector3 position, Quaternion rotation ) {
    //    StartCoroutine( OnLocalPlayerReady( position, rotation ) );
    //}

    //private IEnumerator OnLocalPlayerReady( Vector3 position, Quaternion rotation ) {
    //    while (localPlayerController == null) {
    //        yield return new WaitForEndOfFrame( );
    //    }

    //    localPlayerController.transform.position = position;
    //    localPlayerController.transform.rotation = rotation;
    //    localPlayerController.SetStartPosition( position, rotation );
    //}
}