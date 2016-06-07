using UnityEngine;

public class KnifeItem : WeaponItem {

    public override void AddItemToPlayer( GameObject player ) {
        PlaceKnifeInPlayersHands( player );
        SetKnifeVisualAspect( );
        itemInPlayerHands = true;
    }

    public override void UseItem( GameObject player ) {
        currentItemState = ItemState.ITEM_IN_USE;
        GetComponent<Animation>( ).Play( );

        Vector3 forward = transform.TransformDirection( Vector3.forward );
        RaycastHit hit;
        if (Physics.Raycast( transform.position, forward, out hit, 10)) {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer( "Player" )) {
                //weaponItemData.numberOfUses -= weaponItemData.CostOfUse;
                //ItemHasPerished( );
            }
        }
    }

    private void PlaceKnifeInPlayersHands( GameObject player ) {
        GameObject playerHands = player.GetComponentInChildren<PlayerHands>( ).gameObject;
        gameObject.transform.position = playerHands.transform.position;
        gameObject.transform.parent = playerHands.transform;
    }

    private void SetKnifeVisualAspect( ) {
        spawnVisual.SetActive( false );
        activeVisual.SetActive( true );
    }

}
