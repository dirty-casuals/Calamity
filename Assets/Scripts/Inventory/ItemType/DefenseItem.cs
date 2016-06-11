using UnityEngine;
using GameDataEditor;
using System.Collections;
using UnityEngine.Networking;

public class DefenseItem : Item {

    [HideInInspector]
    [SyncVar]
    public GDEDefenseItemData defenseItemData;
    protected MeshRenderer itemMesh;
    [SyncVar]
    private int cachedCostOfUse;
    AudioSource usedAudio;

    private void Start( ) {
        GDEDataManager.Init( "gde_data" );
        gameObject.tag = "Item";
        currentItemState = ItemState.ITEM_AT_SPAWN_POINT;
        cachedCostOfUse = defenseItemData.CostOfUse;
        usedAudio = GetComponent<AudioSource>( );
        itemMesh = GetComponent<MeshRenderer>( );
        spawnPosition = transform.position;
        spawnParent = transform.parent;

        if (isServer) {
            MoveItemToSpawnPoint( );
        }
    }

    public void OnCollisionEnter( Collision other ) {
        if (currentItemState == ItemState.ITEM_THROWN) {
            if (other.collider.gameObject.layer == LayerMask.NameToLayer( "Floor" )) {
                usedAudio.enabled = true;
            }
        }
    }

    [Command]
    public override void CmdDisableItem( ) {
        currentItemState = ItemState.ITEM_INACTIVE;
        ResetItem( );
    }

    protected virtual IEnumerator HideItemAfterUsePeriod( ) { return null; }

    [Command]
    protected override void CmdItemHasPerished( ) {
        if (defenseItemData.numberOfUses < 0) {
            CmdDisableItem( );
        }
    }

    protected virtual void ResetItem( ) {
        itemMesh.enabled = false;
        MoveItemToSpawnPoint( );
    }

    private void MoveItemToSpawnPoint( ) {
        transform.position = spawnPosition;
        transform.parent = spawnParent;
    }
}