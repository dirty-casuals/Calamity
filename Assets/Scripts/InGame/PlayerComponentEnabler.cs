using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.ImageEffects;
using RAIN.Entities;

public class PlayerComponentEnabler : ComponentEnabler {

    public override void OnStartLocalPlayer( ) {
        characterType = "Player";
        SetupComponents( );
    }

    protected override void SetupTypeLists( ) {
        monoBehavioursToEnable = new Type[ ]
        {
            typeof( AnimTriggers ),
            typeof( CharacterStateHandler ),
            typeof( PlayerController ),
            typeof( PlayerInventory ),
            typeof( CalamityFirstPersonController ),
            typeof( BloomOptimized ),
            typeof( CalamityFirstPersonCamera ),
            typeof( EntityRig )
        };

        behavioursToEnable = new Type[ ]
        {
            typeof( Animator ),
            typeof( Camera )
        };

        collidersToEnable = new Type[ ]
        {
            typeof( CharacterController )
        };
    }
}
