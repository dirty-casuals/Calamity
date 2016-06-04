using UnityEngine;
using System.Collections;
using RAIN.Core;
using RAIN.Entities; 
using System;

public class AIState : CharacterState {
    
    public AIState( GameObject playerBody ) : base( playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        //character.tag = "AI";
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
