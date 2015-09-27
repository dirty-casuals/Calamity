﻿using UnityEngine;
using System.Collections;

public class AIState : CharacterState {

    public AIState( GameObject playerBody ) {
        character = playerBody;
        characterAnimator = character.GetComponent<Animator>( );
        characterRigidbody = character.GetComponent<Rigidbody>( );
        //controller = playerBody.GetComponent<PlayerController>( );
        character.tag = "AI";
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
