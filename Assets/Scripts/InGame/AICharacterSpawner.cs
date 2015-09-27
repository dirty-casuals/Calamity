using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AICharacterSpawner : MonoBehaviour {

    [HideInInspector]
    public List<GameObject> currentMonstersCreated;
    public GameObject spawnPrefab;
    public int spawnCount = 1;
    public float spawnRateInSeconds = 1.0f;

    public void StartMonsterSpawn( ) {
        if ( currentMonstersCreated.Count > 0 ) {
            ReenableCurrentMonsters( );
            return;
        }
        StartCoroutine( StartNewSpawner( ) );
    }

    public void ReenableCurrentMonsters( ) {
        ToggleMonsterState( true );
    }

    public void DisableCurrentMonsters( ) {
        ToggleMonsterState( false );
    }

    private IEnumerator StartNewSpawner( ) {
        for ( int i = 0; i < spawnCount; i++ ) {
            CreateMonster( );
            yield return new WaitForSeconds( spawnRateInSeconds );
        }
    }

    private void ToggleMonsterState( bool currentState ) {
        foreach ( GameObject monster in currentMonstersCreated ) {
            monster.SetActive( currentState );
        }
    }

    private void CreateMonster( ) {
        GameObject spawn = GameObject.Instantiate( spawnPrefab );
        spawn.transform.parent = transform;
        spawn.transform.position = transform.position;
        currentMonstersCreated.Add( spawn );
    }

}