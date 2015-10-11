using UnityEngine;
using System.Collections;
using GameDataEditor;

public class DefenseItem : Item {

    [HideInInspector]
    public GDEDefenseItemData defenseItemData;
    private int cachedCostOfUse;
    private bool useTimerRunning = false;

    private void Start( ) {
        GDEDataManager.Init( "gde_data" );
        gameObject.tag = "Item";
        currentItemState = ItemState.ITEM_AT_SPAWN_POINT;
        cachedCostOfUse = defenseItemData.CostOfUse;
        MoveItemToSpawnLocation( );
    }

    public override void OnNotify( Object sender, EventArguments e ) {
        switch ( e.eventMessage ) {
            case PlayerInventory.ADDED_ITEM_TO_INVENTORY:
                currentItemState = ItemState.ITEM_IN_PLAYER_INVENTORY;
                gameObject.SetActive( false );
                break;
            case PlayerInventory.REMOVED_ITEM_FROM_INVENTORY:
                currentItemState = ItemState.ITEM_AT_SPAWN_POINT;
                defenseItemData.CostOfUse = cachedCostOfUse;
                break;
        }
    }

    public override void RespawnItem( ) {
        gameObject.SetActive( true );
    }

    private void MoveItemToSpawnLocation( ) {
        transform.position = itemSpawnPoint.transform.position;
        transform.parent = itemSpawnPoint.transform;
    }

    public override void UseItem( GameObject player ) {
        if ( currentItemState == ItemState.ITEM_IN_USE ) {
            return;
        }

        Vector3 playerPosition = player.transform.position;
        Vector3 fireFromPosition = new Vector3( playerPosition.x, 1.0f, playerPosition.z );
        gameObject.transform.position = fireFromPosition;
        gameObject.SetActive( true );

        GetComponent<Rigidbody>( ).velocity = ( player.transform.forward * defenseItemData.ProjectileRange );
        defenseItemData.NumberOfUses -= defenseItemData.CostOfUse;
        currentItemState = ItemState.ITEM_IN_USE;
        StartCoroutine( HideItemAfterUsePeriod( ) );
    }

    protected IEnumerator HideItemAfterUsePeriod( ) {
        ItemHasPerished( );
        yield return new WaitForSeconds( defenseItemData.itemDuration );
        if ( currentItemState == ItemState.ITEM_AT_SPAWN_POINT ) {
            MoveItemToSpawnLocation( );
        }
        gameObject.SetActive( false );
    }

    protected override void ItemHasPerished( ) {
        if ( defenseItemData.NumberOfUses > 0 ) {
            return;
        }
        currentItemState = ItemState.ITEM_INACTIVE;
    }
}