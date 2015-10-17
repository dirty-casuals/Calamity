using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

public enum ItemSpawnType {
    PAPER_BALL,
    KNIFE,
    MANDRAKE
}

public class ItemSpawner : MonoBehaviour {

    public ItemSpawnType spawnType;
    public float spawnTimer = 15.0f;
    [HideInInspector]
    public Item currentlySpawnedItem;
    private float timeToSpawnItem = 0.0f;
    private List<Item> spawnedItems = new List<Item>( );
    private IGDEData item;

    private void Awake( ) {
        GDEDataManager.Init( "gde_data" );
    }

    private void Update( ) {
        if (PauseSpawnTimeWhenContainsItem( )) {
            return;
        }

        if (timeToSpawnItem >= spawnTimer) {
            SpawnItem( );
            timeToSpawnItem = 0.0f;
            return;
        }
        timeToSpawnItem += Time.deltaTime;
    }

    private bool PauseSpawnTimeWhenContainsItem( ) {
        if (!currentlySpawnedItem) {
            return false;
        }
        timeToSpawnItem = 0.0f;
        return true;
    }

    private void SpawnItem( ) {
        if (currentlySpawnedItem) {
            return;
        }
        foreach (Item item in spawnedItems) {
            if (item.currentItemState == ItemState.ITEM_AT_SPAWN_POINT) {
                currentlySpawnedItem = item;
                item.RespawnItem( );
                return;
            }
        }
        CreateItemFromGameData( );
    }

    private void CreateItemFromGameData( ) {
        switch (spawnType) {
            case ItemSpawnType.PAPER_BALL:
                GDEDefenseItemData paperItem = new GDEDefenseItemData( );
                GDEDataManager.DataDictionary.TryGetCustom( GDEItemKeys.DefenseItem_Paperball, out paperItem );
                InitializeNewDefenseItem( paperItem );
                break;
            case ItemSpawnType.KNIFE:
                break;
            case ItemSpawnType.MANDRAKE:
                GDEDefenseItemData mandrakeItem = new GDEDefenseItemData( );
                GDEDataManager.DataDictionary.TryGetCustom( GDEItemKeys.DefenseItem_ManDrake, out mandrakeItem );
                InitializeNewDefenseItem( mandrakeItem );
                break;
        }
    }

    private void InitializeNewDefenseItem( GDEDefenseItemData item ) {
        GameObject newItem = Instantiate( item.ItemModel );
        DefenseItem itemType = newItem.GetComponent<DefenseItem>( );
        currentlySpawnedItem = itemType;
        itemType.defenseItemData = item;
        itemType.itemSpawnPoint = gameObject;
        spawnedItems.Add( itemType );
    }

}