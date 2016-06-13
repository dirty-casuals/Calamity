using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultipleSpawner : Spawner {

    private List<GameObject> currentCharactersCreated;
    public int spawnCount = 1;
    public float spawnRateInSeconds = 1.0f;

    private void Awake( ) {
        currentCharactersCreated = new List<GameObject>( );
    }

    public override void StartSpawn( ) {
        if (currentCharactersCreated.Count > 0) {
            StartCoroutine( RespawnCharacters() );
            return;
        }
        StartCoroutine( StartNewSpawner( ) );
    }

    protected override void ToggleCharacterState( bool currentState ) {
        foreach (GameObject character in currentCharactersCreated) {
            character.SetActive( currentState );
        }
    }

    public IEnumerator RespawnCharacters( ) {
        foreach (GameObject character in currentCharactersCreated) {
            character.gameObject.transform.position = gameObject.GetComponentInParent<Transform>( ).position;
            character.SetActive( true );
            yield return new WaitForSeconds( spawnRateInSeconds );
        }
    }

    private IEnumerator StartNewSpawner( ) {
        int created = 0;
        while (created < spawnCount) {
            if (!somethingInSpawnPoint) {
                CreateMonster( );
                created += 1;
            }
            yield return new WaitForSeconds( spawnRateInSeconds );
        }
    }

    private void CreateMonster( ) {
        GameObject spawn = CreateAndGetCharacter( );
        currentCharactersCreated.Add( spawn );
    }

}
