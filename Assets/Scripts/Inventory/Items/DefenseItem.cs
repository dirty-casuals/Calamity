using UnityEngine;
using System.Collections;
using GameDataEditor;

public class DefenseItem : Item {

    [HideInInspector]
    public GDEDefenseItemData defenseItemData;

    private void Start( ) {
        gameObject.tag = "Item";
    }

    private void Update( ) { }

    public override void AddItemToPlayerInventory( GameObject player ) {
        player.GetComponent<PlayerInventory>( ).itemForFirstSlot = this;
        gameObject.SetActive( false );
    }

    public override void UseItem( GameObject player ) {
        Vector3 playerPosition = player.transform.position;
        Vector3 fireFromPosition = new Vector3( playerPosition.x, 1.0f, playerPosition.z );
        gameObject.transform.position = fireFromPosition;
        gameObject.SetActive( true );

        GetComponent<Rigidbody>( ).velocity = (player.transform.forward * defenseItemData.ProjectileRange);
        defenseItemData.NumberOfUses -= defenseItemData.CostOfUse;
        CheckIfItemHasPerished( );
    }

    public override void CheckIfItemHasPerished( ) {
        if ( defenseItemData.NumberOfUses <= 0 ) {
            ranOutOfUses = true;
        }
    }
}