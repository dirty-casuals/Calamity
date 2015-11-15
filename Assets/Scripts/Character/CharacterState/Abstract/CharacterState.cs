﻿using UnityEngine;
using System.Collections;

public class CharacterState {

    public GameObject character;
    public Animator characterAnimator;
    public Rigidbody characterRigidbody;

    public virtual void PlayerPhysicsUpdate( ) { }

    public virtual void PlayerUpdate( ) { }

    public virtual void PlayerCollisionEnter( Collision collision ) { }

}