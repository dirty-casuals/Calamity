using UnityEngine;
using System.Collections;

public enum ItemState {
    ITEM_AT_SPAWN_POINT,
    ITEM_IN_PLAYER_INVENTORY,
    ITEM_THROWN,
    ITEM_IN_USE,
    ITEM_INACTIVE
}

public class Item : UnityObserver {

    public GameObject activeVisual;
    public GameObject spawnVisual;
    public bool itemInPlayerHands = false;
    [HideInInspector]
    public ItemState currentItemState;
    [HideInInspector]
    public GameObject itemSpawnPoint;

    public virtual void SpawnItem( ) { }

    public virtual void RespawnItem( ) {
        spawnVisual.SetActive( true );
        activeVisual.SetActive( false );
    }

    public virtual void PickupItem( ) { }

    public virtual void AddItemToPlayerInventory( GameObject player ) { }

    public virtual void UseItem( GameObject player ) { }

    public virtual void PlaceItemInHand( GameObject player ) { }

    public virtual void DisableItem( ) { }

    protected virtual void ItemHasPerished( ) { }

}