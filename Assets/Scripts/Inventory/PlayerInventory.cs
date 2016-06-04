using UnityEngine;
using UnityEngine.Networking;

public class PlayerInventory : Subject {

    public const string ADDED_ITEM_TO_INVENTORY = "ADDED_ITEM_TO_INVENTORY";
    public const string REMOVED_ITEM_FROM_INVENTORY = "REMOVED_ITEM_FROM_INVENTORY";
    public const string ITEM_THROWN_BY_PLAYER = "ITEM_THROWN_BY_PLAYER";
    [HideInInspector]
    [SyncVar] public GameObject itemForFirstSlot;

    private void Update( ) {
        if ( !isLocalPlayer || !itemForFirstSlot) {
            return;
        }
        CmdRemoveUnusableItems( );
    }

    private void OnTriggerEnter( Collider col ) {
        if (col.gameObject.tag != "Respawn") {
            return;
        }
        CmdCheckPlayerCanHaveItem( col.gameObject );
    }

    [Command]
    private void CmdCheckPlayerCanHaveItem( GameObject spawner ) {
        GameObject itemInSpawner = spawner.GetComponent<ItemSpawner>( ).currentlySpawnedItem;
        if ( !itemInSpawner || itemForFirstSlot != null )
        {
            return;
        }
        CmdAddItemToInventory( itemInSpawner );
        spawner.GetComponent<ItemSpawner>( ).currentlySpawnedItem = null;
    }

    [Command]
    private void CmdAddItemToInventory( GameObject item ) {
        Item newItem = item.GetComponent<Item>( );
        AddUnityObservers( item );
        NotifySendObject( newItem, ADDED_ITEM_TO_INVENTORY );
        itemForFirstSlot = item;
    }

    [Command]
    private void CmdRemoveUnusableItems( ) {
        switch (itemForFirstSlot.GetComponent<Item>( ).currentItemState) {
            case ItemState.ITEM_IN_USE:
            case ItemState.ITEM_IN_PLAYER_INVENTORY:
                break;
            case ItemState.ITEM_THROWN:
                CmdRemoveThrownItem( );
                break;
            case ItemState.ITEM_INACTIVE:
                CmdRemoveUsedItem( );
                break;
        }
    }

    [Command]
    private void CmdRemoveThrownItem( ) {
        Notify( ITEM_THROWN_BY_PLAYER );
        CmdRemoveItemFromInventory( );
    }
    [Command]
    private void CmdRemoveUsedItem( ) {
        Notify( REMOVED_ITEM_FROM_INVENTORY );
        CmdRemoveItemFromInventory( );
    }

    [Command]
    private void CmdRemoveItemFromInventory( ) {
        RemoveUnityObserver( itemForFirstSlot.gameObject );
        itemForFirstSlot.GetComponent<Item>( ).itemInPlayerHands = false;
        itemForFirstSlot = null;
    }
}