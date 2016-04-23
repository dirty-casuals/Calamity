using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : UnityObserver {
    public Text countdownLabel;
    public Text countdownTime;
    public Text roundCount;
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
    public static GameState currentGameState;
    private static GamePreCalamityState preCalamityState;
    private static CalamityState calamityState;
    private static CalamityRoundState nextRoundState;
    private static GameEndState gameEndState;
    private List<Spawner> playerSpawnPoints;
    private List<Spawner> monsterSpawnPoints;
    private int numHumanPlayers = 1;

    private void Start( ) {
        preCalamityState = new GamePreCalamityState( this );
        calamityState = new CalamityState( this );
        nextRoundState = new CalamityRoundState( this );
        gameEndState = new GameEndState( this );
        GetGameSpawnPoints( );
        SetFirstGameState( );
    }

    private void Update( ) {
        currentGameState.GameUpdate( );
    }

    public override void OnNotify( Object sender, EventArguments e ) {
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
        }
    }

    public void StartPlayerSpawners( ) {
        int humansCreated = 0;
        foreach (Spawner spawn in playerSpawnPoints) {
            if (humansCreated < numHumanPlayers) {
                spawn.playable = true;
                humansCreated += 1;
            }
            spawn.StartSpawn( );
        }
    }

    public void StartMonsterSpawners( ) {
        foreach (Spawner spawn in monsterSpawnPoints) {
            spawn.StartSpawn( );
        }
    }

    public void StopMonsterSpawners( ) {
        foreach (Spawner spawn in gameSpawnPoints) {
            spawn.DisableCurrentCharacter( );
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