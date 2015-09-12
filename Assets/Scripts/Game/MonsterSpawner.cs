using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour {

    public GameObject spawnPrefab;
    public int spawnCount = 1;
    public float spawnRateInSeconds = 1.0f;
    [HideInInspector]
    public List<GameObject> currentMonstersCreated;

    public void StartSpawner( ) {
        if ( currentMonstersCreated.Count > 0 ) {
            ReenableCurrentMonsters( );
            return;
        }
        for ( int i = 0; i < spawnCount; i++ ) {
            CreateNewMonsters( );
        }
    }

    public void ReenableCurrentMonsters( ) {
        ToggleMonsterState( true );
    }

    public void DisableCurrentMonsters( ) {
        ToggleMonsterState( false );
    }

    private void ToggleMonsterState( bool currentState ) {
        foreach ( GameObject monster in currentMonstersCreated ) {
            monster.SetActive( currentState );
        }
    }

    private void CreateNewMonsters( ) {
        GameObject spawn = GameObject.Instantiate( spawnPrefab );
        spawn.transform.parent = transform;
        spawn.transform.position = transform.position;
        currentMonstersCreated.Add( spawn );
    }

}