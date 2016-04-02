using UnityEngine;

public class UIInventoryItem : UnityObserver {
    public GameObject paperBall;
    public GameObject knife;
    public GameObject mandrake;
    private GameObject activeItemUIElement;

    public override void OnNotify( Object sender, EventArguments e ) {
        switch (e.eventMessage) {
            case PlayerInventory.ADDED_ITEM_TO_INVENTORY:
                ActivateGUIItem( sender );
                break;
            case PlayerInventory.ITEM_USED_BY_PLAYER:
                DisableAllGUIItems( );
                break;
        }
    }

    private void ActivateGUIItem( Object sender ) {
        DefenseItem defenseItem = sender as DefenseItem;
        WeaponItem weaponItem = sender as WeaponItem;

        DisableAllGUIItems( );
        if (weaponItem != null) {
        }
        if (defenseItem != null) {
            EnableDefenseItem( defenseItem.defenseItemData.itemType );
        }
    }

    private void EnableDefenseItem( string type ) {
        if (type == "paperball") {
            paperBall.SetActive( true );
        }
    }

    private void EnableWeaponItem( ) { }

    private void DisableAllGUIItems( ) {
        paperBall.SetActive( false );
        knife.SetActive( false );
        mandrake.SetActive( false );
    }
}
