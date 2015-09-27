using UnityEngine;
using System.Collections;

public class GameHandler : MonoBehaviour {
    //Make GameHandler a Singleton
    [HideInInspector]
    public AICharacterSpawner[ ] gameSpawnPoints;
    public float startTimeSeconds = 30.0f;
    public float calamityLengthSeconds = 120.0f;
    public static GameState currentGameState;
    private static GamePreCalamityState preCalamityState;
    private static CalamityState calamityState;
    private static GameEndState gameEndState;

    private void Awake( ) {
        preCalamityState = new GamePreCalamityState( this );
        calamityState = new CalamityState( this );
        gameEndState = new GameEndState( this );
        GetGameSpawnPoints( );
        SetPreCalamityState( );
    }

    private void Update( ) {
        currentGameState.GameUpdate( );
    }

    public void SetPreCalamityState( ) {
        currentGameState = preCalamityState;
    }

    public void SetCalamityState( ) {
        currentGameState = calamityState;
        currentGameState.InitializeGameState( );
    }

    public void SetEndCalamityState( ) {
        currentGameState = gameEndState;
        currentGameState.InitializeGameState( );
    }

    public void StartMonsterSpawners( ) {
        foreach ( AICharacterSpawner spawn in gameSpawnPoints ) {
            spawn.StartMonsterSpawn( );
        }
    }

    public void StopMonsterSpawners( ) {
        foreach ( AICharacterSpawner spawn in gameSpawnPoints ) {
            spawn.DisableCurrentMonsters( );
        }
    }

    private void GetGameSpawnPoints( ) {
        gameSpawnPoints = GetComponentsInChildren<AICharacterSpawner>( );
    }

}