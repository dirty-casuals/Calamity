using UnityEngine;
using System.Collections;
using GameDataEditor;

public class DefenseItem : Item {

    [HideInInspector]
    public GDEDefenseItemData defenseItemData;
    private GameObject playerHoldingItem;

    private void Start( ) {
        gameObject.tag = "Item";
    }

    private void Update( ) { }

    private void OnCollisionEnter( Collision col ) {
        if ( col.gameObject.tag != "Player" ) {
            return;
        }
        playerHoldingItem = col.gameObject;
        PlayerInventory inventory = playerHoldingItem.GetComponent<PlayerInventory>( );
        AddItemToCharacterInventory( inventory );
    }

    public override void UseItem( ) {
        Debug.Log( "Use Defense Item" );

        gameObject.SetActive( true );
        gameObject.transform.position = playerHoldingItem.transform.position;


        //Vector3 direction = playerHoldingItem.transform.position - Vector3.forward;

        GetComponent<Rigidbody>( ).AddForce( playerHoldingItem.transform.forward * 100 );

        defenseItemData.NumberOfUses -= defenseItemData.CostOfUse;
        ItemHasPerished( );
    }

    public override void ItemHasPerished( ) {
        if ( defenseItemData.NumberOfUses <= 0 ) {
            ranOutOfUses = true;
        }
    }

    private void AddItemToCharacterInventory( PlayerInventory inventory ) {
        inventory.itemForFirstSlot = this;
        //gameObject.SetActive( false );
    }
}