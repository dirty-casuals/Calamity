using UnityEngine;
using UnityEngine.Networking;

public enum SpawnpointType {
    PLAYER,
    MONSTER
}

public abstract class Spawner : MonoBehaviour {

    public SpawnpointType spawnType;
    public bool playable = false;
    public bool somethingInSpawnPoint = false;
    public GameObject characterPrefab;

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

    public virtual void UpdateChildrenToSpawnPosition( ) { }

    public Vector3 GetPosition( ) {
        return transform.position;
    }

    protected GameObject CreateAndGetCharacter( ) {
        GameObject spawn = Instantiate( characterPrefab );
        spawn.transform.parent = transform;
        spawn.transform.position = transform.position;
        spawn.transform.localPosition = Vector3.zero;
        NetworkServer.Spawn( spawn );
        return spawn;
    }

    protected abstract void ToggleCharacterState( bool currentState );
}
