using UnityEngine;
using System.Collections;
using GameDataEditor;

public enum ItemSpawnType {
    PAPER_BALL,
    KNIFE,
    MANDRAKE
}

public class ItemSpawner : MonoBehaviour {

    public ItemSpawnType spawnType;
    private IGDEData item;
    private GDEDefenseItemData paperBall;

    private void Awake( ) {
        GDEDataManager.Init( "gde_data" );
    }

    private void Start( ) {
        CreateItemFromGameData( );
    }

    private void OnTriggerEnter( Collider col ) {
        if ( col.gameObject.tag != "Player" ) {
            return;
        }
        GameObject playerHoldingItem = col.gameObject;
        PlayerInventory inventory = playerHoldingItem.GetComponent<PlayerInventory>( );
        //AddItemToCharacterInventory( inventory );
    }

    //private void AddItemToCharacterInventory( PlayerInventory inventory ) {
    //    inventory.itemForFirstSlot = paperBall;
    //    //gameObject.SetActive( false );
    //}

    private void CreateItemFromGameData( ) {
        switch ( spawnType ) {
            case ItemSpawnType.PAPER_BALL:
                if ( paperBall != null ) {
                    MoveItemToSpawnLocation( paperBall.ItemModel );
                    return;
                }
                GDEDataManager.DataDictionary.TryGetCustom( GDEItemKeys.DefenseItem_Paperball, out paperBall );
                CreateNewDefenseItem( );
                break;
            case ItemSpawnType.KNIFE:
                break;
            case ItemSpawnType.MANDRAKE:
                break;
        }
    }

    private void CreateNewDefenseItem( ) {
        GameObject newDefenseItem = Instantiate( paperBall.ItemModel );
        newDefenseItem.GetComponent<DefenseItem>( ).defenseItemData = paperBall;
        MoveItemToSpawnLocation( newDefenseItem );
    }

    private void MoveItemToSpawnLocation( GameObject item ) {
        item.transform.position = transform.position;
        item.transform.parent = transform;
    }
}