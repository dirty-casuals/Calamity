using UnityEngine;
using System.Collections;
using RAIN.Entities;

public class MandrakeItem : DefenseItem {

    private GameObject currentPlayer;
    private bool mandrakeActive;

    private void OnTriggerEnter( Collider col ) {
        if (col.gameObject.tag != "Player") {
            return;
        }
        SetPlayerVisibility( col.gameObject, false );
    }

    private void OnTriggerExit( Collider col ) {
        if (col.gameObject.tag != "Player") {
            return;
        }
        SetPlayerVisibility( col.gameObject, true );
    }

    private void OnTriggerStay( Collider col ) {
        if (col.gameObject.tag != "Player" || mandrakeActive) {
            return;
        }
        SetPlayerVisibility( col.gameObject, true );
    }

    public override void AddItemToPlayer( GameObject player ) {
        mandrakeActive = true;
        currentPlayer = player;
    }

    public override void UseItem( GameObject player ) {
        PlaceMandrakeInPlayerHands( );
        defenseItemData.numberOfUses -= defenseItemData.CostOfUse;
        StartCoroutine( HideItemAfterUsePeriod( ) );
    }

    protected override IEnumerator HideItemAfterUsePeriod( ) {
        yield return new WaitForSeconds( defenseItemData.itemDuration );
        /* Force OnTriggerStay to update players in trigger zone
        ** and ensure all network messages sync before destroy
        */
        mandrakeActive = false;
        yield return new WaitForSeconds( 0.2f );
        mandrakeActive = true;
        yield return new WaitForSeconds( 0.2f );
        Destroy( gameObject );
    }

    private void SetPlayerVisibility( GameObject player, bool isVisible ) {
        EntityRig playerRig = player.GetComponentInChildren<EntityRig>( );
        playerRig.Entity.IsActive = isVisible;
    }

    private void PlaceMandrakeInPlayerHands( ) {
        GameObject playerHands = currentPlayer.GetComponentInChildren<PlayerHands>( ).gameObject;
        gameObject.transform.position = playerHands.transform.position;
    }
}
