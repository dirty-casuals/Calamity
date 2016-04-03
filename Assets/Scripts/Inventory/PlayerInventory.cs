using UnityEngine;

public class PlayerInventory : Subject {

    public GameObject inventoryUI;
    public const string ADDED_ITEM_TO_INVENTORY = "ADDED_ITEM_TO_INVENTORY";
    public const string REMOVED_ITEM_FROM_INVENTORY = "REMOVED_ITEM_FROM_INVENTORY";
    public const string ITEM_USED_BY_PLAYER = "ITEM_USED_BY_PLAYER";

    [HideInInspector]
    public Item itemForFirstSlot;

    private void Awake( ) {
        if (inventoryUI == null) {
            Debug.LogError( "Inventory UI Not Found!" );
            return;
        }
        AddUnityObservers( inventoryUI );
    }

    private void Update( ) {
        if (!itemForFirstSlot) {
            return;
        }
        RemoveUnusableItems( );
    }

    private void OnTriggerEnter( Collider col ) {
        if (col.gameObject.tag != "Respawn") {
            return;
        }
        Item itemInSpawner = col.GetComponent<ItemSpawner>( ).currentlySpawnedItem;
        if (!itemInSpawner || itemForFirstSlot != null) {
            return;
        }
        AddItemToInventory( itemInSpawner );

        // Remove item from spawner
        col.GetComponent<ItemSpawner>( ).currentlySpawnedItem = null;
    }

    private void AddItemToInventory( Item newItem ) {
        AddUnityObservers( newItem.gameObject );
        NotifySendObject( newItem, ADDED_ITEM_TO_INVENTORY );
        itemForFirstSlot = newItem;
    }

    private void RemoveCurrentItemFromInventory( ) {
        Notify( REMOVED_ITEM_FROM_INVENTORY );
        RemoveUnityObserver( itemForFirstSlot.gameObject );
        itemForFirstSlot.itemInPlayerHands = false;
        itemForFirstSlot = null;
    }

    private void RemoveUnusableItems( ) {
        switch (itemForFirstSlot.currentItemState) {
            case ItemState.ITEM_IN_USE:
                Notify( ITEM_USED_BY_PLAYER );
                break;
            case ItemState.ITEM_IN_PLAYER_INVENTORY:
                break;
            case ItemState.ITEM_INACTIVE:
                RemoveCurrentItemFromInventory( );
                break;
        }
    }
}