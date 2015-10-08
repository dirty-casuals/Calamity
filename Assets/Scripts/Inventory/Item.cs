using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor;

public class Item : MonoBehaviour {

    public bool ranOutOfUses = false;

    private void Awake( ) {
        GDEDataManager.Init( "gde_data" );
    }

    public virtual void SpawnItem( ) { }

    public virtual void PickupItem( ) { }

    public virtual void CheckIfItemHasPerished( ) { }

    public virtual void AddItemToPlayerInventory( GameObject player ) { }

    public virtual void UseItem( GameObject player ) { }

}