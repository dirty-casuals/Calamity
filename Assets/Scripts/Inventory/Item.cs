using UnityEngine;

public enum ItemState {
    ITEM_AT_SPAWN_POINT,
    ITEM_IN_PLAYER_INVENTORY,
    ITEM_IN_USE,
    ITEM_INACTIVE
}

public class Item : UnityObserver {

    [HideInInspector]
    public ItemState currentItemState;
    [HideInInspector]
    public GameObject itemSpawnPoint;

    public virtual void SpawnItem( ) { }

    public virtual void RespawnItem( ) { }

    public virtual void PickupItem( ) { }

    public virtual void AddItemToPlayerInventory( GameObject player ) { }

    public virtual void UseItem( GameObject player ) { }

    protected virtual void ItemHasPerished( ) { }

}