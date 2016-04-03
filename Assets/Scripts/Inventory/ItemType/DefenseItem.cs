using UnityEngine;
using System.Collections;
using GameDataEditor;

public class DefenseItem : Item {

    [HideInInspector]
    public GDEDefenseItemData defenseItemData;
    private int cachedCostOfUse;
    AudioSource usedAudio;

    private void Start( ) {
        GDEDataManager.Init( "gde_data" );
        gameObject.tag = "Item";
        currentItemState = ItemState.ITEM_AT_SPAWN_POINT;
        cachedCostOfUse = defenseItemData.CostOfUse;
        spawnVisual.SetActive( true );
        activeVisual.SetActive( false );
        MoveItemToSpawnLocation( );
        usedAudio = GetComponent<AudioSource>( );
    }

    public override void OnNotify( Object sender, EventArguments e ) {
        switch (e.eventMessage) {
            case PlayerInventory.ADDED_ITEM_TO_INVENTORY:
                if (sender != this) {
                    break;
                }
                currentItemState = ItemState.ITEM_IN_PLAYER_INVENTORY;
                spawnVisual.SetActive( false );
                activeVisual.SetActive( false );
                break;
            case PlayerInventory.REMOVED_ITEM_FROM_INVENTORY:
                currentItemState = ItemState.ITEM_AT_SPAWN_POINT;
                defenseItemData.CostOfUse = cachedCostOfUse;
                break;
        }
    }

    public override void RespawnItem( ) {
        spawnVisual.SetActive( true );
        activeVisual.SetActive( false );
    }

    public void OnCollisionEnter( Collision other ) {
        if (currentItemState == ItemState.ITEM_IN_USE) {
            if (other.collider.gameObject.layer == LayerMask.NameToLayer( "Floor" )) {
                usedAudio.enabled = true;
            }
        }
    }

    protected override void ItemHasPerished( ) {
        if (defenseItemData.numberOfUses < 0) {
            currentItemState = ItemState.ITEM_INACTIVE;
        }
    }

    protected void ResetItem( ) {
        MoveItemToSpawnLocation( );
        spawnVisual.SetActive( false );
        activeVisual.SetActive( false );
    }

    private void MoveItemToSpawnLocation( ) {
        transform.position = itemSpawnPoint.transform.position;
        transform.parent = itemSpawnPoint.transform;
    }

}