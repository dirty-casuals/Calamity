using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PaperballItem : DefenseItem {

    /* Reset projectile to original after being thrown
    ** So it doesn't move with the player
    */
    private GameObject currentPlayer;
    [SyncVar]
    private Rigidbody itemRigidbody;
    private MeshRenderer itemMesh;

    [Command]
    public override void CmdAddItemToPlayer( GameObject player ) {
        currentPlayer = player;
        itemRigidbody = GetComponent<Rigidbody>( );
        itemMesh = GetComponent<MeshRenderer>( );
        itemMesh.enabled = false;
        itemInPlayerHands = true;
    }

    [Command]
    public override void CmdUseItem( GameObject player ) {
        if (currentItemState == ItemState.ITEM_IN_USE) {
            return;
        }
        LaunchPaperBall( );
        defenseItemData.numberOfUses -= defenseItemData.CostOfUse;
        currentItemState = ItemState.ITEM_IN_USE;
        StartCoroutine( HideItemAfterUsePeriod( ) );
    }
     
    protected override IEnumerator HideItemAfterUsePeriod( ) {
        itemMesh.enabled = true;
        currentItemState = ItemState.ITEM_THROWN;
        yield return new WaitForSeconds( defenseItemData.itemDuration );
        ResetRigidbody( );
        //CmdResetItem( );
        currentItemState = ItemState.ITEM_AT_SPAWN_POINT;
    }

    private void LaunchPaperBall( ) {
        PlacePaperballInPlayerHands( );
        itemRigidbody.isKinematic = false;
        itemRigidbody.useGravity = true;
        itemRigidbody.velocity = (currentPlayer.transform.forward * defenseItemData.projectileRange);
    }

    private void PlacePaperballInPlayerHands( ) {
        GameObject playerHands = currentPlayer.GetComponentInChildren<PlayerHands>( ).gameObject;
        gameObject.transform.position = playerHands.transform.position;
    }

    private void ResetRigidbody( ) {
        itemRigidbody.isKinematic = true;
        itemRigidbody.useGravity = false;
    }

}
