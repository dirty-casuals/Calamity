using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityStandardAssets.Network;

public class GameHandler : UnityObserver {
    public Text countdownLabel;
    public Text countdownTime;
    public Text roundCount;
    public Text nextRoundTextState;
    public GameObject aliveIcon;
    public GameObject deadIcon;
    public GameObject roundPanel;
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
    public static GameState currentGameState;
    private static GamePreCalamityState preCalamityState;
    private static CalamityState calamityState;
    private static CalamityRoundState nextRoundState;
    private static GameEndState gameEndState;
    private List<Spawner> playerSpawnPoints;
    private List<Spawner> monsterSpawnPoints;
    private List<PlayerController> playerControllers = new List<PlayerController>( );
    private List<PlayerController> alivePlayerControllers = new List<PlayerController>( );
    private int numHumanPlayers = 1;
    private static List<GameObject> stateEventObservers = new List<GameObject>( );
    private bool networkingStart = false;

    public void Start( ) {
        StartCoroutine( StartOnNetworking( ) );
    }

    private IEnumerator StartOnNetworking( ) {
        while (!NetworkServer.active) {
            yield return new WaitForEndOfFrame( );
        }
        AddObserversToStateEvents( );
        GetGameSpawnPoints( );
        SetFirstGameState( );
        numHumanPlayers = LobbyPlayer.numPlayers;
        networkingStart = true;
    }

    private void Update( ) {
        if (!networkingStart) {
            return;
        }
        currentGameState.GameUpdate( );
    }

    public override void InitializeUnityObserver( ) {
        preCalamityState = new GamePreCalamityState( this );
        calamityState = new CalamityState( this );
        nextRoundState = new CalamityRoundState( this );
        gameEndState = new GameEndState( this );
    }

    public void AddPlayerController( PlayerController controller ) {
        playerControllers.Add( controller );
        alivePlayerControllers.Add( controller );
    }

    public void RemovePlayerController( PlayerController controller ) {
        playerControllers.Remove( controller );
    }

    public void RunCameraEffects( ) {
        for (int i = 0; i < playerControllers.Count; i += 1) {
            playerControllers[ i ].RunCameraEffects( );
        }
    }

    public static void AddObserversToStateEvents( ) {
        for (int i = 0; i < stateEventObservers.Count; i += 1) {
            GameObject observer = stateEventObservers[ i ];
            preCalamityState.AddUnityObservers( observer );
            calamityState.AddUnityObservers( observer );
            nextRoundState.AddUnityObservers( observer );
            gameEndState.AddUnityObservers( observer );
        }
    }

    public static void RegisterForStateEvents( GameObject observer ) {
        stateEventObservers.Add( observer );
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
                AddPlayerController( (PlayerController)sender );
                break;
        }
    }

    public PlayerController GetLocalPlayer( ) {
        PlayerController playerController = null;
        for (int i = 0; i < playerControllers.Count; i += 1) {
            PlayerController controller = playerControllers[ i ];
            if (controller.isLocalPlayer) {
                playerController = controller;
                break;
            }
        }
        return playerController;
    }

    public void StartPlayerSpawners( ) {
        int humansCreated = 0;
        foreach (Spawner spawn in playerSpawnPoints) {
            if (humansCreated < numHumanPlayers) {
                humansCreated += 1;
                continue;
            }
            //if (spawn.playable) {
            //    spawn.characterPrefab = (GameObject)Resources.Load( "Prefabs/Characters/PlayerNormal" );
            //} else {
            spawn.characterPrefab = (GameObject)Resources.Load( "Prefabs/Characters/AIPlayerNormal" );
            //}
            spawn.StartSpawn( );
        }
    }

    public void StartMonsterSpawners( ) {
        foreach (Spawner spawn in monsterSpawnPoints) {
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

    public void ResetAllThePlayers( ) {
        for (int i = 0; i < playerControllers.Count; i += 1) {
            PlayerController playerController = playerControllers[ i ];
            if (playerController != null) {
                playerController.gameObject.transform.position = playerController.startPosition;
                playerController.Revive( );
            }
        }
    }

    public void UpdateCharacterStates( ) {
        for (int i = 0; i < playerControllers.Count; i += 1) {
            PlayerController playerController = playerControllers[ i ];
            if (playerController != null) {
                playerController.UpdateState( );
            }
        }
    }

    public bool DidAllLose( ) {
        int count = alivePlayerControllers.Count;
        return count != 1;
    }

    public bool DidAllDie( ) {
        int count = alivePlayerControllers.Count;
        return count == 0;
    }

    public PlayerController GetWinner( ) {
        // this is just a placeholder really
        return alivePlayerControllers[ 0 ];
    }

    public int GetNumberAlivePlayersLeft( ) {
        return alivePlayerControllers.Count;
    }

    public int GetNumberDeadPlayers( ) {
        return playerControllers.Count - alivePlayerControllers.Count;
    }

    private void PlayerDied( PlayerController playerController ) {
        alivePlayerControllers.Remove( playerController );
        if (alivePlayerControllers.Count == 0) {
            currentGameState = gameEndState;
            currentGameState.InitializeGameState( );
        }
    }

    private void GetGameSpawnPoints( ) {
        gameSpawnPoints = GetComponentsInChildren<Spawner>( );
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

}