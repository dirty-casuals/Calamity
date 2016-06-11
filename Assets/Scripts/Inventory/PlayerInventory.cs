using UnityEngine;
using UnityEngine.Networking;
using GameDataEditor;

public class PlayerInventory : Subject {

    [HideInInspector]
    [SyncVar]
    public string firstItem;

    private void Awake( ) {
        GDEDataManager.Init( "gde_data" );
    }

    private void OnTriggerEnter( Collider col ) {
        if (col.gameObject.tag != "Respawn" || !isLocalPlayer) {
            return;
        }
        CmdCanPlayerPickupItem( col.gameObject );
    }

    [Command]
    public void CmdUseItemInInventory( ) {
        if (firstItem.Length <= 0) {
            return;
        }
        CreateItemFromGameData( );
        firstItem = "";
    }

    [Command]
    private void CmdCanPlayerPickupItem( GameObject spawner ) {
        bool itemInSpawner = spawner.GetComponent<ItemSpawner>( ).currentlySpawnedItem;
        if (!itemInSpawner || firstItem.Length > 0) {
            return;
        }
        firstItem = spawner.GetComponent<ItemSpawner>( ).spawnType.ToString( );
        RemoveItemFromSpawner( spawner );
    }

    private void CreateItemFromGameData( ) {
        ItemSpawnType spawnType = (ItemSpawnType)System.Enum.Parse( typeof( ItemSpawnType), firstItem );

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
                CmdInitializeNewWeaponItem( knifeItem );
                break;
        }
    }

    private void InitializeNewDefenseItem( GDEDefenseItemData item ) {
        GameObject newItem = Instantiate( item.ItemModel );
        DefenseItem itemType = newItem.GetComponent<DefenseItem>( );
        itemType.defenseItemData = item;
        itemType.AddItemToPlayer( gameObject );
        itemType.CmdUseItem( gameObject );
        NetworkServer.Spawn( newItem );
    }

    private void CmdInitializeNewWeaponItem( GDEWeaponItemData item ) {
        GameObject newItem = Instantiate( item.ItemModel );
        WeaponItem itemType = newItem.GetComponent<WeaponItem>( );
        itemType.weaponItemData = item;
        itemType.AddItemToPlayer( gameObject );
        itemType.CmdUseItem( gameObject );
        NetworkServer.Spawn( item.ItemModel );
    }

    private void RemoveItemFromSpawner( GameObject spawner ) {
        spawner.GetComponent<ItemSpawner>( ).HideItemInSpawnPoint( );
    }
}