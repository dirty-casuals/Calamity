using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using GameDataEditor;

public class PlayerInventory : MonoBehaviour {

    public Item itemForFirstSlot;

    private void Update( ) {
        if ( !itemForFirstSlot ) {
            return;
        }
        RemoveUnusableItems( );
    }

    private void RemoveUnusableItems( ) {

        if ( itemForFirstSlot.ranOutOfUses ) {
            itemForFirstSlot = null;
        }
    }
}