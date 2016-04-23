using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultipleSpawner : Spawner {

    private List<GameObject> currentCharactersCreated;
    public int spawnCount = 1;
    public float spawnRateInSeconds = 1.0f;

    public override void Awake()
    {
        base.Awake( );
        currentCharactersCreated = new List<GameObject>( );
    }

    public override void StartSpawn( ) {
        if (currentCharactersCreated.Count > 0) {
            ReenableCurrentCharacter( );
            return;
        }
        StartCoroutine( StartNewSpawner( ) );
    }

    protected override void ToggleCharacterState( bool currentState ) {
        foreach (GameObject monster in currentCharactersCreated) {
            monster.SetActive( currentState );
        }
    }

    private IEnumerator StartNewSpawner( ) {
        int created = 0;
        while (created < spawnCount) {
            if (!somethingInSpawnPoint) {
                CreateMonster( );
                created++;
            }
            yield return new WaitForSeconds( spawnRateInSeconds );
        }
    }

    private void CreateMonster( ) {
        GameObject spawn = CreateAndGetCharacter( );
        currentCharactersCreated.Add( spawn );
    }

}
