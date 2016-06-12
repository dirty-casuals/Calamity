using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PaperballItem : DefenseItem {
    [SyncVar]
    private GameObject currentPlayer;
    [SyncVar]
    private Rigidbody itemRigidbody;

    public override void AddItemToPlayer( GameObject player ) {
        currentPlayer = player;
        itemRigidbody = GetComponent<Rigidbody>( );
    }

    [Command]
    public override void CmdUseItem( GameObject player ) {
        LaunchPaperBall( );
        defenseItemData.numberOfUses -= defenseItemData.CostOfUse;
        StartCoroutine( HideItemAfterUsePeriod( ) );
    }

    protected override IEnumerator HideItemAfterUsePeriod( ) {
        yield return new WaitForSeconds( defenseItemData.itemDuration );
        NetworkServer.Destroy( gameObject );
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
