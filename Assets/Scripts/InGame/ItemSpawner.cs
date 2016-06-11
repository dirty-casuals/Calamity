using UnityEngine;
using UnityEngine.Networking;

public enum ItemSpawnType {
    PAPER_BALL,
    KNIFE,
    MANDRAKE
}

public class ItemSpawner : NetworkBehaviour {
    public ItemSpawnType spawnType;
    public float spawnTimer = 15.0f;
    [HideInInspector]
    [SyncVar]
    public bool currentlySpawnedItem;
    private float timeToSpawnItem = 0.0f;

    private void Start( ) {
        if (!isServer) {
            return;
        }
        CmdSpawnItem( );
    }

    private void Update( ) {
        if (!isServer) {
            return;
        }
        if (currentlySpawnedItem) {
            timeToSpawnItem = 0.0f;
        } else if (timeToSpawnItem >= spawnTimer) {
            CmdSpawnItem( );
            timeToSpawnItem = 0.0f;
        } else {
            timeToSpawnItem += Time.deltaTime;
        }
    }

    public void HideItemInSpawnPoint( ) {
        GetComponent<MeshRenderer>( ).enabled = false;
        currentlySpawnedItem = false;
    }

    [Command]
    private void CmdSpawnItem( ) {
        if (currentlySpawnedItem) {
            return;
        }
        ShowItemInSpawnPoint( );
    }

    private void ShowItemInSpawnPoint( ) {
        GetComponent<MeshRenderer>( ).enabled = true;
        currentlySpawnedItem = true;
    }
}