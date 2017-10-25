using UnityEngine;

public class UIInventoryItem : UnityObserver {
    public GameObject paperBall;
    public GameObject mandrake;
    public GameObject knife;
    private GameObject activeItemUIElement;

    public void Awake( ) {
        SetupObserver( );
    }

    public override void OnNotify( Object sender, EventArguments e ) {
        switch (e.eventMessage) {
            case PlayerInventory.ADDED_ITEM_TO_INVENTORY:
                ActivateGUIItem( e.extendedMessage[0] );
                break;
            case PlayerInventory.ITEM_USED_BY_PLAYER:
                DisableAllGUIItems( );
                break;
        }
    }

    private void ActivateGUIItem( string itemType ) {
        ItemSpawnType spawnType = (ItemSpawnType)System.Enum.Parse( typeof( ItemSpawnType ), itemType );

        DisableAllGUIItems( );
        switch (spawnType) {
            case ItemSpawnType.PAPER_BALL:
                paperBall.SetActive( true );
                break;
            case ItemSpawnType.MANDRAKE:
                mandrake.SetActive( true );
                break;
            case ItemSpawnType.KNIFE:
                knife.SetActive( true );
                break;
        }
    }

    private void DisableAllGUIItems( ) {
        paperBall.SetActive( false );
        knife.SetActive( false );
        mandrake.SetActive( false );
    }
}