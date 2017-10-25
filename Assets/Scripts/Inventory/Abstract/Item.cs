using UnityEngine;

public enum ItemState {
    ITEM_AT_SPAWN_POINT,
    ITEM_IN_PLAYER_INVENTORY,
    ITEM_THROWN,
    ITEM_IN_USE,
    ITEM_INACTIVE
}

public class Item : Subject {

    public bool itemInPlayerHands = false;
    [HideInInspector]
    public ItemState currentItemState;
    public Vector3 spawnPosition;
    public Transform spawnParent;
    public float itemXSpawnPosition;

    public virtual void UseItem( GameObject player ) { }

    public virtual void AddItemToPlayer( GameObject player ) { }

    public virtual void DisableItem( ) { }

    protected virtual void ItemHasPerished( ) { }

}