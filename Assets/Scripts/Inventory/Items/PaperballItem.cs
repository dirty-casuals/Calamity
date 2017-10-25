using UnityEngine;
using System.Collections;

public class PaperballItem : DefenseItem {

    private GameObject currentPlayer;
    private Rigidbody itemRigidbody;

    public override void AddItemToPlayer( GameObject player ) {
        currentPlayer = player;
        itemRigidbody = GetComponent<Rigidbody>( );
    }

    public override void UseItem( GameObject player ) {
        LaunchPaperBall( );
        defenseItemData.numberOfUses -= defenseItemData.CostOfUse;
        StartCoroutine( HideItemAfterUsePeriod( ) );
    }

    protected override IEnumerator HideItemAfterUsePeriod( ) {
        yield return new WaitForSeconds( defenseItemData.itemDuration );
        Destroy( gameObject );
    }

    private void LaunchPaperBall( ) {
        PlacePaperballInPlayerHands( );
        itemRigidbody.velocity = (currentPlayer.transform.forward * defenseItemData.projectileRange);
    }

    private void PlacePaperballInPlayerHands( ) {
        GameObject playerHands = currentPlayer.GetComponentInChildren<PlayerHands>( ).gameObject;
        gameObject.transform.position = playerHands.transform.position;
    }
}
