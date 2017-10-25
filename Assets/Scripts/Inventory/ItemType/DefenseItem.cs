using UnityEngine;
using GameDataEditor;
using System.Collections;

public class DefenseItem : Item {

    [HideInInspector]
    public GDEDefenseItemData defenseItemData;
    protected MeshRenderer itemMesh;
    AudioSource usedAudio;

    private void Start( ) {
        GDEDataManager.Init( "gde_data" );
        gameObject.tag = "Item";
        currentItemState = ItemState.ITEM_AT_SPAWN_POINT;
        usedAudio = GetComponent<AudioSource>( );
        itemMesh = GetComponent<MeshRenderer>( );
        spawnPosition = transform.position;
        spawnParent = transform.parent;
        MoveItemToSpawnPoint( );
    }

    public void OnCollisionEnter( Collision other ) {
        if (currentItemState == ItemState.ITEM_THROWN) {
            if (other.collider.gameObject.layer == LayerMask.NameToLayer( "Floor" )) {
                usedAudio.enabled = true;
            }
        }
    }

    protected virtual IEnumerator HideItemAfterUsePeriod( ) { return null; }

    private void MoveItemToSpawnPoint( ) {
        transform.position = spawnPosition;
        transform.parent = spawnParent;
    }
}