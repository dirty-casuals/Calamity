using UnityEngine;
using GameDataEditor;

public class WeaponItem : Item {

    [HideInInspector]
    public GDEWeaponItemData weaponItemData;
    private int cachedCostOfUse;

    private void Start( ) {
        GDEDataManager.Init( "gde_data" );
        gameObject.tag = "Item";
        currentItemState = ItemState.ITEM_AT_SPAWN_POINT;
        cachedCostOfUse = weaponItemData.CostOfUse;
        MoveItemToSpawnLocation( );
    }

    //public override void OnNotify( Object sender, EventArguments e ) {
    //    switch (e.eventMessage) {
    //        case PlayerInventory.ADDED_ITEM_TO_INVENTORY:
    //            if (sender != this) {
    //                break;
    //            }
    //            currentItemState = ItemState.ITEM_IN_PLAYER_INVENTORY;
    //            //spawnVisual.SetActive( false );
    //            //activeVisual.SetActive( false );
    //            break;
    //        case PlayerInventory.REMOVED_ITEM_FROM_INVENTORY:
    //            currentItemState = ItemState.ITEM_AT_SPAWN_POINT;
    //            weaponItemData.CostOfUse = cachedCostOfUse;
    //            break;
    //    }
    //}

    public override void CmdDisableItem( ) {
        currentItemState = ItemState.ITEM_INACTIVE;
        ResetItem( );
    }

    protected override void CmdItemHasPerished( ) {
        if (weaponItemData.numberOfUses < 0) {
            CmdDisableItem( );
        }
    }

    protected void ResetItem( ) {
        MoveItemToSpawnLocation( );
    }

    private void MoveItemToSpawnLocation( ) {
        //transform.position = itemSpawnPoint.transform.position;
        //transform.parent = itemSpawnPoint.transform;
    }
}
