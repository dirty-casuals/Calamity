using UnityEngine;
using System.Collections;
using RAIN.Core;

public class AIState : CharacterState {

    public AIState( GameObject playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        //controller = playerBody.GetComponent<PlayerController>( );
        //character.tag = "AI";
    }
	
	// Update is called once per frame
	void Update () {

    }

    public override void KnockoutPlayer( ) {
        AIRig ai = character.GetComponentInChildren<AIRig>( );
        ai.AI.IsActive = false;
        character.tag = "Monster";
        characterAnimator.SetBool( "KnockOut", true );
    }
}
