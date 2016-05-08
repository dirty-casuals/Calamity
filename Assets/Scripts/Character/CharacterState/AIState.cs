using UnityEngine;
using System.Collections;
using RAIN.Core;
using RAIN.Entities;
using System;

public class AIState : CharacterState {

    private PlayerController controller;

    public AIState( GameObject playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        controller = playerBody.GetComponent<PlayerController>( );
        //character.tag = "AI";
    }
	
	// Update is called once per frame
	private void Update () {

    }

    public override void KnockoutPlayer( ) {
        base.KnockoutPlayer( );
        AIRig ai = character.GetComponentInChildren<AIRig>( );
        ai.AI.IsActive = false;
        character.GetComponentInChildren<EntityRig>( ).Entity.IsActive = false;
        controller.SetNextState( PlayerType.AI_MONSTER );
    }

    public override void RevivePlayer( ) {
        base.RevivePlayer( );
        AIRig ai = character.GetComponentInChildren<AIRig>( );
        ai.AI.IsActive = true;
        character.GetComponentInChildren<EntityRig>( ).Entity.IsActive = true;
    }
}
