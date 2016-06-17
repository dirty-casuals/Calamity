using UnityEngine;
using RAIN.Entities;
using UnityStandardAssets.Characters.FirstPerson;
using System;

public class NormalState : PlayerState {
    
    private CalamityFirstPersonController firstPersonController;
    private SkinnedMeshRenderer playerMesh;
    private EntityRig playerRig;
    private bool playerControllerDisabled;

    public NormalState( GameObject playerBody ) : base( playerBody ) {
        playerMesh = character.GetComponentInChildren<SkinnedMeshRenderer>( );
        playerRig = playerBody.GetComponentInChildren<EntityRig>( );
        character.tag = "Player";
    }

    public override void KnockoutPlayer( ) {
        base.KnockoutPlayer( );
        controller.SetNextState( PlayerType.MONSTER );
        controller.Die( );
        playerRig.Entity.IsActive = false;
    }

    public override void RevivePlayer( ) {
        playerRig.Entity.IsActive = true;
        base.RevivePlayer( );
    }
}