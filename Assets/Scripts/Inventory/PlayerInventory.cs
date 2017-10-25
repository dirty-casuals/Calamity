using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor;

public class PlayerInventory : Subject {

    public const string ADDED_ITEM_TO_INVENTORY = "ADDED_ITEM_TO_INVENTORY";
    public const string ITEM_USED_BY_PLAYER = "ITEM_USED_BY_PLAYER";
    [HideInInspector]
    public string firstItem;

    public void Start( ) {
        StartCoroutine( InitializeInventoryUI() );
        GDEDataManager.Init( "gde_data" );
    }

    private IEnumerator InitializeInventoryUI( ) {
        UIInventoryItem playerInventoryUI = null;
        while (playerInventoryUI == null) {
            playerInventoryUI = FindObjectOfType<UIInventoryItem>( );
            yield return new WaitForFixedUpdate( );
        }
        AddUnityObservers( playerInventoryUI.gameObject );
    }

    private void OnTriggerEnter( Collider col ) {
        if (col.gameObject.tag != "Respawn") {
            return;
        }
        if (CanPlayerPickupItem( col.gameObject )) {
            string itemType = col.gameObject.GetComponent<ItemSpawner>( ).spawnType.ToString( );
            AddItemToInventoryUI( itemType );
            PickupItem( col.gameObject, itemType );
        }
    }

    public void RemoveItemFromInventoryUI( ) {
        List<string> item = new List<string>( ) { firstItem };
        NotifyExtendedMessage( ITEM_USED_BY_PLAYER, item );
    }

    public void UseItemInInventory( ) {
        if (firstItem.Length <= 0) {
            return;
        }
        CreateItemFromGameData( );
        firstItem = "";
    }

    public void RemoveItemFromInventory( ) {
        firstItem = "";
    }

    private bool CanPlayerPickupItem( GameObject spawner ) {
        bool itemInSpawner = spawner.GetComponent<ItemSpawner>( ).currentlySpawnedItem;
        return (itemInSpawner || firstItem.Length < 0);
    }

    private void AddItemToInventoryUI( string itemType ) {
        List<string> item = new List<string>( ) { itemType };
        NotifyExtendedMessage( ADDED_ITEM_TO_INVENTORY, item );
    }

    private void PickupItem( GameObject spawner, string itemType ) {
        firstItem = itemType;
        RemoveItemFromSpawner( spawner );
    }

    private void CreateItemFromGameData( ) {
        ItemSpawnType spawnType = (ItemSpawnType)System.Enum.Parse( typeof( ItemSpawnType ), firstItem );

        switch (spawnType) {
            case ItemSpawnType.PAPER_BALL:
                GDEDefenseItemData paperItem = new GDEDefenseItemData( );
                GDEDataManager.DataDictionary.TryGetCustom( GDEItemKeys.DefenseItem_Paperball, out paperItem );
                InitializeNewDefenseItem( paperItem );
                break;
            case ItemSpawnType.MANDRAKE:
                GDEDefenseItemData mandrakeItem = new GDEDefenseItemData( );
                GDEDataManager.DataDictionary.TryGetCustom( GDEItemKeys.DefenseItem_ManDrake, out mandrakeItem );
                InitializeNewDefenseItem( mandrakeItem );
                break;
            case ItemSpawnType.KNIFE:
                GDEWeaponItemData knifeItem = new GDEWeaponItemData( );
                GDEDataManager.DataDictionary.TryGetCustom( GDEItemKeys.WeaponItem_Knife, out knifeItem );
                InitializeNewWeaponItem( knifeItem );
                break;
        }
    }

    private void InitializeNewDefenseItem( GDEDefenseItemData item ) {
        GameObject newItem = Instantiate( item.ItemModel );
        DefenseItem itemType = newItem.GetComponent<DefenseItem>( );
        itemType.defenseItemData = item;
        itemType.AddItemToPlayer( gameObject );
        itemType.UseItem( gameObject );
        Instantiate( newItem );
    }

    private void InitializeNewWeaponItem( GDEWeaponItemData item ) {
        GameObject newItem = Instantiate( item.ItemModel );
        WeaponItem itemType = newItem.GetComponent<WeaponItem>( );
        itemType.weaponItemData = item;
        itemType.AddItemToPlayer( gameObject );
        itemType.UseItem( gameObject );
        Instantiate( item.ItemModel );
    }

    private void RemoveItemFromSpawner( GameObject spawner ) {
        spawner.GetComponent<ItemSpawner>( ).HideItemInSpawnPoint( );
    }
}