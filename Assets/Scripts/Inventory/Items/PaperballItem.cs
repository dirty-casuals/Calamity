using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PaperballItem : DefenseItem {

    /* Reset projectile to original after being thrown
    ** So it doesn't move with the player
    */
    private Transform cachedTransform;
    private GameObject currentPlayer;
    private Rigidbody itemRigidbody;

    [Command]
    public override void CmdPlaceItemInHand( GameObject player ) {
        currentPlayer = player;
        itemRigidbody = GetComponent<Rigidbody>( );
        cachedTransform = gameObject.transform.parent;

        PlacePaperballInPlayerHands( );
        SetPaperballVisualAspect( );
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
        currentItemState = ItemState.ITEM_THROWN;
        yield return new WaitForSeconds( defenseItemData.itemDuration );
        ResetRigidbody( );
        CmdResetItem( );
        currentItemState = ItemState.ITEM_AT_SPAWN_POINT;
    }

    private void PlacePaperballInPlayerHands( ) {
        GameObject playerHands = currentPlayer.GetComponentInChildren<PlayerHands>( ).gameObject;
        gameObject.transform.position = playerHands.transform.position;
        gameObject.transform.parent = playerHands.transform;
    }

    private void SetPaperballVisualAspect( ) {
        spawnVisual.SetActive( false );
        activeVisual.SetActive( true );
    }

    private void LaunchPaperBall( ) {
        itemRigidbody.isKinematic = false;
        itemRigidbody.useGravity = true;
        gameObject.transform.parent = cachedTransform;
        itemRigidbody.velocity = (currentPlayer.transform.forward * defenseItemData.projectileRange);
    }

    private void ResetRigidbody( ) {
        itemRigidbody.isKinematic = true;
        itemRigidbody.useGravity = false;
    }

}
