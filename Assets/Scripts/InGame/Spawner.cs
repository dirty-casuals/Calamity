using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SpawnpointType {
    PLAYER,
    MONSTER
}

public abstract class Spawner : MonoBehaviour {

    public SpawnpointType spawnType;
    public bool playable = false;
    public bool somethingInSpawnPoint = false;
    public GameObject characterPrefab;

    private BoxCollider spawnCollider;

    public virtual void Awake( ) {
        spawnCollider = GetComponent<BoxCollider>( );
    }

    public void OnTriggerEnter( Collider other ) {
        somethingInSpawnPoint = true;
    }

    public void OnTriggerExit( Collider other ) {
        somethingInSpawnPoint = false;
    }

    public abstract void StartSpawn( );

    public void ReenableCurrentCharacter( ) {
        ToggleCharacterState( true );
    }

    public void DisableCurrentCharacter( ) {
        ToggleCharacterState( false );
    }

    public virtual void UpdateChildrenToSpawnPosition() {

    }

    protected GameObject CreateAndGetCharacter( ) {
        GameObject spawn = GameObject.Instantiate( characterPrefab );
        spawn.transform.parent = transform;
        spawn.transform.position = transform.position;

        return spawn;
    }

    protected abstract void ToggleCharacterState( bool currentState );
}
