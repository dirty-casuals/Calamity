using UnityEngine;
using GameDataEditor;
using System.Collections;

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
            case PlayerInventory.ITEM_THROWN_BY_PLAYER:
                defenseItemData.CostOfUse = cachedCostOfUse;
                break;
            
        }
    }

    public void OnCollisionEnter( Collision other ) {
        if (currentItemState == ItemState.ITEM_THROWN) {
            if (other.collider.gameObject.layer == LayerMask.NameToLayer( "Floor" )) {
                usedAudio.enabled = true;
            }
        }
    }

    public override void DisableItem( ) {
        currentItemState = ItemState.ITEM_INACTIVE;
        ResetItem( );
    }

    protected virtual IEnumerator HideItemAfterUsePeriod( ) { return null; }

    protected override void ItemHasPerished( ) {
        if (defenseItemData.numberOfUses < 0) {
            DisableItem( );
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