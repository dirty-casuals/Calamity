using UnityEngine;
using UnityEngine.Networking;

public enum ItemState {
    ITEM_AT_SPAWN_POINT,
    ITEM_IN_PLAYER_INVENTORY,
    ITEM_THROWN,
    ITEM_IN_USE,
    ITEM_INACTIVE
}

public class Item : UnityObserver {

    [SyncVar] public GameObject activeVisual;
    [SyncVar] public GameObject spawnVisual;
    [SyncVar] public bool itemInPlayerHands = false;
    [HideInInspector]
    [SyncVar] public ItemState currentItemState;
    [HideInInspector]
    [SyncVar] public GameObject itemSpawnPoint;

    public virtual void SpawnItem( ) { }

    public virtual void PickupItem( ) { }

    public virtual void AddItemToPlayerInventory( GameObject player ) { }

    public virtual void CmdUseItem( GameObject player ) { }

    public virtual void CmdPlaceItemInHand( GameObject player ) { }

    public virtual void CmdDisableItem( ) { }

    [Command]
    public void CmdRespawnItem( ) {
        GetComponent<MeshRenderer>( ).enabled = true;
    }

    protected virtual void CmdItemHasPerished( ) { }

}