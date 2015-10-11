using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInventory : Subject {

    public const string ADDED_ITEM_TO_INVENTORY = "ADDED_ITEM_TO_INVENTORY";
    public const string REMOVED_ITEM_FROM_INVENTORY = "REMOVED_ITEM_FROM_INVENTORY";

    [HideInInspector]
    public Item itemForFirstSlot;

    private void Update( ) {
        if ( !itemForFirstSlot ) {
            return;
        }
        RemoveUnusableItems( );
    }

    private void OnTriggerEnter( Collider col ) {
        if ( col.gameObject.tag != "Respawn" ) {
            return;
        }
        Item itemInSpawner = col.GetComponent<ItemSpawner>( ).currentlySpawnedItem;
        PlayerHasPickedUpDifferentItem( itemInSpawner );
        AddItemToInventory( itemInSpawner );
        // Remove item from inventory
        col.GetComponent<ItemSpawner>( ).currentlySpawnedItem = null;
    }

    private void PlayerHasPickedUpDifferentItem( Item newItem ) {
        if ( itemForFirstSlot && newItem != itemForFirstSlot ) {
            RemoveCurrentItemFromInventory( );
        }
    }

    private void AddItemToInventory( Item newItem ) {
        AddUnityObservers( newItem.gameObject );
        Notify( ADDED_ITEM_TO_INVENTORY );
        itemForFirstSlot = newItem;
    }

    private void RemoveCurrentItemFromInventory( ) {
        Notify( REMOVED_ITEM_FROM_INVENTORY );
        RemoveUnityObserver( itemForFirstSlot.gameObject );
        itemForFirstSlot = null;
    }

    private void RemoveUnusableItems( ) {
        switch ( itemForFirstSlot.currentItemState ) { 
            case ItemState.ITEM_IN_USE:
            case ItemState.ITEM_IN_PLAYER_INVENTORY:
                return;
            case ItemState.ITEM_INACTIVE:
                RemoveCurrentItemFromInventory( );
                break;
        }
    }
}