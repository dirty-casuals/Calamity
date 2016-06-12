using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using RAIN.Entities;

public class MandrakeItem : DefenseItem {

    [SyncVar]
    private GameObject currentPlayer;
    private bool mandrakeActive;

    private void OnTriggerEnter( Collider col ) {
        if (col.gameObject.tag != "Player" || !isServer) {
            return;
        }
        RpcSetPlayerVisibility( col.gameObject, false );
    }

    private void OnTriggerExit( Collider col ) {
        if (col.gameObject.tag != "Player" || !isServer) {
            return;
        }
        RpcSetPlayerVisibility( col.gameObject, true );
    }

    private void OnTriggerStay( Collider col ) {
        if (col.gameObject.tag != "Player" || !isServer || mandrakeActive) {
            return;
        }
        RpcSetPlayerVisibility( col.gameObject, true );
    }

    public override void AddItemToPlayer( GameObject player ) {
        mandrakeActive = true;
        currentPlayer = player;
    }

    [Command]
    public override void CmdUseItem( GameObject player ) {
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
        NetworkServer.Destroy( gameObject );
    }

    [ClientRpc]
    private void RpcSetPlayerVisibility( GameObject player, bool isVisible ) {
        EntityRig playerRig = player.GetComponentInChildren<EntityRig>( );
        playerRig.Entity.IsActive = isVisible;
    }

    private void PlaceMandrakeInPlayerHands( ) {
        GameObject playerHands = currentPlayer.GetComponentInChildren<PlayerHands>( ).gameObject;
        gameObject.transform.position = playerHands.transform.position;
    }
}
