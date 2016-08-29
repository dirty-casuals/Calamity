using UnityEngine;
using RAIN.Entities;
using UnityStandardAssets.Characters.FirstPerson;
using System;

public class NormalState : PlayerState {

    private EntityRig playerRig;

    public NormalState( GameObject playerBody ) : base( playerBody ) {
        playerMesh = character.GetComponentInChildren<SkinnedMeshRenderer>( );
        playerRig = playerBody.GetComponentInChildren<EntityRig>( );
        character.tag = "Player";
    }

    public override void KnockoutPlayer( ) {
        base.KnockoutPlayer( );
        controller.SetNextStateToMonster( PlayerType.MONSTER );
        playerRig.Entity.IsActive = false;
    }

    public override void RevivePlayer( ) {
        playerRig.Entity.IsActive = true;
        base.RevivePlayer( );
    }
}