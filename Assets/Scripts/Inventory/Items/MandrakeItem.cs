using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using RAIN.Entities;

public class MandrakeItem : DefenseItem {

    private EntityRig playerRig;

    [Command]
    public override void CmdPlaceItemInHand( GameObject player ) {
        playerRig = player.GetComponentInChildren<EntityRig>( );
        CmdPlaceMandrakeInPlayerHands( player );
        CmdSetMandrakeVisualAspect( );
        itemInPlayerHands = true;
    }

    public override void UseItem( GameObject player ) {
        if (currentItemState == ItemState.ITEM_IN_USE) {
            return;
        }
        MakePlayerInvisible( player );
        defenseItemData.numberOfUses -= defenseItemData.CostOfUse;
        currentItemState = ItemState.ITEM_IN_USE;
        StartCoroutine( HideItemAfterUsePeriod( ) );
    }

    protected override IEnumerator HideItemAfterUsePeriod( ) {
        yield return new WaitForSeconds( defenseItemData.itemDuration );
        playerRig.Entity.IsActive = true;
        ItemHasPerished( );
    }

    [Command]
    private void CmdPlaceMandrakeInPlayerHands( GameObject player ) {
        GameObject playerHands = player.GetComponentInChildren<PlayerHands>( ).gameObject;
        gameObject.transform.position = playerHands.transform.position;
        gameObject.transform.parent = playerHands.transform;
    }

    [Command]
    private void CmdSetMandrakeVisualAspect( ) {
        spawnVisual.SetActive( false );
        activeVisual.SetActive( true );
    }

    private void MakePlayerInvisible( GameObject player ) {
        playerRig.Entity.IsActive = false;
    }

}
