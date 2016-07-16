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
    private MeshRenderer itemRenderer;

    public override void OnStartServer( ) {
        itemRenderer = GetComponent<MeshRenderer>( );
        CmdSpawnItem( );
    }

    [ServerCallback]
    private void Update( ) {
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
        itemRenderer.enabled = false;
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
        itemRenderer.enabled = true;
        currentlySpawnedItem = true;
    }
}