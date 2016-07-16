using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor;

public class PlayerInventory : NetworkSubject {

    public const string ADDED_ITEM_TO_INVENTORY = "ADDED_ITEM_TO_INVENTORY";
    public const string ITEM_USED_BY_PLAYER = "ITEM_USED_BY_PLAYER";
    [HideInInspector]
    [SyncVar]
    public string firstItem;

    public override void OnStartLocalPlayer( ) {
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
        if (col.gameObject.tag != "Respawn" || !isLocalPlayer) {
            return;
        }
        if (CanPlayerPickupItem( col.gameObject )) {
            string itemType = col.gameObject.GetComponent<ItemSpawner>( ).spawnType.ToString( );
            AddItemToInventoryUI( itemType );
            CmdPickupItem( col.gameObject, itemType );
        }
    }

    public void RemoveItemFromInventoryUI( ) {
        if (!isLocalPlayer) {
            return;
        }
        List<string> item = new List<string>( ) { firstItem };
        NotifyExtendedMessage( ITEM_USED_BY_PLAYER, item );
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
    public void CmdRemoveItemFromInventory( ) {
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

    [Command]
    private void CmdPickupItem( GameObject spawner, string itemType ) {
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