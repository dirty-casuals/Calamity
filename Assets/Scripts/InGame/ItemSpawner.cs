﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using GameDataEditor;

public enum ItemSpawnType {
    PAPER_BALL,
    KNIFE,
    MANDRAKE
}

public class ItemSpawner : NetworkBehaviour {
    public ItemSpawnType spawnType;
    public float spawnTimer = 15.0f;
    [HideInInspector]
    [SyncVar] public GameObject currentlySpawnedItem;
    private float timeToSpawnItem = 0.0f;
    private List<Item> spawnedItems = new List<Item>( );

    private void Awake( ) {
        GDEDataManager.Init( "gde_data" );
    }

    private void Start( ) {
        ServerSpawnItem( );
    }

    private void Update( ) {
        if (currentlySpawnedItem) {
            timeToSpawnItem = 0.0f;
            return;
        }
        if (timeToSpawnItem >= spawnTimer) {
            ServerSpawnItem( );
            timeToSpawnItem = 0.0f;
            return;
        }
        timeToSpawnItem += Time.deltaTime;
    }

    [Server]
    private void ServerSpawnItem( ) {
        if (currentlySpawnedItem) {
            return;
        }
        foreach (Item item in spawnedItems) {
            if (item.currentItemState == ItemState.ITEM_AT_SPAWN_POINT) {
                currentlySpawnedItem = item.gameObject;
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
                GDEWeaponItemData knifeItem = new GDEWeaponItemData( );
                GDEDataManager.DataDictionary.TryGetCustom( GDEItemKeys.WeaponItem_Knife, out knifeItem );
                CmdInitializeNewWeaponItem( knifeItem );
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
        NetworkServer.Spawn( newItem );
        itemType.defenseItemData = item;
        itemType.itemSpawnPoint = gameObject;
        spawnedItems.Add( itemType );
        currentlySpawnedItem = newItem;
    }

    private void CmdInitializeNewWeaponItem( GDEWeaponItemData item ) {
        GameObject newItem = Instantiate( item.ItemModel );
        WeaponItem itemType = newItem.GetComponent<WeaponItem>( );
        currentlySpawnedItem = newItem;
        itemType.weaponItemData = item;
        itemType.itemSpawnPoint = gameObject;
        spawnedItems.Add( itemType );
        NetworkServer.Spawn( item.ItemModel );
    }
}