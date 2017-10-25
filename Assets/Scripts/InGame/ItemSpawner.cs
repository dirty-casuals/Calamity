using UnityEngine;

public enum ItemSpawnType {
    PAPER_BALL,
    KNIFE,
    MANDRAKE
}

public class ItemSpawner : MonoBehaviour {
    public ItemSpawnType spawnType;
    public float spawnTimer = 15.0f;
    [HideInInspector]
    public bool currentlySpawnedItem;
    private float timeToSpawnItem = 0.0f;
    private MeshRenderer itemRenderer;

    public void Awake( ) {
        itemRenderer = GetComponent<MeshRenderer>( );
    }

    public void Start( ) {
        SpawnItem( );
    }

    private void Update( ) {
        if (currentlySpawnedItem) {
            timeToSpawnItem = 0.0f;
        } else if (timeToSpawnItem >= spawnTimer) {
            SpawnItem( );
            timeToSpawnItem = 0.0f;
        } else {
            timeToSpawnItem += Time.deltaTime;
        }
    }

    public void HideItemInSpawnPoint( ) {
        itemRenderer.enabled = false;
        currentlySpawnedItem = false;
    }

    private void SpawnItem( ) {
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