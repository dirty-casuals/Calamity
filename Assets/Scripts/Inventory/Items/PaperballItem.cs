using UnityEngine;
using System.Collections;

public class PaperballItem : DefenseItem {

    public override void UseItem( GameObject player ) {
        if (currentItemState == ItemState.ITEM_IN_USE) {
            return;
        }

        Vector3 playerPosition = player.transform.position;
        Vector3 fireFromPosition = new Vector3( playerPosition.x, 1.0f, playerPosition.z );
        gameObject.transform.position = fireFromPosition;
        spawnVisual.SetActive( false );
        activeVisual.SetActive( true );

        GetComponent<Rigidbody>( ).velocity = (player.transform.forward * defenseItemData.projectileRange);
        defenseItemData.numberOfUses -= defenseItemData.CostOfUse;
        currentItemState = ItemState.ITEM_IN_USE;
        StartCoroutine( HideItemAfterUsePeriod( ) );
    }

    protected override IEnumerator HideItemAfterUsePeriod( ) {
        yield return new WaitForSeconds( defenseItemData.itemDuration );
        ResetItem( );
        ItemHasPerished( );
    }

}
