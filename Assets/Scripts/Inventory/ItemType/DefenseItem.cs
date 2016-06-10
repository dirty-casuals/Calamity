using UnityEngine;
using GameDataEditor;
using System.Collections;
using UnityEngine.Networking;

public class DefenseItem : Item {

    [HideInInspector]
    [SyncVar]
    public GDEDefenseItemData defenseItemData;
    [SyncVar]
    private int cachedCostOfUse;
    AudioSource usedAudio;

    private void Start( ) {
        GDEDataManager.Init( "gde_data" );
        gameObject.tag = "Item";
        currentItemState = ItemState.ITEM_AT_SPAWN_POINT;
        cachedCostOfUse = defenseItemData.CostOfUse;
        usedAudio = GetComponent<AudioSource>( );
    }

    public override void OnNotify( Object sender, EventArguments e ) {
        switch (e.eventMessage) {
            case PlayerInventory.ADDED_ITEM_TO_INVENTORY:
                if (sender != this) {
                    break;
                }
                currentItemState = ItemState.ITEM_IN_PLAYER_INVENTORY;
                GetComponent<MeshRenderer>( ).enabled = false;
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

    [Command]
    public override void CmdDisableItem( ) {
        currentItemState = ItemState.ITEM_INACTIVE;
        CmdResetItem( );
    }

    protected virtual IEnumerator HideItemAfterUsePeriod( ) { return null; }

    [Command]
    protected override void CmdItemHasPerished( ) {
        if (defenseItemData.numberOfUses < 0) {
            CmdDisableItem( );
        }
    }

    [Command]
    protected void CmdResetItem( ) {
        GetComponent<MeshRenderer>( ).enabled = false;
    }
}